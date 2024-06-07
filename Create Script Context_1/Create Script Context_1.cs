using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Linq;

using SLDISMacros.Extensions;
using SLDisMacros.Interfaces;

public class Script
{
	public static void Run(IEngine engine)
	{
		// Load XML Document
		string script = engine.Input.FileContent;
		XDocument xDoc = MacroExtensions.StringToXDocument(script);
		XNamespace ns = "http://www.skyline.be/automation";

		// Get the ScriptParameters
		var parameters = xDoc.Descendants(ns + "ScriptParameter")
			.Select(param => new ScriptParameter
			{
				Id = param.Attribute("id")?.Value,
				Type = param.Attribute("type")?.Value,
				Values = param.Attribute("values")?.Value,
				Description = param.Element(ns + "Description")?.Value,
			});

		// Create ScriptContext class
		var builder = new TabbedStringBuilder();
		builder.AppendLine("namespace Skyline.DataMiner.Automation");
		builder.OpenCurlyBraces();

		// Using namespaces
		builder.AppendLine("using System;");
		builder.AppendLine("using System.Linq;");
		builder.AppendLine();
		builder.AppendLine("using Newtonsoft.Json;");
		builder.AppendLine();
		builder.AppendLine("using Skyline.DataMiner.Automation;");
		builder.AppendLine("using Skyline.DataMiner.Utils.InteractiveAutomationScript;");
		builder.AppendLine();

		// ScriptContext
		builder.AppendLine("public class ScriptContext");
		builder.OpenCurlyBraces();

		// Constructor
		builder.AppendLine("public ScriptContext(IEngine engine)");
		builder.OpenCurlyBraces();
		builder.AppendLine("Engine = engine;");
		builder.AppendLine();
		foreach (var parameter in parameters)
		{
			builder.AppendLine($"{parameter.Description.Sanitize()} = GetScriptParam(\"{parameter.Description}\").Single();");
		}

		builder.CloseCurlyBraces();

		// Properties
		builder.AppendLine();
		builder.AppendLine("public IEngine Engine { get; }");

		foreach (var parameter in parameters)
		{
			builder.AppendLine();
			builder.AppendLine($"public string {parameter.Description.Sanitize()} {{ get; }}");
		}

		// Methods
		// GetScriptParam
		builder.AppendLine();
		builder.AppendLine("private static string[] GetScriptParam(string name)");
		builder.OpenCurlyBraces();

		builder.AppendLine("var rawValue = Engine.GetScriptParam(name)?.Value;");
		builder.AppendLine("if (String.IsNullOrEmpty(rawValue))");
		builder.OpenCurlyBraces();
		builder.AppendLine("throw new ArgumentException($\"Script Param '{name}' cannot be left empty.\");");
		builder.CloseCurlyBraces();
		builder.AppendLine();
		builder.AppendLine("if (IsJsonArray(rawValue))");
		builder.OpenCurlyBraces();
		builder.AppendLine("return JsonConvert.DeserializeObject<string[]>(rawValue);");
		builder.CloseCurlyBraces();
		builder.AppendLine("else");
		builder.OpenCurlyBraces();
		builder.AppendLine("return new[] { rawValue };");
		builder.CloseCurlyBraces();
		builder.CloseCurlyBraces();

		// IsJsonArray
		builder.AppendLine();
		builder.AppendLine("private static bool IsJsonArray(string json)");
		builder.OpenCurlyBraces();
		builder.AppendLine("try");
		builder.OpenCurlyBraces();
		builder.AppendLine("JArray.Parse(json);");
		builder.AppendLine("return true;");
		builder.CloseCurlyBraces();
		builder.AppendLine("catch");
		builder.OpenCurlyBraces();
		builder.AppendLine("return false;");
		builder.CloseCurlyBraces();
		builder.CloseCurlyBraces();

		builder.CloseCurlyBraces();
		builder.CloseCurlyBraces();

		// Copy to Clipboard
		Clipboard.SetText(builder.ToString());
	}
}

public class ScriptParameter
{
	public string Id { get; set; }

	public string Type { get; set; }

	public string Values { get; set; }

	public string Description { get; set; }
}

namespace SLDISMacros.Extensions
{
	using System.Text.RegularExpressions;

	public static class MacroExtensions
	{
		public static string GetSelectedText(this IMacroInput input)
		{
			List<string> selectedTextParts = new List<string>();

			foreach (var selectedSpan in input.SelectedTextSpans)
			{
				selectedTextParts.Add(selectedSpan.Text);
			}

			return String.Join(null, selectedTextParts);
		}

		public static XDocument StringToXDocument(string text)
		{
			XDocument xDoc;

			try
			{
				xDoc = XDocument.Parse(text);

				//bIsTempRootTagAdded = false;
			}
			catch (Exception)
			{
				xDoc = XDocument.Parse("<TempRoot>" + text + "</TempRoot>");

				//bIsTempRootTagAdded = true;
			}

			return xDoc;
		}
	}

	public static class StringExtensions
	{
		public static string Filtered(this string str)
		{
			return Regex.Replace(str, @"\W+", "");   // trim non-word characters
		}

		public static string FirstLetterToLower(this string input)
		{
			switch (input)
			{
				case null:

					throw new ArgumentNullException(nameof(input));

				case "":

					throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));

				default:

					return input.First().ToString().ToLower() + input.Substring(1);
			}
		}

		public static string FirstLetterToUpper(this string input)
		{
			switch (input)
			{
				case null:

					throw new ArgumentNullException(nameof(input));

				case "":

					throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));

				default:

					return input.First().ToString().ToUpper() + input.Substring(1);
			}
		}

		public static string Sanitize(this string input)
		{
			return input
				.Replace(" ", String.Empty)
				.Replace('-', '_')
				.FirstLetterToUpper();
		}
	}

	public class TabbedStringBuilder
	{
		private int tabIndex = 0;
		private readonly StringBuilder sb = new StringBuilder();

		public int Length { get => sb.Length; }

		public TabbedStringBuilder RawAppend(string line)
		{
			sb.Append(line);
			return this;
		}

		public TabbedStringBuilder Append(string line)
		{
			sb.Append(new String('\t', tabIndex));
			sb.Append(line);
			return this;
		}

		public TabbedStringBuilder RawAppendLine(string line)
		{
			sb.Append(line);
			sb.Append(Environment.NewLine);
			return this;
		}

		public TabbedStringBuilder AppendLine()
		{
			sb.Append(Environment.NewLine);
			return this;
		}

		public TabbedStringBuilder AppendLine(string line)
		{
			sb.Append(new String('\t', tabIndex));
			sb.Append(line);
			sb.Append(Environment.NewLine);
			return this;
		}

		public TabbedStringBuilder OpenCurlyBraces()
		{
			sb.Append(new String('\t', tabIndex));
			sb.Append("{");
			sb.Append(Environment.NewLine);
			tabIndex++;
			return this;
		}

		public TabbedStringBuilder CloseCurlyBraces(bool newLine = true)
		{
			tabIndex--;
			sb.Append(new String('\t', tabIndex));
			sb.Append("}");
			if (newLine)
				sb.Append(Environment.NewLine);
			return this;
		}

		public TabbedStringBuilder Remove(int startIndex, int length)
		{
			sb.Remove(startIndex, length);
			return this;
		}

		public override string ToString()
		{
			return sb.ToString();
		}
	}
}