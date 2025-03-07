using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Modules
{
	public class ModuleManifest
	{
		private struct LineData
		{
			public string Key;

			public string Data;

			public static LineData Parse(string line)
			{
				LineData result;
				if (line.Length > 2 && line[0] == ' ' && line[1] == ' ')
				{
					result = default(LineData);
					result.Key = null;
					result.Data = line.TrimEnd();
					return result;
				}
				int num = line.IndexOf(':');
				if (num < 0)
				{
					result = default(LineData);
					result.Key = null;
					result.Data = line.Trim();
					return result;
				}
				result = default(LineData);
				result.Key = line.Substring(0, num).Trim();
				result.Data = ((num + 1 == line.Length) ? "" : line.Substring(num + 1).Trim());
				return result;
			}

			public override string ToString()
			{
				if (Key == null)
				{
					return "{ " + Data + " }";
				}
				return "{ " + Key + ", " + Data + " }";
			}
		}

		public string ID;

		public Version Version;

		public List<string> Files;

		public static void Save(string path, ModuleManifest manifest)
		{
			List<string> list = new List<string>(manifest.Files.Count + 4)
			{
				"ManifestVersion: 1",
				"ModuleID: " + manifest.ID,
				"ModuleVersion: " + manifest.Version.ToString(),
				"ModuleFiles:"
			};
			foreach (string file in manifest.Files)
			{
				list.Add("  " + file);
			}
			File.WriteAllLines(path, list.ToArray());
		}

		public static ModuleManifest Load(string path)
		{
			if (!File.Exists(path))
			{
				return null;
			}
			string[] array = File.ReadAllLines(path);
			if (array.Length == 0)
			{
				return null;
			}
			if (array[0].TrimEnd() == "ManifestVersion: 1")
			{
				return Load_Version1(array);
			}
			Debug.LogError((object)("Cannot read Odin module manifest file with version '" + array[0] + "'."));
			return null;
		}

		private static ModuleManifest Load_Version1(string[] lines)
		{
			LineData[] array = (from n in lines
				where !n.StartsWith("#") && !string.IsNullOrEmpty(n.Trim())
				select LineData.Parse(n)).ToArray();
			if (array.Length < 4)
			{
				return null;
			}
			ModuleManifest moduleManifest = new ModuleManifest
			{
				Files = new List<string>()
			};
			for (int i = 1; i < array.Length; i++)
			{
				LineData lineData = array[i];
				switch (lineData.Key)
				{
				case "ModuleID":
					moduleManifest.ID = lineData.Data;
					break;
				case "ModuleVersion":
					moduleManifest.Version = new Version(lineData.Data);
					break;
				case "ModuleFiles":
				{
					int num = i + 1;
					while (num < array.Length)
					{
						LineData lineData2 = array[num++];
						if (lineData2.Key != null || lineData2.Data.Length < 3 || lineData2.Data[0] != ' ' || lineData2.Data[1] != ' ')
						{
							break;
						}
						moduleManifest.Files.Add(lineData2.Data.Trim());
					}
					i = num - 1;
					break;
				}
				}
			}
			return moduleManifest;
		}
	}
}
