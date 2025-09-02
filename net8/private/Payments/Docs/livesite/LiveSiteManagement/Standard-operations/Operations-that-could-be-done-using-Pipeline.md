# Operations that could be done using Pipeline

<span style="background-color:white">We could use PowerShell script to execute scripts without SRE&#39;s present like app service restart/removing bad instance</span>

<span style="background-color:white">Execution wiki&#160;</span>[https://aka.ms/ps\_execution](https://nam06.safelinks.protection.outlook.com/?url=https%3A%2F%2Faka.ms%2Fps_execution&amp;data=05%7C01%7Cwwei%40microsoft.com%7Ca74a934250bc42f784a008da7fa84965%7C72f988bf86f141af91ab2d7cd011db47%7C1%7C0%7C637962658015572750%7CUnknown%7CTWFpbGZsb3d8eyJWIjoiMC4wLjAwMDAiLCJQIjoiV2luMzIiLCJBTiI6Ik1haWwiLCJXVCI6Mn0%3D%7C3000%7C%7C%7C&amp;sdata=H%2FALoTfSMOsUMZukt4kovBh2C1Kd%2BF0FLyNyt0JMtyM%3D&amp;reserved=0)

In order to execute pipeline, developer(s) needs to:

1. Create a PowerShell script to be executed. If script needs to be run on specific Azure VM(s), create a text file with VM list. then compress files into zip format
2. Create a service change and attach zip file.
3. Get technical  approval
4. Get deployment approval
5. Run Pipeline and point to Service changes that was created earlier
6. Check Service change attachment to review logs

- <span style="background-color:white">INT Pipeline:&#160;</span>[Pipelines - Runs for SSD-STORECORE-PST-PX-INT.yml (visualstudio.com)](https://microsoft.visualstudio.com/Universal%20Store/_build?definitionId=87387)
- <span style="background-color:white">PROD pipeline&#160;</span>[Pipelines - Runs for SSD-STORECORE-PST-PX-PROD.yml (visualstudio.com)](https://microsoft.visualstudio.com/Universal%20Store/_build?definitionId=87254)

-