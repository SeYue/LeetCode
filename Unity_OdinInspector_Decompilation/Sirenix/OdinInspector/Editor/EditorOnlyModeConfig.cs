using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Editor Only Mode Utility.
	/// </summary>
	public sealed class EditorOnlyModeConfig
	{
		private static readonly object instance_LOCK = new object();

		private static EditorOnlyModeConfig instance;

		private const string BACKUP_FILE_SUFFIX = ".backup.txt";

		private const string SOURCE_CODE_NOT_SUPPORTED_MESSAGE = "Enabling and disabling Editor Only Mode is not supported when using Odin with source code.";

		private static readonly string RuntimeAssemblyDefinitionFile = "Sirenix.OdinInspector.CompatibilityLayer.asmdef";

		private static readonly string[] SerializerScriptFiles = new string[2] { "VectorIntFormatters.cs", "SerializedNetworkBehaviour.cs" };

		private static string ExcludeFromEverything = "PluginImporter:\r\n  serializedVersion: 1\r\n  iconMap: {}\r\n  executionOrder: {}\r\n  isPreloaded: 0\r\n  isOverridable: 0\r\n  platformData:\r\n    Any:\r\n      enabled: 0\r\n      settings:\r\n        Exclude Android: 1\r\n        Exclude Editor: 1\r\n        Exclude Linux: 1\r\n        Exclude Linux64: 1\r\n        Exclude LinuxUniversal: 1\r\n        Exclude N3DS: 1\r\n        Exclude OSXIntel: 1\r\n        Exclude OSXIntel64: 1\r\n        Exclude OSXUniversal: 1\r\n        Exclude PS4: 1\r\n        Exclude PSM: 1\r\n        Exclude PSP2: 1\r\n        Exclude SamsungTV: 1\r\n        Exclude Tizen: 1\r\n        Exclude WebGL: 1\r\n        Exclude WiiU: 1\r\n        Exclude Win: 1\r\n        Exclude Win64: 1\r\n        Exclude WindowsStoreApps: 1\r\n        Exclude XboxOne: 1\r\n        Exclude iOS: 1\r\n        Exclude tvOS: 1\r\n    Editor:\r\n      enabled: 0\r\n      settings:\r\n        DefaultValueInitialized: true\r\n  userData:\r\n  assetBundleName:\r\n  assetBundleVariant: ";

		private static string ExcludeFromEverythingExceptEditor = "PluginImporter:\r\n  serializedVersion: 1\r\n  iconMap: {}\r\n  executionOrder: {}\r\n  isPreloaded: 0\r\n  isOverridable: 0\r\n  platformData:\r\n    Any:\r\n      enabled: 0\r\n      settings:\r\n        Exclude Android: 1\r\n        Exclude Editor: 0\r\n        Exclude Linux: 1\r\n        Exclude Linux64: 1\r\n        Exclude LinuxUniversal: 1\r\n        Exclude N3DS: 1\r\n        Exclude OSXIntel: 1\r\n        Exclude OSXIntel64: 1\r\n        Exclude OSXUniversal: 1\r\n        Exclude PS4: 1\r\n        Exclude PSM: 1\r\n        Exclude PSP2: 1\r\n        Exclude SamsungTV: 1\r\n        Exclude Tizen: 1\r\n        Exclude WebGL: 1\r\n        Exclude WiiU: 1\r\n        Exclude Win: 1\r\n        Exclude Win64: 1\r\n        Exclude WindowsStoreApps: 1\r\n        Exclude XboxOne: 1\r\n        Exclude iOS: 1\r\n        Exclude tvOS: 1\r\n    Editor:\r\n      enabled: 1\r\n      settings:\r\n        DefaultValueInitialized: true\r\n  userData:\r\n  assetBundleName:\r\n  assetBundleVariant: ";

		private readonly bool EditorOnlyBuild;

		private bool isUsingSourceCode;

		private string[] platformSpecificAssemblyFiles;

		private string[] globalAssemblyFiles;

		private bool isInEditorOnlyMode;

		public static EditorOnlyModeConfig Instance
		{
			get
			{
				if (instance == null)
				{
					lock (instance_LOCK)
					{
						if (instance == null)
						{
							instance = new EditorOnlyModeConfig();
						}
					}
				}
				return instance;
			}
		}

		private bool SerializationModeIsForceText => (int)EditorSettings.get_serializationMode() == 2;

		private bool ShowEnableEditorOnlyMode
		{
			get
			{
				if (!isInEditorOnlyMode)
				{
					return !isUsingSourceCode;
				}
				return false;
			}
		}

		private bool ShowDisableEditorOnlyMode
		{
			get
			{
				if (isInEditorOnlyMode)
				{
					return !isUsingSourceCode;
				}
				return false;
			}
		}

		[OnInspectorGUI]
		[PropertyOrder(-2399f)]
		[InfoBox("If you're not interested in using Odin's serialization system - inheriting from classes such as SerializedMonoBehaviour and SerializedScriptableObject etc. - you can disable the serialization system completely, without losing the ability to leverage all of the attributes and editor functionality that Odin provides. This will also let you use Odin while targeting non-IL2CPP UWP platform targets.\n\nDisabling the serialization system will prevent almost all of Odin from being included in your builds. The only Odin code that will be included is the small Sirenix.OdinInspector.Attributes assembly, containing only the attribute definitions. If you are using IL2CPP, many of these attributes will likely be removed during Unity's code stripping step.\n\nNote that Odin still uses the serialization system in the editor itself to provide you with various editor functionality. You will still be able to inherit from classes like SerializedMonoBehaviour while in the editor, but you will get a warning in the inspector if you do so, and you will get compiler errors if you try to build.\n", InfoMessageType.Info, null)]
		private void TopMessage()
		{
		}

		private EditorOnlyModeConfig()
		{
			Update();
		}

		/// <summary>
		/// Gaither all necessary information about the editor only state.
		/// </summary>
		public void Update()
		{
			string sirenixAssembliesPath = SirenixAssetPaths.SirenixAssembliesPath;
			if (!Directory.Exists(sirenixAssembliesPath))
			{
				isInEditorOnlyMode = false;
				isUsingSourceCode = true;
				return;
			}
			string text = SirenixAssetPaths.SirenixAssembliesPath + "Sirenix.Serialization.Config.dll";
			globalAssemblyFiles = ((!File.Exists(text)) ? new string[0] : new string[1] { text });
			platformSpecificAssemblyFiles = (from p in AssetDatabase.GetAllAssetPaths()
				where p.StartsWith(SirenixAssetPaths.SirenixAssembliesPath)
				where p.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase)
				where p.StartsWith(SirenixAssetPaths.SirenixAssembliesPath + "NoEditor/") || p.StartsWith(SirenixAssetPaths.SirenixAssembliesPath + "NoEmitAndNoEditor/")
				select p).ToArray();
			if (platformSpecificAssemblyFiles.Length + globalAssemblyFiles.Length == 0)
			{
				isUsingSourceCode = true;
				return;
			}
			isUsingSourceCode = false;
			isInEditorOnlyMode = platformSpecificAssemblyFiles.Concat(globalAssemblyFiles).All((string x) => File.Exists(x + ".backup.txt"));
		}

		[Button(ButtonSizes.Gigantic)]
		[HideIf("SerializationModeIsForceText", true)]
		[DisableIf("EditorOnlyBuild")]
		[InfoBox("In order to make make the proper modification to the assembly import settings, the serializationMode in the EditorSettings must be set to ForceText.", InfoMessageType.Info, null)]
		[PropertyOrder(-10f)]
		private void SetForceText()
		{
			EditorSettings.set_serializationMode((SerializationMode)2);
			AssetDatabase.Refresh();
		}

		/// <summary>
		/// Disables Editor Only Mode.
		/// </summary>
		[Button(ButtonSizes.Gigantic)]
		[GUIColor(1f, 0.8f, 0f, 1f)]
		[EnableIf("SerializationModeIsForceText")]
		[ShowIf("ShowDisableEditorOnlyMode", true)]
		[DisableIf("EditorOnlyBuild")]
		[PropertyOrder(-8f)]
		public void DisableEditorOnlyMode()
		{
			if (isUsingSourceCode)
			{
				Debug.LogError((object)"Enabling and disabling Editor Only Mode is not supported when using Odin with source code.");
				return;
			}
			if (!isInEditorOnlyMode)
			{
				Debug.LogError((object)"Editor mode is already disabled.");
				return;
			}
			UnityEditorEventUtility.EditorApplication_delayCall += delegate
			{
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				if (!SerializationModeIsForceText)
				{
					Debug.LogError((object)"In order to make make the proper modification to the assembly import settings, the serializationMode in the EditorSettings must be set to ForceText.");
				}
				else
				{
					Update();
					foreach (string item in globalAssemblyFiles.Concat(platformSpecificAssemblyFiles))
					{
						if (!File.Exists(item + ".backup.txt"))
						{
							Debug.LogError((object)("The old import settings was not found which was supposed to be located at: '" + item + ".backup.txt"));
						}
						else
						{
							SetPluginImportSettings(item + ".meta", File.ReadAllText(item + ".backup.txt"));
							File.Delete(item + ".backup.txt");
						}
					}
					string[] array = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension("link.xml.backup.txt"), new string[1] { SirenixAssetPaths.SirenixAssembliesPath.TrimEnd('/') });
					for (int i = 0; i < array.Length; i++)
					{
						string text = AssetDatabase.GUIDToAssetPath(array[i]);
						string text2 = PathUtilities.GetDirectoryName(text).Replace('\\', '/').TrimEnd('/') + "/link.xml";
						AssetDatabase.MoveAsset(text, text2);
					}
					GlobalConfigAttribute globalConfigAttribute = typeof(GlobalSerializationConfig).BaseType.GetProperty("ConfigAttribute", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).GetValue(null, null) as GlobalConfigAttribute;
					string assetPath = AssetDatabase.GetAssetPath((Object)(object)GlobalConfig<GlobalSerializationConfig>.Instance);
					string fileName = Path.GetFileName(assetPath);
					string text3 = globalConfigAttribute.AssetPath.TrimEnd('/');
					if (!Directory.Exists(text3))
					{
						Directory.CreateDirectory(text3);
						AssetDatabase.Refresh();
					}
					string text4 = SirenixAssetPaths.OdinPath + "Scripts/";
					string text5 = text4 + "Editor/";
					string[] serializerScriptFiles = SerializerScriptFiles;
					foreach (string text6 in serializerScriptFiles)
					{
						string from = text5 + text6;
						string to = text4 + text6;
						MoveAsset(from, to);
					}
					string text7 = text4 + RuntimeAssemblyDefinitionFile;
					string from2 = text7 + ".backup.txt";
					MoveAsset(from2, text7);
					AssetDatabase.MoveAsset(assetPath, text3 + "/" + fileName);
					AssetDatabase.Refresh();
					Update();
				}
			};
		}

		private static void MoveAsset(string from, string to)
		{
			if (!File.Exists(from))
			{
				return;
			}
			if (File.Exists(to))
			{
				File.Delete(to);
			}
			File.Move(from, to);
			if (File.Exists(from + ".meta"))
			{
				if (File.Exists(to + ".meta"))
				{
					File.Delete(to + ".meta");
				}
				File.Move(from + ".meta", to + ".meta");
			}
		}

		/// <summary>
		/// Enables editor only mode.
		/// </summary>
		public void EnableEditorOnlyMode(bool force)
		{
			if (isUsingSourceCode)
			{
				Debug.LogError((object)"Enabling and disabling Editor Only Mode is not supported when using Odin with source code.");
				return;
			}
			if (isInEditorOnlyMode && !force)
			{
				Debug.LogError((object)"Editor mode is already enabled.");
				return;
			}
			AssetDatabase.SaveAssets();
			if (!SerializationModeIsForceText)
			{
				Debug.LogError((object)"In order to make make the proper modification to the assembly import settings, the serializationMode in the EditorSettings must be set to ForceText.");
				return;
			}
			Update();
			foreach (string item in globalAssemblyFiles.Concat(platformSpecificAssemblyFiles))
			{
				if (File.Exists(item + ".backup.txt"))
				{
					File.Delete(item + ".backup.txt");
				}
				File.Copy(item + ".meta", item + ".backup.txt");
			}
			string[] array = AssetDatabase.FindAssets("link", new string[1] { SirenixAssetPaths.SirenixAssembliesPath.TrimEnd('/') });
			for (int i = 0; i < array.Length; i++)
			{
				string text = AssetDatabase.GUIDToAssetPath(array[i]);
				if (text.ToLower().EndsWith(".xml"))
				{
					if (File.Exists(text + ".backup.txt"))
					{
						AssetDatabase.DeleteAsset(text + ".backup.txt");
					}
					AssetDatabase.MoveAsset(text, text + ".backup.txt");
				}
			}
			string assetPath = AssetDatabase.GetAssetPath((Object)(object)GlobalConfig<GlobalSerializationConfig>.Instance);
			string fileName = Path.GetFileName(assetPath);
			if (!File.Exists(SirenixAssetPaths.OdinEditorConfigsPath + fileName))
			{
				AssetDatabase.MoveAsset(assetPath, SirenixAssetPaths.OdinEditorConfigsPath + fileName);
			}
			string text2 = SirenixAssetPaths.OdinPath + "Scripts/";
			string text3 = text2 + "Editor/";
			string[] serializerScriptFiles = SerializerScriptFiles;
			foreach (string text4 in serializerScriptFiles)
			{
				string from = text2 + text4;
				string to = text3 + text4;
				MoveAsset(from, to);
			}
			string text5 = text2 + RuntimeAssemblyDefinitionFile;
			string to2 = text5 + ".backup.txt";
			MoveAsset(text5, to2);
			string[] array2 = platformSpecificAssemblyFiles;
			foreach (string text6 in array2)
			{
				SetPluginImportSettings(text6 + ".meta", ExcludeFromEverything);
			}
			string[] array3 = globalAssemblyFiles;
			foreach (string text7 in array3)
			{
				SetPluginImportSettings(text7 + ".meta", ExcludeFromEverythingExceptEditor);
			}
			AssetDatabase.Refresh();
			Update();
		}

		[Button(ButtonSizes.Gigantic)]
		[GUIColor(0f, 1f, 0f, 1f)]
		[EnableIf("SerializationModeIsForceText")]
		[ShowIf("ShowEnableEditorOnlyMode", true)]
		[DisableIf("EditorOnlyBuild")]
		[PropertyOrder(-8f)]
		private void EnableEditorOnlyMode()
		{
			UnityEditorEventUtility.EditorApplication_delayCall += delegate
			{
				EnableEditorOnlyMode(force: false);
			};
		}

		/// <summary>
		/// Checks to see whether Editor Only Mode is enabled.
		/// </summary>
		public bool IsEditorOnlyModeEnabled()
		{
			Update();
			if (isInEditorOnlyMode)
			{
				return !isUsingSourceCode;
			}
			return false;
		}

		/// <summary>
		/// Checks to see whether Odin Inspector is installed in Source Code mode.
		/// </summary>
		public bool IsInSourceCode()
		{
			Update();
			return isUsingSourceCode;
		}

		[OnInspectorGUI]
		private void OnInspectorGUI()
		{
			if (isUsingSourceCode)
			{
				SirenixEditorGUI.ErrorMessageBox("Enabling and disabling Editor Only Mode is not supported when using Odin with source code.");
			}
		}

		private bool TryThisNTimes(Action action, int numberOfTries = 20, int sleepBetweenTries = 10)
		{
			Exception ex = null;
			while (numberOfTries-- > 0)
			{
				try
				{
					action();
					return true;
				}
				catch (Exception ex2)
				{
					ex = ex2;
					Thread.Sleep(sleepBetweenTries);
				}
			}
			Debug.LogException(ex);
			return false;
		}

		private void SetPluginImportSettings(string metaFile, string pluginImportSettings)
		{
			List<string> list = new List<string>();
			string[] array = pluginImportSettings.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
			bool flag = false;
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (flag || text.StartsWith("PluginImporter:"))
				{
					list.Add(text);
					flag = true;
				}
			}
			List<string> currLines = new List<string>();
			TryThisNTimes(delegate
			{
				currLines.Clear();
				using FileStream stream2 = new FileStream(metaFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				using StreamReader streamReader = new StreamReader(stream2, Encoding.UTF8);
				while (streamReader.Peek() >= 0)
				{
					currLines.Add(streamReader.ReadLine());
				}
			});
			List<string> newLines = new List<string>();
			foreach (string item in currLines)
			{
				if (item.StartsWith("PluginImporter:"))
				{
					break;
				}
				newLines.Add(item);
			}
			newLines.AddRange(list);
			TryThisNTimes(delegate
			{
				File.Delete(metaFile);
				using FileStream stream = new FileStream(metaFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
				using StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8);
				for (int j = 0; j < newLines.Count; j++)
				{
					streamWriter.WriteLine(newLines[j]);
				}
			});
		}

		private bool HasDemos()
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(SirenixAssetPaths.SirenixPluginPath + "Demos");
			if (!directoryInfo.Exists)
			{
				return false;
			}
			return directoryInfo.GetDirectories().Any();
		}

		[OnInspectorGUI]
		[ShowIf("ShowDisableEditorOnlyMode", true)]
		[InfoBox("Since Editor Only Mode is enabled, remember to remove any Odin demos you might have imported before you build. Otherwise, you'll get compiler errors when you build, since most demos utilize Odin's serialization system which is not included in builds in Editor Only mode.", InfoMessageType.Warning, null)]
		private void DeleteImportedDemosInfo()
		{
		}

		[EnableIf("HasDemos")]
		[ShowIf("ShowDisableEditorOnlyMode", true)]
		[Button("$GetDeleteImportedDemosBtnText", ButtonSizes.Large)]
		private void DeleteImportedDemos()
		{
			List<string> list = new List<string>();
			if (Directory.Exists(SirenixAssetPaths.SirenixPluginPath + "Demos"))
			{
				list.AddRange(from x in new DirectoryInfo(SirenixAssetPaths.SirenixPluginPath + "Demos").GetDirectories()
					select x.FullName);
			}
			DeleteDirsAndFiles(list);
		}

		private string GetDeleteImportedDemosBtnText()
		{
			if (HasDemos())
			{
				return "Delete all imported demos located in \"" + SirenixAssetPaths.SirenixPluginPath + "Demos/\"";
			}
			return "No imported demos were found in \"" + SirenixAssetPaths.SirenixPluginPath + "Demos/\"";
		}

		private static void DeleteDirsAndFiles(List<string> directoriesToDelete)
		{
			foreach (string item in directoriesToDelete.Select((string x) => x.Replace('\\', '/')))
			{
				string file = item + ".mdb";
				if (Directory.Exists(item))
				{
					string[] files = Directory.GetFiles(item, "*", SearchOption.AllDirectories);
					for (int i = 0; i < files.Length; i++)
					{
						string file2 = files[i].Replace('\\', '/');
						DeleteFile(file2);
					}
					DeleteDirectory(item);
				}
				DeleteFile(file);
			}
			AssetDatabase.Refresh();
		}

		private static void DeleteFile(string file)
		{
			if (File.Exists(file))
			{
				try
				{
					File.Delete(file);
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		}

		private static void DeleteDirectory(string dir)
		{
			if (Directory.Exists(dir))
			{
				try
				{
					Directory.Delete(dir, recursive: true);
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		}
	}
}
