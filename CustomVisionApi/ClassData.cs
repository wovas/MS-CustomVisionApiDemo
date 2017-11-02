namespace CustomVisionApiDemo.Train
{
    using System.Collections.Generic;

    /// <summary>
    /// Class for Image Classes data serialization.
    /// </summary>
    public class ClassData
    {
        public IEnumerable<string> Urls { get; set; }

        public IEnumerable<string> Tags { get; set; }
    }
}
