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

using System.Runtime.Serialization;

namespace UeSaveConverter
{
	/// <summary>
	/// Exception thrown when there is an error during save file conversion
	/// </summary>
	internal class ConverterException : Exception
	{
		/// <summary>
		/// Whether conversion of further files can be attempted
		/// </summary>
		public bool IsRecoverable { get; }

		public ConverterException(bool isRecoverable)
		{
			IsRecoverable = isRecoverable;
		}

		public ConverterException(string? message, bool isRecoverable)
			: base(message)
		{
			IsRecoverable = isRecoverable;
		}

		public ConverterException(string? message, bool isRecoverable, Exception? innerException)
			: base(message, innerException)
		{
			IsRecoverable = isRecoverable;
		}

		protected ConverterException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			IsRecoverable = (bool)(info.GetValue(nameof(IsRecoverable), typeof(bool)) ?? false);
		}
	}
}
