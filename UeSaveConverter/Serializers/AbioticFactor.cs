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

using Newtonsoft.Json;
using UeSaveGame;
using UeSaveGame.Json;
using UeSaveGame.Util;

// This file contains custom serializers needed to parse save files for the game Abiotic Factor

namespace UeSaveConverter.Serializers
{
	[SaveClassPath("/Game/Blueprints/Saves/Abiotic_WorldSave.Abiotic_WorldSave_C")]
	[SaveClassPath("/Game/Blueprints/Saves/Abiotic_WorldMetadataSave.Abiotic_WorldMetadataSave_C")]
	internal class Abiotic_WorldSave : SaveClassBase
	{
		private static readonly FString VersionPropertyName = new("ABF_SAVE_VERSION");

		private int mDataLength;

		public int Version { get; set; }

		public int Unknown { get; set; }

		public override bool HasCustomHeader => true;

		public override long GetHeaderSize()
		{
			return Version > 0 ? VersionPropertyName.Length + 5 + 4 + 4 + 4 : 0;
		}

		public override void DeserializeHeader(BinaryReader reader)
		{
			long startPos = reader.BaseStream.Position;

			FString? versionPropertyName = reader.ReadUnrealString();
			if (versionPropertyName is null || versionPropertyName != VersionPropertyName)
			{
				// Might be old version of save file before custom header was added
				reader.BaseStream.Seek(startPos, SeekOrigin.Begin);
				return;
			}

			Version = reader.ReadInt32();
			Unknown = reader.ReadInt32();
			mDataLength = reader.ReadInt32();
		}

		public override void SerializeHeader(BinaryWriter writer, long dataLength)
		{
			if (Version == 0) return;

			mDataLength = (int)dataLength;

			writer.WriteUnrealString(VersionPropertyName);
			writer.Write(Version);
			writer.Write(Unknown);
			writer.Write(mDataLength);
		}
	}

	internal class Abiotic_WorldSaveSerializer : SaveClassSerializerBase<Abiotic_WorldSave>
	{
		public override bool HasCustomHeader => true;

		public override void HeaderFromJson(JsonReader reader, Abiotic_WorldSave saveClass)
		{
			while (reader.Read())
			{
				if (reader.TokenType == JsonToken.PropertyName)
				{
					switch (reader.Value)
					{
						case nameof(Abiotic_WorldSave.Version):
							saveClass.Version = reader.ReadAsInt32() ?? 0;
							break;
						case nameof(Abiotic_WorldSave.Unknown):
							saveClass.Unknown = reader.ReadAsInt32() ?? 0;
							break;
					}
				}
			}
		}

		public override void HeaderToJson(JsonWriter writer, Abiotic_WorldSave saveClass)
		{
			writer.WriteStartObject();

			writer.WritePropertyName(nameof(Abiotic_WorldSave.Version));
			writer.WriteValue(saveClass.Version);

			writer.WritePropertyName(nameof(Abiotic_WorldSave.Unknown));
			writer.WriteValue(saveClass.Unknown);

			writer.WriteEndObject();
		}
	}

	[SaveClassPath("/Game/Blueprints/Saves/Abiotic_CharacterSave.Abiotic_CharacterSave_C")]
	internal class Abiotic_PlayerSave : SaveClassBase
	{
		private int mDataLength;

		public int Version { get; set; }

		public override bool HasCustomHeader => true;

		public override long GetHeaderSize()
		{
			return Version > 0 ? 4 + 4 : 0;
		}

		public override void DeserializeHeader(BinaryReader reader)
		{
			long startPos = reader.BaseStream.Position;

			Version = reader.ReadInt32();
			mDataLength = reader.ReadInt32();

			if (mDataLength == 1918986307)
			{
				// Old save file format from before custom headers were added
				Version = 0;
				mDataLength = 0;

				reader.BaseStream.Seek(startPos, SeekOrigin.Begin);
			}
		}

		public override void SerializeHeader(BinaryWriter writer, long dataLength)
		{
			if (Version == 0) return;

			mDataLength = (int)dataLength;

			writer.Write(Version);
			writer.Write(mDataLength);
		}
	}

	internal class Abiotic_PlayerSaveSerializer : SaveClassSerializerBase<Abiotic_PlayerSave>
	{
		public override bool HasCustomHeader => true;

		public override void HeaderFromJson(JsonReader reader, Abiotic_PlayerSave saveClass)
		{
			while (reader.Read())
			{
				if (reader.TokenType == JsonToken.PropertyName)
				{
					switch (reader.Value)
					{
						case nameof(Abiotic_PlayerSave.Version):
							saveClass.Version = reader.ReadAsInt32() ?? 0;
							break;
					}
				}
			}
		}

		public override void HeaderToJson(JsonWriter writer, Abiotic_PlayerSave saveClass)
		{
			writer.WriteStartObject();

			writer.WritePropertyName(nameof(Abiotic_WorldSave.Version));
			writer.WriteValue(saveClass.Version);

			writer.WriteEndObject();
		}
	}
}
