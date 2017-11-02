namespace CustomVisionApiDemo.Train
{
    using Microsoft.Cognitive.CustomVision;
    using Microsoft.Cognitive.CustomVision.Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Threading;

    class Program
    {
        private static readonly string DemoProjectName = "TeamDemo";

        static void Main(string[] args)
        {
            var taggedImages = JsonConvert.DeserializeObject<IEnumerable<ClassData>>(File.ReadAllText("imagesData.json"));

            TrainingApiCredentials trainingCredentials = new TrainingApiCredentials(
                ConfigurationManager.ConnectionStrings["CustomVision.TrainingKey"].ConnectionString);

            TrainingApi trainingApi = new TrainingApi(trainingCredentials);

            var demoProject = RecreateProject(trainingApi);

            var tagMap = new Dictionary<string, Guid>();

            Console.WriteLine("Populating Images...");
            foreach (var taggedImageClass in taggedImages)
            {
                foreach (var tag in taggedImageClass.Tags)
                {
                    if (!tagMap.ContainsKey(tag))
                    {
                        tagMap[tag] = trainingApi.CreateTag(demoProject.Id, tag).Id;
                        Console.WriteLine($"Created tag: {tag} with id: {tagMap[tag]}");
                    }
                }

                var submissionResult = trainingApi.CreateImagesFromUrls(
                      demoProject.Id,
                      new ImageUrlCreateBatch()
                      {
                          Urls = taggedImageClass.Urls.ToList(),
                          TagIds = taggedImageClass.Tags.Select(x => tagMap[x]).ToList()
                      }
                  );

                Console.WriteLine($"Populated {submissionResult.Images.Count()} images, with tags {string.Join(" ", taggedImageClass.Tags)}. Failed: {taggedImageClass.Urls.Count() - submissionResult.Images.Count()}");
            }

            Console.WriteLine("Let's train the model.");
            var iteration = trainingApi.TrainProject(demoProject.Id);

            // The returned iteration will be in progress, and can be queried periodically to see when it has completed
            while (iteration.Status == "Training")
            {
                Thread.Sleep(1000);

                // Re-query the iteration to get it's updated status
                iteration = trainingApi.GetIteration(demoProject.Id, iteration.Id);
            }

            // The iteration is now trained. Make it the default project endpoint
            iteration.IsDefault = true;
            trainingApi.UpdateIteration(demoProject.Id, iteration.Id, iteration);
            Console.WriteLine("Training complete.");

            var account = trainingApi.GetAccountInfo();
            var predictionKey = account.Keys.PredictionKeys.PrimaryKey;

            // Create a prediction endpoint, passing in a prediction credentials object that contains the obtained prediction key
            PredictionEndpointCredentials predictionEndpointCredentials = new PredictionEndpointCredentials(predictionKey);
            PredictionEndpoint endpoint = new PredictionEndpoint(predictionEndpointCredentials);

            // Make a prediction against the new project
            Console.WriteLine("Making a prediction:");
            var result = endpoint.PredictImage(demoProject.Id, File.Open(@"mostropolis-test.jpg", FileMode.Open));

            Console.WriteLine("Prediction Results");
            // Loop over each prediction and write out the results
            foreach (var c in result.Predictions)
            {
                Console.WriteLine($"\t{c.Tag}: {c.Probability:P1}");
            }

            Console.ReadKey();
        }

        private static ProjectModel RecreateProject(TrainingApi api)
        {
            Console.WriteLine("Looking for existing project...");
            var demoProject = api
                .GetProjects()
                .FirstOrDefault(x => x.Name == DemoProjectName);

            if (demoProject != null)
            {
                Console.WriteLine("DemoProject exists. Cleaning up.");
                api.DeleteProject(demoProject.Id);
            }

            Console.WriteLine($"Creating project '{DemoProjectName}'");
            demoProject = api.CreateProject(DemoProjectName, "Demo project for Tech coffee talk");

            return demoProject;
        }
    }
}
