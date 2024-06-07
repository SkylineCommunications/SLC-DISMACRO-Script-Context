# SLC-DISMACRO-Script-Context

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=SkylineCommunications_SLC-DISMACRO-Script-Context&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=SkylineCommunications_SLC-DISMACRO-Script-Context)

Contains a DIS Macro to generate a ScriptContext class that retrieves all the ScriptParameter tags and parses them into properties.

> [!Note]
> This generated class can be used as a start. For example all the properties are generated as "string" because the ScriptParameter attribute "type" only supports string.
> You can then edit this generated class and change your properties to be for example an integer, or an enum, by updating the constructor.

## Installation

You can do a manual install by downloading the .xml file from the releases.
1. Go to [releases](https://github.com/SkylineCommunications/SLC-DISMACRO-Script-Context/releases)
1. Download the .xml file from the latest version
1. Go to Visual Studio -> Extension -> DIS -> Tool Windows -> DIS Macros
1. In the DIS Macros windows right click "My Macros"
1. Click on "Import macros...", this will open a windows explorer dialog
1. Go to the file you just downloaded.

## How to use

1. Open an Automation Script in visual studio. 
1. Open the Automation Script's .xml file.
1. Open the DIS Macros tool window.
1. Select the "Create Script Context" macro
1. Press run at the top of the DIS Macros window.
1. The ScriptContext class will be copied to your Clipboard.
1. Create a new file in your Automation Script Solution, for example: "ScriptContext.cs".
1. Paste the generated code.

## Example

```csharp
namespace Skyline.DataMiner.Automation
{
	using System;
	using System.Linq;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ScriptContext
	{
		public ScriptContext(IEngine engine)
		{
			Engine = engine;

			AgentId = GetScriptParam("Agent Id").Single();
			ElementId = GetScriptParam("Agent Id").Single();
			RepoName = GetScriptParam("Agent Id").Single();
			RepoOwner = GetScriptParam("Agent Id").Single();
		}

		public IEngine Engine { get; }

		public string AgentId { get; }

		public string ElementId { get; }

		public string RepoName { get; }

		public string RepoOwner { get; }

		private static string[] GetScriptParam(string name)
		{
			var rawValue = Engine.GetScriptParam(name)?.Value;
			if (String.IsNullOrEmpty(rawValue))
			{
				throw new ArgumentException($"Script Param '{name}' cannot be left empty.");
			}

			if (IsJsonArray(rawValue))
			{
				return JsonConvert.DeserializeObject<string[]>(rawValue);
			}
			else
			{
				return new[] { rawValue };
			}
		}

		private static bool IsJsonArray(string json)
		{
			try
			{
				JArray.Parse(json);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
```

## About DataMiner

DataMiner is a transformational platform that provides vendor-independent control and monitoring of devices and services. 
Out of the box and by design, it addresses key challenges such as security, complexity, multi-cloud, and much more. 
It has a pronounced open architecture and powerful capabilities enabling users to evolve easily and continuously.

The foundation of DataMiner is its powerful and versatile data acquisition and control layer. 
With DataMiner, there are no restrictions to what data users can access. 
Data sources may reside on premises, in the cloud, or in a hybrid setup.

A unique catalog of 7000+ connectors already exist. 
In addition, you can leverage DataMiner Development Packages to build you own connectors (also known as "protocols" or "drivers").

> **Note**
> See also: [About DataMiner](https://aka.dataminer.services/about-dataminer).

## About Skyline Communications

At Skyline Communications, we deal in world-class solutions that are deployed by leading companies around the globe. 
Check out [our proven track record](https://aka.dataminer.services/about-skyline) and see how we make our customers' lives easier by empowering them to take their operations to the next level.