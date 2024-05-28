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

namespace UeSaveConverter
{
	/// <summary>
	/// Utility for working with exceptions
	/// </summary>
	internal static class ExceptionHelper
	{
		/// <summary>
		/// Prints summary information (type and message) about an exception and all nested inner exception
		/// </summary>
		/// <param name="ex">The exception to print information about</param>
		/// <param name="logger">Where to print the message</param>
		public static void PrintException(Exception ex, Logger logger)
		{
			logger.LogError($"[{ex.GetType().FullName}] {ex.Message}");
			
			for (Exception? inner = ex.InnerException; inner is not null; inner = inner.InnerException)
			{
				logger.LogError($"(Caused by) [{ex.GetType().FullName}] {ex.Message}");
			}
		}
	}
}
