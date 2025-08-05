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

// This file contains custom serializers needed to parse save files for the game Parcel Simulator

namespace UeSaveConverter.Serializers
{
	internal class BlobStruct : BaseStructData
	{
		public byte[]? Value;

		public override IEnumerable<string> StructTypes
		{
			get { yield return "F_ParcelCriterion_Contents"; }
		}

		public override void Deserialize(BinaryReader reader, int size, PackageVersion engineVersion)
		{
			Value = reader.ReadBytes(size);
		}

		public override int Serialize(BinaryWriter writer, PackageVersion engineVersion)
		{
			if (Value is null)
			{
				return 0;
			}

			writer.Write(Value);
			return Value.Length;
		}
	}

	internal class BlobStructSerializer : StructDataSerializerBase
	{
		public override IEnumerable<string> StructTypes
		{
			get { yield return "F_ParcelCriterion_Contents"; }
		}

		public override IStructData? FromJson(JsonReader reader)
		{
			BlobStruct blob = new();
			string? value = reader.Value is string sv ? sv : reader.ReadAsString();
			if (value is not null)
			{
				blob.Value = Convert.FromBase64String(value);
			}
			return blob;
		}

		public override void ToJson(IStructData? data, JsonWriter writer)
		{
			BlobStruct? blob = data as BlobStruct;
			if (blob is null) throw new ArgumentException("Unexpected data type", nameof(data));

			if (blob.Value is null)
			{
				writer.WriteNull();
			}
			else
			{
				writer.WriteValue(Convert.ToBase64String(blob.Value));
			}
		}
	}
}
