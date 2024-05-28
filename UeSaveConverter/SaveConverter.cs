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

using UeSaveGame.Json;

namespace UeSaveConverter
{
	/// <summary>
	/// Runs a save conversion
	/// </summary>
	internal class SaveConverter
	{
		private readonly Options mOptions;
		private readonly Logger mLogger;
		private readonly SaveGameSerializer mSerializer;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="options">Converter options</param>
		/// <param name="logger">A place to log messages and errors</param>
		public SaveConverter(Options options, Logger logger)
		{
			mOptions = options;
			mLogger = logger;
			mSerializer = new();
		}

		/// <summary>
		/// Perform the conversion
		/// </summary>
		/// <returns>The result of the conversion</returns>
		/// <exception cref="ConverterException">The converter encountered an unrecoverable error</exception>
		public SaveConverterResult Run()
		{
			switch (mOptions.InputType)
			{
				case InputType.File:
					return ConvertFile(mOptions.InputPath, mOptions.OutputPath) ? SaveConverterResult.Success : SaveConverterResult.Failure;
				case InputType.Directory:
				case InputType.DirectoryTree:
					{
						int errorCount = 0;
						SearchOption searchOption = mOptions.InputType == InputType.DirectoryTree ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
						string[] files = Directory.GetFiles(mOptions.InputPath, mOptions.FileFilter, searchOption);
						foreach (string inPath in files)
						{
							string outPath = MakeOutputPath(inPath, mOptions.InputPath, mOptions.OutputPath, mOptions.OperatingMode);
							if (!ConvertFile(inPath, outPath))
							{
								++errorCount;
							}
						}
						return errorCount == 0 ? SaveConverterResult.Success : errorCount < files.Length ? SaveConverterResult.PartialFailure : SaveConverterResult.Failure;
					}
				default:
					throw new ConverterException("Internal program error (Invalid input type).", false);
			}
        }

		/// <summary>
		/// Creates an output file path for a conversion
		/// </summary>
		/// <param name="inPath">The path of the conversion input file</param>
		/// <param name="inDir">The conversion input directory</param>
		/// <param name="outDir">The conversion output directory</param>
		/// <param name="mode"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">A path argument is null</exception>
		/// <exception cref="ArgumentException">The value of mode is invalid</exception>
		public static string MakeOutputPath(string inPath, string inDir, string outDir, OperatingMode mode)
		{
			if (inPath is null) throw new ArgumentNullException(nameof(inPath));
			if (inDir is null) throw new ArgumentNullException(nameof(inDir));
			if (outDir is null) throw new ArgumentNullException(nameof(outDir));

			string relativePath = Path.GetRelativePath(inDir, inPath);
			string relativeDir = Path.GetDirectoryName(relativePath)!;
			string fileName = Path.GetFileNameWithoutExtension(relativePath)!;
			switch (mode)
			{
				case OperatingMode.ToSav:
					if (fileName.EndsWith(".sav"))
					{
						// Input extension is .sav.json - just remove .json
						return Path.Combine(outDir, relativeDir, fileName);
					}
					return Path.Combine(outDir, relativeDir, $"{fileName}.sav");
				case OperatingMode.ToJson:
					// Example.sav.json
					return Path.Combine(outDir, $"{relativePath}.json");
				default:
					throw new ArgumentException("Mode must be ToSav or ToJson", nameof(mode));
			}
		}

		private bool ConvertFile(string inPath, string outPath)
		{
			if (Directory.Exists(inPath))
			{
				mLogger.LogError($"Input file \"{inPath}\" is a directory.");
				return false;
			}

			if (!File.Exists(inPath))
			{
				mLogger.LogError($"Input file \"{inPath}\" not found.");
				return false;
			}

			if (!mOptions.AllowOverwrite && File.Exists(outPath))
			{
				mLogger.LogError($"Output file \"{outPath}\" already exists. If you want to overwrite existing files, pass the {Options.OverwriteParam} parameter.");
				return false;
			}

			string outDir = Path.GetDirectoryName(outPath)!;
			if (string.IsNullOrEmpty(outDir))
			{
				mLogger.LogError($"Output path is not valid: {outPath}");
				return false;
			}

#if !DEBUG
			try
			{
#endif
				using FileStream inFile = File.OpenRead(inPath);
				using FileStream outFile = IOUtil.CreateFile(outPath, mLogger);

				mLogger.Log(LogLevel.Information, $"Converting \"{inPath}\" -> \"{outPath}\"");

				switch (mOptions.OperatingMode)
				{
					case OperatingMode.ToSav:
						mSerializer.ConvertFromJson(inFile, outFile);
						break;
					case OperatingMode.ToJson:
						mSerializer.ConvertToJson(inFile, outFile);
						break;
					default:
						throw new ConverterException("Internal program error (Invalid operating mode).", false);
				}

				return true;
#if !DEBUG
			}
			catch (ConverterException ex)
			{
				if (ex.IsRecoverable)
				{
					mLogger.LogError("An error occured when attempting to convert a file.");
					ExceptionHelper.PrintException(ex, mLogger);
					return false;
				}

				// Unrecoverable
				throw;
			}
			catch (Exception ex)
			{
				mLogger.LogError("An error occured when attempting to convert a file.");
				ExceptionHelper.PrintException(ex, mLogger);
				return false;
			}
#endif
		}
	}

	/// <summary>
	/// The result of a save conversion run
	/// </summary>
	internal enum SaveConverterResult
	{
		None,
		Success,
		PartialFailure,
		Failure
	}
}
