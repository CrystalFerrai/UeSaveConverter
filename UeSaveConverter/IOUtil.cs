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
	/// <summary>
	/// Utility for File IO operations
	/// </summary>
	internal static class IOUtil
	{
		/// <summary>
		/// Attempts to create a file. Will retry indefinitely until it succeeds or the application terminates
		/// </summary>
		/// <param name="path">Path to the file to create</param>
		/// <param name="logger">Used to log messages about file creation issues for the user to see</param>
		public static FileStream CreateFile(string path, Logger logger)
		{
			do
			{
				try
				{
					Directory.CreateDirectory(Path.GetDirectoryName(path)!);
					return File.Create(path);
				}
				catch (IOException ex)
				{
					logger.Log(LogLevel.Error, $"{ex.Message} Press Ctrl+C to abort or any other key to try again.");
					Console.ReadKey(true);
					logger.Log(LogLevel.Important, "Retrying...");
				}
			} while (true);
		}

	}
}
