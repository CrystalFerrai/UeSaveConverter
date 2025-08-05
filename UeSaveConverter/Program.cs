// Copyright 2025 Crystal Ferrai
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

namespace UeSaveConverter
{
	internal class Program
	{
		/// <summary>
		/// Program entry point
		/// </summary>
		private static int Main(string[] args)
		{
			ConsoleLogger logger = new();

			Options? options;
			if (!Options.TryParseCommandLine(args, logger, out options))
			{
				Options.PrintUsage(logger);
				return OnExit(1);
			}

			SaveConverter converter = new(options, logger);
#if !DEBUG
			try
			{
#endif
				SaveConverterResult result = converter.Run();
				switch (result)
				{
					case SaveConverterResult.Success:
						logger.Log(LogLevel.Important, "Conversion complete.");
						return OnExit(0);
					case SaveConverterResult.PartialFailure:
						logger.LogError("Some files failed to be converted.");
						return OnExit(1);
					case SaveConverterResult.Failure:
						logger.LogError("Conversion failed.");
						return OnExit(1);
					default:
						throw new ApplicationException("Internal program error (invalid conversion result).");
				}
#if !DEBUG
			}
			catch (Exception ex)
			{
				logger.LogError("A fatal error occurred during conversion.");
				ExceptionHelper.PrintException(ex, logger);
				return OnExit(1);
			}
#endif
		}

		private static int OnExit(int code)
		{
			if (System.Diagnostics.Debugger.IsAttached)
			{
				Console.Out.WriteLine("[Debug] Press a key to exit...");
				Console.ReadKey(true);
			}
			return code;
		}
	}
}
