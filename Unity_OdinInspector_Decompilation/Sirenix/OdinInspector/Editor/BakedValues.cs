using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.Serialization;
using Sirenix.Serialization.Utilities;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public static class BakedValues
	{
		public enum BakedValueType
		{
			Invalid,
			String,
			Int,
			DateTime,
			Bool
		}

		private static Dictionary<string, object> loadedValues;

		private static byte[] LoadBakedValueBytes()
		{
			string path = SirenixAssetPaths.OdinPath + "/Assets/Editor/ConfigData.bytes";
			if (!File.Exists(path))
			{
				return null;
			}
			return File.ReadAllBytes(path);
		}

		private static Dictionary<string, object> ReadValues(byte[] bytes)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			using MemoryStream memoryStream = new MemoryStream(bytes);
			using Cache<BinaryDataReader> cache = Cache<BinaryDataReader>.Claim();
			BinaryDataReader value = cache.Value;
			value.PrepareNewSerializationSession();
			value.Stream = memoryStream;
			memoryStream.Position = 0L;
			if (!value.EnterArray(out var length))
			{
				throw new ArgumentException();
			}
			for (int i = 0; i < length; i++)
			{
				if (!value.EnterNode(out var _))
				{
					throw new ArgumentException();
				}
				string currentNodeName = value.CurrentNodeName;
				if (string.IsNullOrEmpty(currentNodeName))
				{
					throw new ArgumentException();
				}
				if (!value.ReadInt32(out var value2))
				{
					throw new ArgumentException();
				}
				if (!value.ReadInt32(out var value3))
				{
					throw new ArgumentException();
				}
				if (!value.ReadInt32(out var value4))
				{
					throw new ArgumentException();
				}
				if (!value.ReadString(out var value5))
				{
					throw new ArgumentException();
				}
				if (!value.ExitNode())
				{
					throw new ArgumentException();
				}
				object valueFromData = GetValueFromData(value5.Substring(value3, value4), (BakedValueType)value2);
				dictionary.Add(currentNodeName, valueFromData);
			}
			if (!value.ExitArray())
			{
				throw new ArgumentException();
			}
			return dictionary;
		}

		private unsafe static object GetValueFromData(string data, BakedValueType type)
		{
			switch (type)
			{
			case BakedValueType.String:
				return data.Trim();
			case BakedValueType.Int:
				if (data.Length != 2)
				{
					throw new ArgumentException();
				}
				fixed (char* ptr = data)
				{
					return *(int*)ptr;
				}
			case BakedValueType.DateTime:
				if (data.Length != 4)
				{
					throw new ArgumentException();
				}
				fixed (char* ptr2 = data)
				{
					return DateTime.FromBinary(*(long*)ptr2);
				}
			case BakedValueType.Bool:
				if (data.Length != 1)
				{
					throw new ArgumentException();
				}
				return data[0] != '\0';
			default:
				throw new ArgumentException();
			}
		}

		public static bool TryGetBakedValue<T>(string name, out T value)
		{
			if (loadedValues == null)
			{
				try
				{
					byte[] array = LoadBakedValueBytes();
					if (array != null)
					{
						loadedValues = ReadValues(array);
					}
				}
				catch (Exception innerException)
				{
					Debug.LogException(new Exception("Could not load baked config values from file.", innerException));
					loadedValues = new Dictionary<string, object>();
				}
			}
			if (!loadedValues.TryGetValue(name, out var value2))
			{
				value = default(T);
				return false;
			}
			if (value2 == null)
			{
				value = default(T);
				return typeof(T) == typeof(string);
			}
			if (!(value2 is T))
			{
				value = default(T);
				return false;
			}
			value = (T)value2;
			return true;
		}
	}
}
