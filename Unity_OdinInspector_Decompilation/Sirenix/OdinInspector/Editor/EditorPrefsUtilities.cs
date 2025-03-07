using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	internal static class EditorPrefsUtilities
	{
		public static string ConvertToProjectKey(string key)
		{
			return Application.get_dataPath() + key;
		}

		public static void SaveList(string key, List<string> list)
		{
			string text = key + ".length";
			ClearList(key);
			EditorPrefs.SetInt(text, list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				EditorPrefs.SetString(key + "[" + i + "]", list[i]);
			}
		}

		public static List<string> LoadList(string key)
		{
			string text = key + ".length";
			if (!EditorPrefs.HasKey(text))
			{
				return new List<string>();
			}
			List<string> list = new List<string>();
			int @int = EditorPrefs.GetInt(text);
			for (int i = 0; i < @int; i++)
			{
				list.Add(EditorPrefs.GetString(key + "[" + i + "]", ""));
			}
			return list;
		}

		public static void ClearList(string key)
		{
			string text = key + ".length";
			int @int = EditorPrefs.GetInt(text, 0);
			for (int i = 0; i < @int; i++)
			{
				EditorPrefs.DeleteKey(key + "[" + i + "]");
			}
			EditorPrefs.DeleteKey(text);
		}
	}
}
