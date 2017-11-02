# This is the Demo project for [Microsoft's CustomVision Azure API](https://customvision.ai/).

[![Build status](https://ci.appveyor.com/api/projects/status/b15jagf96mqcnh4p?svg=true)](https://ci.appveyor.com/project/wovas/ms-customvisionapidemo-o1qyy)

We use smal results DataSet from [Questolog](https://questolog.com/) to train the model.

After training service will be able to classify results photos based on address, city and escape quest name.

To make the code running for you: 
1. Create account on [CustomVision](https://customvision.ai/) website.
2. Get Training API Code on [settings page](https://www.customvision.ai/projects#/settings) and copy it to the clipboard.
3. Paste your code to the App.config. (Replace *{PLACE_YOUR_API_KEY_HERE}* with actual value from clipboard).