// Copyright 2024 Crystal Ferrai
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace UeSaveConverter
{
	/// <summary>
	/// Program options derived from command line
	/// </summary>
	internal class Options
	{
		/// <summary>
		/// Path to the input file or directory
		/// </summary>
		public string InputPath { get; private set; }

		/// <summary>
		/// Path to the output file or directory
		/// </summary>
		public string OutputPath { get; private set; }

		/// <summary>
		/// The conversion operating mode
		/// </summary>
		public OperatingMode OperatingMode { get; private set; }

		/// <summary>
		/// Whether the input path represents a file or directory
		/// </summary>
		public InputType InputType { get; private set; }

		/// <summary>
		/// The file system filter to use when locating files if the input path is a directory
		/// </summary>
		public string FileFilter { get; private set; }

		/// <summary>
		/// Whether to allow overwriting output files
		/// </summary>
		public bool AllowOverwrite { get; private set; }

		public const string OverwriteParam = "--overwrite";

		private Options()
		{
			// TryParseCommandLine ensures string properties will not be null
			InputPath = null!;
			OutputPath = null!;
			OperatingMode = OperatingMode.Invalid;
			InputType = InputType.Invalid;
			FileFilter = null!;
			AllowOverwrite = false;
		}

		/// <summary>
		/// Create an Options instance from command line arguments
		/// </summary>
		/// <param name="args">The command line arguments to parse</param>
		/// <param name="logger">For logging parse errors</param>
		/// <param name="options">Outputs the options if parsing is successful</param>
		/// <returns>Whether parsing was successful</returns>
		public static bool TryParseCommandLine(string[] args, Logger logger, [NotNullWhen(true)] out Options? options)
		{
			if (args.Length == 0)
			{
				options = null;
				return false;
			}

			Options instance = new();

			int positionalArgIndex = 0;
			bool includeSubdirs = false;

			for (int i = 0; i < args.Length; ++i)
			{
				if (args[i].StartsWith("--"))
				{
					// Explicit arg
					string argValue = args[i][2..];
					switch (argValue)
					{
						case "to-json":
							if (instance.OperatingMode != OperatingMode.Invalid)
							{
								logger.LogError("Cannot combine arguments --to-json and --to-sav.");
								options = null;
								return false;
							}
							instance.OperatingMode = OperatingMode.ToJson;
							break;
						case "to-sav":
							if (instance.OperatingMode != OperatingMode.Invalid)
							{
								logger.LogError("Cannot combine arguments --to-json and --to-sav.");
								options = null;
								return false;
							}
							instance.OperatingMode = OperatingMode.ToSav;
							break;
						case "include-subdirectories":
							includeSubdirs = true;
							break;
						case "file-filter":
							{
								string? filter = null;
								if (i < args.Length - 1)
								{
									string next = args[i + 1];
									if (!next.StartsWith("--"))
									{
										filter = next;
									}
								}

								if (filter is null)
								{
									logger.LogError($"");
									options = null;
									return false;
								}

								instance.FileFilter = filter;
								++i;
							}
							break;
						case "overwrite":
							instance.AllowOverwrite = true;
							break;
						default:
							logger.LogError($"Unrecognized argument '{args[i]}'");
							options = null;
							return false;
					}
				}
				else
				{
					// Positional arg
					switch (positionalArgIndex)
					{
						case 0:
							instance.InputPath = args[i];
							break;
						case 1:
							instance.OutputPath = args[i];
							break;
						default:
							logger.LogError("Too many positional arguments.");
							options = null;
							return false;
					}
					++positionalArgIndex;
				}
			}

			bool isDirectoryMode = Directory.Exists(instance.InputPath);

			if (isDirectoryMode)
			{
				instance.InputType = includeSubdirs ? InputType.DirectoryTree : InputType.Directory;
				if (instance.OutputPath is null)
				{
					logger.LogError("Must specify output path when input path is a directory.");
					options = null;
					return false;
				}
			}
			else // File mode
			{
				if (!File.Exists(instance.InputPath))
				{
					logger.LogError($"Input path '{instance.InputPath}' does not exist.");
					options = null;
					return false;
				}
				instance.InputType = InputType.File;
			}

			if (instance.OperatingMode == OperatingMode.Invalid && instance.InputType != InputType.File)
			{
				logger.LogError("Must specify --to-json or --to-sav when input is a directory.");
				options = null;
				return false;
			}

			if (instance.OperatingMode == OperatingMode.Invalid)
			{
				string extension = Path.GetExtension(instance.InputPath)!;
				switch (extension)
				{
					case ".sav":
						instance.OperatingMode = OperatingMode.ToJson;
						break;
					case ".json":
						instance.OperatingMode = OperatingMode.ToSav;
						break;
					default:
						logger.LogError("Cannot determine input file type from file extension. Please specify '--to-json' or '--to-sav'.");
						options = null;
						return false;
				}
			}

			if (!isDirectoryMode && instance.OutputPath is null)
			{
				string directory = Path.GetDirectoryName(instance.InputPath)!;
				instance.OutputPath = SaveConverter.MakeOutputPath(instance.InputPath, directory, directory, instance.OperatingMode);
			}

			if (instance.FileFilter is null)
			{
				instance.FileFilter = instance.OperatingMode == OperatingMode.ToSav ? "*.sav.json" : "*.sav";
			}

			options = instance;
			return true;
		}

		/// <summary>
		/// Prints how to use the program, including all possible command line arguments
		/// </summary>
		/// <param name="logger">Where the message will be printed</param>
		/// <param name="indent">Every line of the output will be prefixed with this</param>
		public static void PrintUsage(Logger logger, string indent = "")
		{
			string? programName = Assembly.GetExecutingAssembly().GetName().Name;
			logger.Log(LogLevel.Important, $"{indent}Usage: {programName} [[options]] [input path] [output path]");
			logger.LogEmptyLine(LogLevel.Important);
			logger.Log(LogLevel.Important, $"{indent}  input path   A file or directory containing files to convert to or from json.");
			logger.LogEmptyLine(LogLevel.Important);
			logger.Log(LogLevel.Important, $"{indent}  output path  A file or directory that will receive the converted file(s). Optional if");
			logger.Log(LogLevel.Important, $"{indent}               input is a file. If not specified, output will be placed next to input.");
			logger.LogEmptyLine(LogLevel.Important);
			logger.Log(LogLevel.Important, $"{indent}Options");
			logger.LogEmptyLine(LogLevel.Important);
			logger.Log(LogLevel.Important, $"{indent}  --to-json                 Convert sav to json. Implied if input is a .sav file.");
			logger.LogEmptyLine(LogLevel.Important);
			logger.Log(LogLevel.Important, $"{indent}  --to-sav                  Convert json to sav. Implied if input is a .json file.");
			logger.LogEmptyLine(LogLevel.Important);
			logger.Log(LogLevel.Important, $"{indent}  --include-subdirectories  If input is a directory, also process subdirectories.");
			logger.LogEmptyLine(LogLevel.Important);
			logger.Log(LogLevel.Important, $"{indent}  --file-filter [filter]    If input is a directory, use this filter to match files.");
			logger.Log(LogLevel.Important, $"{indent}                            Default is *.sav for to-json and *.sav.json for to-sav.");
			logger.LogEmptyLine(LogLevel.Important);
			logger.Log(LogLevel.Important, $"{indent}  --overwrite               Allow overwriting existing output file(s).");
		}
	}

	/// <summary>
	/// The conversion operating mode
	/// </summary>
	internal enum OperatingMode
	{
		Invalid,
		/// <summary>
		/// Convert from Sav to Json
		/// </summary>
		ToJson,
		/// <summary>
		/// Convert from Json to Sav
		/// </summary>
		ToSav
	}

	/// <summary>
	/// The type of file system object on which a conversion will operate
	/// </summary>
	internal enum InputType
	{
		Invalid,
		/// <summary>
		/// A single file
		/// </summary>
		File,
		/// <summary>
		/// All files within a directory, top level only
		/// </summary>
		Directory,
		/// <summary>
		/// All files with a directory and its subdirectories
		/// </summary>
		DirectoryTree
	}
}
