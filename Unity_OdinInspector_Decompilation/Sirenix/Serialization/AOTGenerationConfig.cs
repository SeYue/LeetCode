using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization.Editor;
using Sirenix.Serialization.Utilities.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities.Editor.Expressions;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Contains configuration for generating an assembly that provides increased AOT support in Odin.
	/// </summary>
	[SirenixEditorConfig]
	public class AOTGenerationConfig : GlobalConfig<AOTGenerationConfig>
	{
		[Serializable]
		private class TypeEntry : ISearchFilterable
		{
			[NonSerialized]
			public bool IsInitialized;

			[NonSerialized]
			public bool IsNew;

			[NonSerialized]
			public string NiceTypeName;

			public string TypeName;

			public bool IsCustom;

			public bool Emit;

			public Type Type;

			public bool IsMatch(string searchString)
			{
				return FuzzySearch.Contains(searchString, (Type == null) ? "null invalid" : Type.GetNiceFullName());
			}
		}

		private class TypeEntryDrawer : OdinValueDrawer<TypeEntry>
		{
			private static readonly GUIStyle MissingLabelStyle;

			private static readonly GUIStyle NewLabelStyle;

			private static readonly GUIStyle ChangedLabelStyle;

			private bool isEditing;

			protected override void DrawPropertyLayout(GUIContent label)
			{
				//IL_0016: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				//IL_001c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Unknown result type (might be due to invalid IL or missing references)
				//IL_0027: Unknown result type (might be due to invalid IL or missing references)
				//IL_008e: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
				//IL_0104: Unknown result type (might be due to invalid IL or missing references)
				//IL_0105: Unknown result type (might be due to invalid IL or missing references)
				//IL_012f: Unknown result type (might be due to invalid IL or missing references)
				//IL_014f: Unknown result type (might be due to invalid IL or missing references)
				//IL_016f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0187: Unknown result type (might be due to invalid IL or missing references)
				//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
				//IL_0214: Unknown result type (might be due to invalid IL or missing references)
				//IL_0234: Unknown result type (might be due to invalid IL or missing references)
				//IL_023a: Invalid comparison between Unknown and I4
				//IL_0243: Unknown result type (might be due to invalid IL or missing references)
				//IL_0254: Unknown result type (might be due to invalid IL or missing references)
				//IL_025a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0264: Unknown result type (might be due to invalid IL or missing references)
				//IL_027b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0281: Unknown result type (might be due to invalid IL or missing references)
				//IL_028b: Unknown result type (might be due to invalid IL or missing references)
				IPropertyValueEntry<TypeEntry> propertyValueEntry = base.ValueEntry;
				TypeEntry smartValue = propertyValueEntry.SmartValue;
				bool flag = false;
				Rect controlRect = EditorGUILayout.GetControlRect((GUILayoutOption[])(object)new GUILayoutOption[0]);
				Rect val = controlRect.SetWidth(20f);
				((Rect)(ref controlRect)).set_xMin(((Rect)(ref controlRect)).get_xMin() + 20f);
				if (smartValue.Type == null)
				{
					isEditing = true;
				}
				bool flag2 = isEditing;
				if (string.IsNullOrEmpty(smartValue.NiceTypeName) && smartValue.Type != null)
				{
					smartValue.NiceTypeName = smartValue.Type.GetNiceName();
				}
				GUIHelper.PushGUIEnabled(smartValue.Type != null);
				flag = smartValue.Emit != (smartValue.Emit = EditorGUI.Toggle(val, smartValue.Emit));
				GUIHelper.PopGUIEnabled();
				((Rect)(ref controlRect)).set_y(((Rect)(ref controlRect)).get_y() + 2f);
				((Rect)(ref controlRect)).set_width(((Rect)(ref controlRect)).get_width() - 30f);
				Rect val2 = controlRect;
				if (smartValue.IsNew || smartValue.IsCustom || smartValue.Type == null)
				{
					((Rect)(ref val2)).set_xMax(((Rect)(ref val2)).get_xMax() - 78f);
				}
				Rect val3 = controlRect;
				((Rect)(ref val3)).set_xMin(((Rect)(ref val3)).get_xMax() - 75f);
				((Rect)(ref val3)).set_width(75f);
				if (smartValue.Type == null)
				{
					EditorGUI.LabelField(val3, GUIHelper.TempContent("INVALID"), MissingLabelStyle);
				}
				else if (smartValue.IsCustom)
				{
					EditorGUI.LabelField(val3, GUIHelper.TempContent("MODIFIED"), ChangedLabelStyle);
				}
				else if (smartValue.IsNew)
				{
					EditorGUI.LabelField(val3, GUIHelper.TempContent("NEW"), NewLabelStyle);
				}
				else
				{
					EditorGUI.LabelField(val3, GUIHelper.TempContent(""));
				}
				string text = smartValue.TypeName;
				if (isEditing)
				{
					GUI.SetNextControlName(propertyValueEntry.Property.Path);
					text = EditorGUI.TextField(val2, smartValue.TypeName, EditorStyles.get_textField());
					if (GUI.GetNameOfFocusedControl() == propertyValueEntry.Property.Path && (((object)Event.get_current()).Equals((object)Event.KeyboardEvent("return")) || Event.get_current().OnKeyUp((KeyCode)13)))
					{
						isEditing = false;
					}
				}
				else
				{
					if (GUI.Button(val2, smartValue.NiceTypeName, EditorStyles.get_label()))
					{
						isEditing = true;
					}
					if ((int)Event.get_current().get_type() == 7 && ((Rect)(ref controlRect)).Contains(Event.get_current().get_mousePosition()))
					{
						EditorIcons.Pen.Draw(controlRect.AlignRight(30f).AddX(30f), 16f);
					}
				}
				if (isEditing && SirenixEditorGUI.IconButton(controlRect.AlignRight(30f).AddX(30f), EditorIcons.Checkmark))
				{
					isEditing = false;
				}
				if ((text ?? "") != (smartValue.TypeName ?? ""))
				{
					smartValue.TypeName = text;
					smartValue.IsCustom = true;
					smartValue.Type = GetTypeFromName(smartValue.TypeName);
					smartValue.NiceTypeName = ((smartValue.Type == null) ? smartValue.TypeName : smartValue.Type.GetNiceName());
					flag = true;
				}
				if (flag2 && !isEditing)
				{
					if (smartValue.Type != null)
					{
						smartValue.TypeName = TypeBinder.BindToName(smartValue.Type);
					}
					propertyValueEntry.Values.ForceMarkDirty();
				}
				if (flag)
				{
					smartValue.IsCustom = true;
					propertyValueEntry.Values.ForceMarkDirty();
				}
			}

			static TypeEntryDrawer()
			{
				//IL_000a: Unknown result type (might be due to invalid IL or missing references)
				//IL_000f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected O, but got Unknown
				//IL_001e: Unknown result type (might be due to invalid IL or missing references)
				//IL_002a: Expected O, but got Unknown
				//IL_0034: Unknown result type (might be due to invalid IL or missing references)
				//IL_0039: Unknown result type (might be due to invalid IL or missing references)
				//IL_003e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0048: Expected O, but got Unknown
				//IL_0048: Unknown result type (might be due to invalid IL or missing references)
				//IL_0054: Expected O, but got Unknown
				//IL_005e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0063: Unknown result type (might be due to invalid IL or missing references)
				//IL_0068: Unknown result type (might be due to invalid IL or missing references)
				//IL_0072: Expected O, but got Unknown
				//IL_0072: Unknown result type (might be due to invalid IL or missing references)
				//IL_007e: Expected O, but got Unknown
				GUIStyle val = new GUIStyle(GUIStyle.op_Implicit("sv_label_6"));
				val.set_margin(new RectOffset(3, 3, 2, 0));
				val.set_alignment((TextAnchor)4);
				MissingLabelStyle = val;
				GUIStyle val2 = new GUIStyle(GUIStyle.op_Implicit("sv_label_3"));
				val2.set_margin(new RectOffset(3, 3, 2, 0));
				val2.set_alignment((TextAnchor)4);
				NewLabelStyle = val2;
				GUIStyle val3 = new GUIStyle(GUIStyle.op_Implicit("sv_label_4"));
				val3.set_margin(new RectOffset(3, 3, 2, 0));
				val3.set_alignment((TextAnchor)4);
				ChangedLabelStyle = val3;
			}
		}

		private static readonly bool EditorOnlyBuild = false;

		private static readonly TwoWaySerializationBinder TypeBinder = new DefaultSerializationBinder();

		[DisableIf("EditorOnlyBuild")]
		[SerializeField]
		[ToggleLeft]
		[EnableIf("EnableAutomateBeforeBuilds")]
		[SuffixLabel("$AutomateBeforeBuildsSuffix", false)]
		private bool automateBeforeBuilds;

		[DisableIf("EditorOnlyBuild")]
		[Indent(1)]
		[SerializeField]
		[ToggleLeft]
		[ShowIf("ShowAutomateConfig", true)]
		private bool deleteDllAfterBuilds = true;

		[Indent(1)]
		[SerializeField]
		[ToggleLeft]
		[ShowIf("ShowAutomateConfig", true)]
		public bool AutomateForAllAOTPlatforms = true;

		[Indent(1)]
		[HideIf("AutomateForAllAOTPlatforms", true)]
		[SerializeField]
		[ShowIf("ShowAutomateConfig", true)]
		private List<BuildTarget> automateForPlatforms = new List<BuildTarget>
		{
			(BuildTarget)9,
			(BuildTarget)20
		};

		[SerializeField]
		[HideInInspector]
		private long lastScan;

		[DisableIf("EditorOnlyBuild")]
		[SerializeField]
		[PropertyOrder(4f)]
		[ListDrawerSettings(DraggableItems = false, OnTitleBarGUI = "GenericVariantsTitleGUI", HideAddButton = true)]
		[Searchable(FilterOptions = SearchFilterOptions.ISearchFilterableInterface)]
		private List<TypeEntry> supportSerializedTypes;

		private string AutomateBeforeBuildsSuffix
		{
			get
			{
				if (!EnableAutomateBeforeBuilds)
				{
					return "The automation feature is only available in Unity 5.6 and up";
				}
				return "";
			}
		}

		private bool EnableAutomateBeforeBuilds => UnityVersion.IsVersionOrGreater(5, 6);

		private bool ShowAutomateConfig
		{
			get
			{
				if (EnableAutomateBeforeBuilds)
				{
					return automateBeforeBuilds;
				}
				return false;
			}
		}

		/// <summary>
		/// <para>
		/// Whether to automatically scan the project and generate an AOT dll, right before builds. This will only affect platforms that are in the <see cref="P:Sirenix.Serialization.AOTGenerationConfig.AutomateForPlatforms" /> list.
		/// </para>
		/// <para>
		/// **This will only work on Unity 5.6 and higher!**
		/// </para>
		/// </summary>
		public bool AutomateBeforeBuilds
		{
			get
			{
				return automateBeforeBuilds;
			}
			set
			{
				automateBeforeBuilds = value;
			}
		}

		/// <summary>
		/// Whether to automatically delete the generated AOT dll after a build has completed.
		/// </summary>
		public bool DeleteDllAfterBuilds
		{
			get
			{
				return deleteDllAfterBuilds;
			}
			set
			{
				deleteDllAfterBuilds = value;
			}
		}

		/// <summary>
		/// A list of platforms to automatically scan the project and generate an AOT dll for, right before builds. This will do nothing unless <see cref="P:Sirenix.Serialization.AOTGenerationConfig.AutomateBeforeBuilds" /> is true.
		/// </summary>
		public List<BuildTarget> AutomateForPlatforms => automateForPlatforms;

		/// <summary>
		/// The path to the AOT folder that the AOT .dll and linker file is created in, relative to the current project folder.
		/// </summary>
		public string AOTFolderPath => SirenixAssetPaths.SirenixAssembliesPath + "AOT/";

		public bool ShouldAutomationGeneration(BuildTarget target)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			if (!AutomateBeforeBuilds)
			{
				return false;
			}
			if (AutomateForAllAOTPlatforms)
			{
				BuildTarget activeBuildTarget = EditorUserBuildSettings.get_activeBuildTarget();
				ScriptingImplementation currentScriptingBackend = AssemblyImportSettingsUtilities.GetCurrentScriptingBackend();
				ApiCompatibilityLevel currentApiCompatibilityLevel = AssemblyImportSettingsUtilities.GetCurrentApiCompatibilityLevel();
				if (AssemblyImportSettingsUtilities.IsJITSupported(activeBuildTarget, currentScriptingBackend, currentApiCompatibilityLevel))
				{
					return false;
				}
				return true;
			}
			if (AutomateForPlatforms != null && AutomateForPlatforms.Contains(target))
			{
				return true;
			}
			return false;
		}

		private void GenericVariantsTitleGUI()
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Expected O, but got Unknown
			SirenixEditorGUI.VerticalLineSeparator();
			GUILayout.Label("Last scan: " + DateTime.FromBinary(lastScan).ToString(), SirenixGUIStyles.CenteredGreyMiniLabel, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			if (SirenixEditorGUI.ToolbarButton(new GUIContent("  Sort  ")))
			{
				SortTypes();
			}
			if (SirenixEditorGUI.ToolbarButton(EditorIcons.Plus))
			{
				supportSerializedTypes.Insert(0, new TypeEntry
				{
					IsCustom = true,
					Emit = true
				});
			}
		}

		private void SortTypes()
		{
			Comparison<TypeEntry> comparison = delegate(TypeEntry a, TypeEntry b)
			{
				bool flag = a.Type != null;
				bool flag2 = b.Type != null;
				if (flag != flag2)
				{
					if (!flag)
					{
						return -1;
					}
					return 1;
				}
				if (!flag)
				{
					return (a.TypeName ?? "").CompareTo(b.TypeName ?? "");
				}
				if (a.IsCustom != b.IsCustom)
				{
					if (!a.IsCustom)
					{
						return 1;
					}
					return -1;
				}
				return (a.IsNew != b.IsNew) ? ((!a.IsNew) ? 1 : (-1)) : (a.NiceTypeName ?? "").CompareTo(b.NiceTypeName ?? "");
			};
			supportSerializedTypes.Sort(comparison);
		}

		[DisableIf("EditorOnlyBuild")]
		[Button("Scan Project", 36)]
		[HorizontalGroup("ButtonMargin", 0.2f, 0, 0, 0f, PaddingRight = -4f)]
		[PropertyOrder(2f)]
		private void ScanProjectButton()
		{
			UnityEditorEventUtility.EditorApplication_delayCall += ScanProject;
		}

		/// <summary>
		/// Scans the entire project for types to support AOT serialization for.
		/// </summary>
		public void ScanProject()
		{
			List<Type> types = default(List<Type>);
			if (AOTSupportUtilities.ScanProjectForSerializedTypes(ref types, true, true, true, true, (List<string>)null, true))
			{
				RegisterTypes(supportSerializedTypes, types);
				SortTypes();
				lastScan = DateTime.Now.Ticks;
				EditorUtility.SetDirty((Object)(object)this);
			}
		}

		private void RegisterTypes(List<TypeEntry> typeEntries, List<Type> types)
		{
			HashSet<Type> preExistingNonCustomTypes = new HashSet<Type>(from n in typeEntries
				where !n.IsCustom && n.Type != null
				select n.Type);
			typeEntries.RemoveAll((TypeEntry n) => !n.IsCustom);
			HashSet<Type> preExistingCustomTypes = new HashSet<Type>(from n in typeEntries
				where n.Type != null
				select n.Type);
			typeEntries.AddRange(from type in types
				where !preExistingCustomTypes.Contains(type)
				select new TypeEntry
				{
					Type = type,
					TypeName = TypeBinder.BindToName(type),
					NiceTypeName = type.GetNiceName(),
					IsCustom = false,
					Emit = true,
					IsNew = !preExistingNonCustomTypes.Contains(type),
					IsInitialized = false
				});
			InitializeTypeEntries();
		}

		[DisableIf("EditorOnlyBuild")]
		[OnInspectorGUI]
		[PropertyOrder(-1f)]
		private void DrawTopInfoBox()
		{
			SirenixEditorGUI.InfoMessageBox("On AOT-compiled platforms, Unity's code stripping can remove classes that the serialization system needs, or fail to generate code for needed variants of generic types. Therefore, Odin can create an assembly that directly references all functionality that is needed at runtime, to ensure it is available.");
		}

		[DisableIf("EditorOnlyBuild")]
		[OnInspectorGUI]
		[HorizontalGroup("ButtonMargin", 0f, 0, 0, 0f)]
		[PropertyOrder(1f)]
		private void DrawWarning()
		{
			EditorGUILayout.HelpBox("Scanning the entire project might take a while. It will scan the entire project for relevant types including ScriptableObjects, prefabs and scenes. Modified type entries will not be touched.", (MessageType)2);
		}

		[DisableIf("EditorOnlyBuild")]
		[TitleGroup("Generate AOT DLL", "Sirenix/Assemblies/AOT/Sirenix.Serialization.AOTGenerated.dll", TitleAlignments.Left, true, true, false, 0f)]
		[PropertyOrder(9f)]
		[Button("Generate DLL", ButtonSizes.Large)]
		private void GenerateDLLButton()
		{
			GenerateDLL();
			GUIUtility.ExitGUI();
		}

		/// <summary>
		/// Generates an AOT DLL, using the current configuration of the AOTGenerationConfig instance.
		/// </summary>
		public void GenerateDLL()
		{
			List<Type> list = (from n in supportSerializedTypes
				where n.Emit && n.Type != null
				select n.Type).ToList();
			FixUnityAboutWindowBeforeEmit.Fix();
			AOTSupportUtilities.GenerateDLL(AOTFolderPath, "Sirenix.Serialization.AOTGenerated", list, true);
		}

		public void GenerateDLL(string folderPath, bool generateLinkXML = true)
		{
			List<Type> list = (from n in supportSerializedTypes
				where n.Emit && n.Type != null
				select n.Type).ToList();
			FixUnityAboutWindowBeforeEmit.Fix();
			AOTSupportUtilities.GenerateDLL(folderPath, "Sirenix.Serialization.AOTGenerated", list, generateLinkXML);
		}

		[DisableIf("EditorOnlyBuild")]
		[OnInspectorGUI]
		[PropertyOrder(-1000f)]
		private void OnGUIInitializeTypeEntries()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			if ((int)Event.get_current().get_type() == 8)
			{
				InitializeTypeEntries();
			}
		}

		public List<Type> GetAOTSupportedTypes()
		{
			InitializeTypeEntries();
			return (from n in supportSerializedTypes
				where n.Emit && n.Type != null
				select n.Type).ToList();
		}

		private void InitializeTypeEntries()
		{
			supportSerializedTypes = supportSerializedTypes ?? new List<TypeEntry>();
			foreach (TypeEntry supportSerializedType in supportSerializedTypes)
			{
				if (supportSerializedType.IsInitialized)
				{
					continue;
				}
				if (supportSerializedType.Type == null)
				{
					if (supportSerializedType.TypeName != null)
					{
						supportSerializedType.Type = GetTypeFromName(supportSerializedType.TypeName);
					}
					if (supportSerializedType.Type != null)
					{
						supportSerializedType.NiceTypeName = supportSerializedType.Type.GetNiceName();
					}
				}
				supportSerializedType.IsInitialized = true;
			}
		}

		private static Type GetTypeFromName(string name)
		{
			Type type = TypeBinder.BindToType(name);
			if (type != null)
			{
				return type;
			}
			if (ExpressionUtility.TryParseTypeNameAsCSharpIdentifier(name, out type))
			{
				return type;
			}
			return null;
		}
	}
}
