using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.Serialization.Utilities.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Configurations for Odin DLLs import settings.
	/// </summary>
	[SirenixEditorConfig]
	public class ImportSettingsConfig : GlobalConfig<ImportSettingsConfig>
	{
		public enum OdinAssemblyOptions
		{
			Recommended,
			AOT,
			JIT
		}

		private const string AOTAssemblyFolder = "NoEmitAndNoEditor";

		private const string JITAssemblyFolder = "NoEditor";

		private const int LabelWidth = 270;

		private static bool editorOnlyMode;

		private static bool isHeaderInfoBoxFolded = true;

		[SerializeField]
		[HideInInspector]
		private bool automateBeforeBuild = true;

		private OdinAssemblyOptions currentOption;

		/// <summary>
		/// Gets or sets a value indicating whether or not Odin should automatically configure the import settings of its DLLs in a preprocess build step.
		/// Keep in mind that this feature is only supported by Unity version 5.6 and up.
		/// </summary>
		[ShowInInspector]
		[BoxGroup("AutomateBox", true, false, 0f, ShowLabel = false)]
		[LabelWidth(270f)]
		[SuffixLabel("$AutomateSuffix", false)]
		[DisableIf("editorOnlyMode")]
		[EnableIf("IsAutomationSupported")]
		public bool AutomateBeforeBuild
		{
			get
			{
				if (IsAutomationSupported)
				{
					return automateBeforeBuild;
				}
				return false;
			}
			set
			{
				automateBeforeBuild = value;
				if (automateBeforeBuild && !IsAutomationSupported)
				{
					Debug.LogWarning((object)"Automatic configuration of Odin DLL import settings is only supported by Unity versions 5.6 and up.");
				}
			}
		}

		[EnableGUI]
		[LabelWidth(270f)]
		[BoxGroup("SettingsBox", true, false, 0f, ShowLabel = false)]
		[ShowInInspector]
		[DisplayAsString]
		[SuffixLabel("$BuildTargetSuffix", false)]
		[DisableIf("editorOnlyMode")]
		private BuildTarget CurrentBuildTarget => EditorUserBuildSettings.get_activeBuildTarget();

		[EnableGUI]
		[LabelWidth(270f)]
		[BoxGroup("SettingsBox", true, false, 0f, ShowLabel = false)]
		[ShowInInspector]
		[DisplayAsString]
		[SuffixLabel("$ScriptingBackendSuffix", false)]
		[DisableIf("editorOnlyMode")]
		private ScriptingImplementation CurrentScriptingBackend => AssemblyImportSettingsUtilities.GetCurrentScriptingBackend();

		[EnableGUI]
		[LabelWidth(270f)]
		[BoxGroup("SettingsBox", true, false, 0f, ShowLabel = false)]
		[ShowInInspector]
		[DisplayAsString]
		[SuffixLabel("$ApiLevelSuffix", false)]
		[DisableIf("editorOnlyMode")]
		private ApiCompatibilityLevel CurrentApiCompatibilityLevel => AssemblyImportSettingsUtilities.GetCurrentApiCompatibilityLevel();

		[EnableGUI]
		[LabelWidth(270f)]
		[ShowInInspector]
		[DisplayAsString]
		[BoxGroup("SelectApplyBox", true, false, 0f, ShowLabel = false)]
		[DisableIf("editorOnlyMode")]
		[SuffixLabel("$RecommendedSuffix", false)]
		private string CurrentRecommendedBuildConfiguration
		{
			get
			{
				if (editorOnlyMode)
				{
					return "Editor Only Mode enabled.";
				}
				return GetRecommendedOption().ToString();
			}
		}

		[ShowInInspector]
		[LabelWidth(270f)]
		[EnumToggleButtons]
		[EnableIf("EnableApplyButton")]
		[BoxGroup("SelectApplyBox", true, false, 0f, ShowLabel = false)]
		[HorizontalGroup("SelectApplyBox/Select", 590f, 0, 0, 0f)]
		public OdinAssemblyOptions AssemblyBuildConfiguration
		{
			get
			{
				return currentOption;
			}
			set
			{
				currentOption = value;
			}
		}

		private string AutomateSuffix
		{
			get
			{
				if (!IsAutomationSupported)
				{
					return "The automation feature is only available in Unity 5.6 and up";
				}
				return "Recommended";
			}
		}

		private string BuildTargetSuffix
		{
			get
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				if (!AssemblyImportSettingsUtilities.PlatformSupportsJIT(EditorUserBuildSettings.get_activeBuildTarget()))
				{
					return "Only AOT";
				}
				return "Supports JIT";
			}
		}

		private string ScriptingBackendSuffix
		{
			get
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				if (!AssemblyImportSettingsUtilities.ScriptingBackendSupportsJIT(AssemblyImportSettingsUtilities.GetCurrentScriptingBackend()))
				{
					return "Only AOT";
				}
				return "Supports JIT";
			}
		}

		private string ApiLevelSuffix
		{
			get
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				if (!AssemblyImportSettingsUtilities.ApiCompatibilityLevelSupportsJIT(AssemblyImportSettingsUtilities.GetCurrentApiCompatibilityLevel()))
				{
					return "Only AOT";
				}
				return "Supports JIT";
			}
		}

		private string RecommendedSuffix
		{
			get
			{
				if (GetRecommendedOption() != OdinAssemblyOptions.JIT)
				{
					return "Some settings are only AOT";
				}
				return "All settings support JIT";
			}
		}

		private bool EnableApplyButton
		{
			get
			{
				if (!editorOnlyMode)
				{
					if (IsAutomationSupported)
					{
						if (IsAutomationSupported)
						{
							return !automateBeforeBuild;
						}
						return false;
					}
					return true;
				}
				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether or not automatic configuration of Odin's DLL import settings is supported by the current Unity version.
		/// </summary>
		public static bool IsAutomationSupported => UnityVersion.IsVersionOrGreater(5, 6);

		[BoxGroup("SelectApplyBox", true, false, 0f, ShowLabel = false)]
		[Button(ButtonSizes.Large)]
		[EnableIf("EnableApplyButton")]
		private void Apply()
		{
			UnityEditorEventUtility.EditorApplication_delayCall += ApplyDelayed;
		}

		private void ApplyDelayed()
		{
			ApplyImportSettings();
		}

		public void ApplyImportSettings()
		{
			//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			if (EditorOnlyModeConfig.Instance.IsEditorOnlyModeEnabled())
			{
				throw new InvalidOperationException("Editor Only Mode enabled.");
			}
			string fullName = new DirectoryInfo(SirenixAssetPaths.SirenixAssembliesPath).FullName;
			string text = Directory.GetCurrentDirectory().TrimEnd('\\', '/');
			bool flag = !new DirectoryInfo(text).HasSubDirectory(new DirectoryInfo(fullName));
			string path = fullName + "NoEmitAndNoEditor/";
			string path2 = fullName + "NoEditor/";
			DirectoryInfo directoryInfo = new DirectoryInfo(path);
			DirectoryInfo directoryInfo2 = new DirectoryInfo(path2);
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			FileInfo[] files = directoryInfo.GetFiles("*.dll");
			foreach (FileInfo fileInfo in files)
			{
				string fullName2 = fileInfo.FullName;
				fullName2 = ((!flag) ? fullName2.Substring(text.Length + 1) : (SirenixAssetPaths.SirenixAssembliesPath.TrimEnd('\\', '/') + "/" + fullName2.Substring(fullName.Length)));
				list.Add(fullName2);
			}
			FileInfo[] files2 = directoryInfo2.GetFiles("*.dll");
			foreach (FileInfo fileInfo2 in files2)
			{
				string fullName3 = fileInfo2.FullName;
				fullName3 = ((!flag) ? fullName3.Substring(text.Length + 1) : (SirenixAssetPaths.SirenixAssembliesPath.TrimEnd('\\', '/') + "/" + fullName3.Substring(fullName.Length)));
				list2.Add(fullName3);
			}
			OdinAssemblyOptions recommendedOption = currentOption;
			if (recommendedOption == OdinAssemblyOptions.Recommended)
			{
				recommendedOption = GetRecommendedOption();
			}
			AssetDatabase.StartAssetEditing();
			try
			{
				switch (recommendedOption)
				{
				case OdinAssemblyOptions.AOT:
					SetImportSettings(EditorUserBuildSettings.get_activeBuildTarget(), list, (OdinAssemblyImportSettings)0);
					SetImportSettings(EditorUserBuildSettings.get_activeBuildTarget(), list2, (OdinAssemblyImportSettings)3);
					break;
				case OdinAssemblyOptions.JIT:
					SetImportSettings(EditorUserBuildSettings.get_activeBuildTarget(), list, (OdinAssemblyImportSettings)3);
					SetImportSettings(EditorUserBuildSettings.get_activeBuildTarget(), list2, (OdinAssemblyImportSettings)0);
					break;
				default:
					throw new ArgumentException(string.Concat("Unknown Odin assembly option: ", currentOption, ". Please select either AOT or JIT"));
				}
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}
		}

		private static void SetImportSettings(BuildTarget platform, List<string> assemblyPaths, OdinAssemblyImportSettings importSettings)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			foreach (string assemblyPath in assemblyPaths)
			{
				string text = assemblyPath.Replace('\\', '/');
				AssemblyImportSettingsUtilities.SetAssemblyImportSettings(platform, text, importSettings);
			}
		}

		[OnInspectorGUI]
		[PropertyOrder(-1000000f)]
		private void DrawEditorOnlyMode()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			if ((int)Event.get_current().get_type() == 8)
			{
				editorOnlyMode = EditorOnlyModeConfig.Instance.IsEditorOnlyModeEnabled();
			}
			if (editorOnlyMode)
			{
				SirenixEditorGUI.InfoMessageBox("Editor Only Mode is currently enabled. These configurations are currently irrelevant.");
			}
		}

		[OnInspectorGUI]
		[PropertyOrder(-10000f)]
		private void DrawHeaderInfoBox()
		{
			isHeaderInfoBoxFolded = SirenixEditorGUI.DetailedMessageBox("Odin will automatically detect your current build target and make sure it uses the assemblies best suited for your target platform.\n\nClick here to learn more.", "The Odin Serializer has two sets of assemblies: one set for AOT platforms and one for those platforms where JIT'ing is supported. JIT is usually the most performant, but is not supported on all platforms. Finding out whether your setup supports it, goes beyond what Unity's Import Settings has to offer, which is why this tool becomes necessary.\n\nOdin has a predefined set of known setups where JIT is supported. If your setup doesn't match any of those, then it'll choose to use the AOT assemblies.\n\nEnabling the \"Automate Before Build\" option will enable a preprocess build step, that will configure the import settings automatically, based on your current build settings.\n\nIf you've stumbled on a setup where we're using AOT when we could be using JIT, or the other way around, then you can always disable the preprocess build set and manually configure your assemblies.", (MessageType)1, isHeaderInfoBoxFolded);
		}

		public OdinAssemblyOptions GetRecommendedOption()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			if (!AssemblyImportSettingsUtilities.IsJITSupported(EditorUserBuildSettings.get_activeBuildTarget(), AssemblyImportSettingsUtilities.GetCurrentScriptingBackend(), AssemblyImportSettingsUtilities.GetCurrentApiCompatibilityLevel()))
			{
				return OdinAssemblyOptions.AOT;
			}
			return OdinAssemblyOptions.JIT;
		}
	}
}
