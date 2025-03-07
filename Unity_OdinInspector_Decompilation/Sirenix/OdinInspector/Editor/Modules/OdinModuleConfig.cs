using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Modules
{
	[SirenixEditorConfig]
	[InitializeOnLoad]
	public class OdinModuleConfig : GlobalConfig<OdinModuleConfig>
	{
		public enum ModuleAutomationSettings
		{
			Ask,
			Automatic,
			Manual
		}

		private class ModuleSettings
		{
			[EnumToggleButtons]
			public ModuleAutomationSettings ModuleToggling;

			[EnumToggleButtons]
			public ModuleAutomationSettings ModuleUpdating;

			[OnInspectorGUI]
			[PropertyOrder(-1f)]
			private void OnInspectorGUI()
			{
				//IL_0015: Unknown result type (might be due to invalid IL or missing references)
				GUILayout.Label("Module Settings", ModuleDefinition.TitleStyle, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				SirenixEditorGUI.HorizontalLineSeparator(SirenixGUIStyles.BorderColor);
			}

			[OnInspectorGUI]
			private void DrawApplyButton()
			{
				GUILayout.FlexibleSpace();
				bool enabled = GlobalConfig<OdinModuleConfig>.Instance.ModuleTogglingSettings != ModuleToggling || GlobalConfig<OdinModuleConfig>.Instance.ModuleUpdateSettings != ModuleUpdating;
				GUIHelper.PushGUIEnabled(enabled);
				if (GUILayout.Button("Apply changes", (GUILayoutOption[])(object)new GUILayoutOption[0]))
				{
					GlobalConfig<OdinModuleConfig>.Instance.ModuleTogglingSettings = ModuleToggling;
					GlobalConfig<OdinModuleConfig>.Instance.ModuleUpdateSettings = ModuleUpdating;
					EditorUtility.SetDirty((Object)(object)GlobalConfig<OdinModuleConfig>.Instance);
					AssetDatabase.SaveAssets();
					RefreshModuleSetup();
					GUIUtility.ExitGUI();
				}
				GUIHelper.PopGUIEnabled();
			}
		}

		private class ModuleMenuItem : OdinMenuItem
		{
			private static GUIStyle backing_StatusStyle;

			private bool unstable;

			private string status;

			private float nextStatusUpdate;

			private static readonly OdinMenuStyle MenuItemStyle = new OdinMenuStyle
			{
				Height = 40,
				LabelVerticalOffset = -6f
			};

			private static readonly OdinMenuStyle UnstableMenuItemStyle = new OdinMenuStyle
			{
				Height = 52,
				LabelVerticalOffset = -12f
			};

			private static GUIStyle StatusStyle
			{
				get
				{
					//IL_000c: Unknown result type (might be due to invalid IL or missing references)
					//IL_0011: Unknown result type (might be due to invalid IL or missing references)
					//IL_001d: Expected O, but got Unknown
					if (backing_StatusStyle == null)
					{
						GUIStyle val = new GUIStyle(SirenixGUIStyles.LeftAlignedGreyMiniLabel);
						val.set_richText(true);
						backing_StatusStyle = val;
					}
					return backing_StatusStyle;
				}
			}

			public ModuleMenuItem(OdinMenuTree tree, string name, object value)
				: base(tree, name, value)
			{
				UpdateStatus();
			}

			private void UpdateStatus()
			{
				//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
				//IL_0149: Unknown result type (might be due to invalid IL or missing references)
				ModuleDefinition moduleDefinition = (ModuleDefinition)base.Value;
				ModuleManifest moduleManifest = moduleDefinition.LoadManifest();
				bool flag = moduleDefinition.CheckSupportsCurrentEnvironment();
				base.Style = (moduleDefinition.UnstableExperimental ? UnstableMenuItemStyle : MenuItemStyle);
				unstable = moduleDefinition.UnstableExperimental;
				if (moduleManifest != null)
				{
					bool flag2 = moduleManifest.Version < moduleDefinition.LatestVersion;
					if (flag2)
					{
						base.Icon = EditorIcons.ArrowUp.Active;
					}
					else
					{
						base.Icon = EditorIcons.Checkmark.Active;
					}
					if (flag)
					{
						status = string.Concat("Installed ( <color=#", ColorUtility.ToHtmlStringRGBA(flag2 ? new Color(0.9f, 0.45f, 0.01f, 1f) : new Color(0.1f, 0.9f, 0.1f, 1f)), ">", moduleManifest.Version, "</color> )");
					}
					else
					{
						status = "<color=#c0392b>Installed ( dependencies missing )</color>";
						base.Icon = (Texture)(object)EditorIcons.UnityErrorIcon;
					}
				}
				else if (flag)
				{
					base.Icon = EditorIcons.X.Active;
					status = "Inactive ( available: <color=#" + ColorUtility.ToHtmlStringRGBA(new Color(0.1f, 0.9f, 0.1f, 1f)) + ">" + (moduleDefinition.LatestVersion ?? new Version(0, 0, 0, 0)).ToString() + "</color> )";
				}
				else
				{
					base.Icon = EditorIcons.AlertCircle.Active;
					status = "Inactive ( <color=#c0392b>dependencies missing</color> )";
				}
				nextStatusUpdate = Time.get_realtimeSinceStartup() + 5f;
			}

			protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
			{
				//IL_002c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				//IL_003d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0043: Unknown result type (might be due to invalid IL or missing references)
				//IL_004d: Unknown result type (might be due to invalid IL or missing references)
				//IL_006a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0070: Unknown result type (might be due to invalid IL or missing references)
				//IL_007a: Unknown result type (might be due to invalid IL or missing references)
				if (Time.get_realtimeSinceStartup() > nextStatusUpdate)
				{
					UpdateStatus();
				}
				if (!EditorGUIUtility.get_isProSkin())
				{
					StatusStyle.get_normal().set_textColor(base.IsSelected ? Color.get_white() : Color.get_black());
				}
				GUI.Label(labelRect.AlignBottom(16f).AddY(12f), status, StatusStyle);
				if (unstable)
				{
					GUI.Label(labelRect.AlignBottom(16f).AddY(26f), "<color=#c0392b>EXPERIMENTAL & UNSTABLE</color>", StatusStyle);
				}
			}
		}

		private EditorTimeHelper timeHelper = new EditorTimeHelper();

		[SerializeField]
		[HideInInspector]
		private List<ModuleConfiguration> configurations = new List<ModuleConfiguration>();

		[NonSerialized]
		private ModuleManager backing_moduleManager;

		private OdinMenuTree moduleTree;

		private PropertyTree selectedModuleTree;

		private object nextSelection;

		private bool hasNextSelection;

		private float MenuWidth = 220f;

		private bool ResizableMenuWidth = true;

		[HideInInspector]
		public ModuleAutomationSettings ModuleTogglingSettings;

		[HideInInspector]
		public ModuleAutomationSettings ModuleUpdateSettings;

		private static bool initialized;

		private static bool editorWasCompilingLastUpdate;

		public ModuleManager ModuleManager
		{
			get
			{
				if (backing_moduleManager == null)
				{
					backing_moduleManager = ModuleManager.CreateDefault();
				}
				return backing_moduleManager;
			}
		}

		public ModuleConfiguration GetConfig(ModuleDefinition module)
		{
			if (!ModuleManager.Modules.Contains(module))
			{
				return null;
			}
			if (configurations == null)
			{
				configurations = new List<ModuleConfiguration>();
			}
			ModuleConfiguration moduleConfiguration = null;
			for (int i = 0; i < configurations.Count; i++)
			{
				ModuleConfiguration moduleConfiguration2 = configurations[i];
				if (moduleConfiguration2.ID == module.ID)
				{
					moduleConfiguration = moduleConfiguration2;
					continue;
				}
				bool flag = false;
				for (int j = 0; j < ModuleManager.Modules.Count; j++)
				{
					if (ModuleManager.Modules[j].ID == moduleConfiguration2.ID)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					configurations.RemoveAt(i);
					i--;
				}
			}
			if (moduleConfiguration != null)
			{
				return moduleConfiguration;
			}
			moduleConfiguration = new ModuleConfiguration
			{
				ID = module.ID
			};
			configurations.Add(moduleConfiguration);
			SaveAssetChanges();
			return moduleConfiguration;
		}

		public ModuleConfiguration GetConfig(string moduleID)
		{
			foreach (ModuleDefinition module in ModuleManager.Modules)
			{
				if (module.ID == moduleID)
				{
					return GetConfig(module);
				}
			}
			return null;
		}

		public void SaveAssetChanges()
		{
			EditorUtility.SetDirty((Object)(object)this);
			AssetDatabase.SaveAssets();
		}

		static OdinModuleConfig()
		{
			UnityEditorEventUtility.DelayAction(delegate
			{
				EnsureInitialized();
				if (!EditorApplication.get_isPlayingOrWillChangePlaymode())
				{
					RefreshModuleSetup();
				}
			});
		}

		private static void EnsureInitialized()
		{
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Expected O, but got Unknown
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Expected O, but got Unknown
			if (initialized)
			{
				return;
			}
			Type type = TwoWaySerializationBinder.Default.BindToType("UnityEditor.Compilation.CompilationPipeline");
			bool flag = false;
			if (type != null)
			{
				EventInfo @event = type.GetEvent("compilationFinished", BindingFlags.Static | BindingFlags.Public);
				if (@event != null)
				{
					@event.GetAddMethod(nonPublic: true).Invoke(null, new object[1] { (Action<object>)delegate
					{
						//IL_0019: Unknown result type (might be due to invalid IL or missing references)
						//IL_001e: Unknown result type (might be due to invalid IL or missing references)
						//IL_0024: Expected O, but got Unknown
						//IL_0029: Unknown result type (might be due to invalid IL or missing references)
						//IL_0033: Expected O, but got Unknown
						CallbackFunction delayCall = EditorApplication.delayCall;
						object obj = _003C_003Ec._003C_003E9__18_1;
						if (obj == null)
						{
							CallbackFunction val = delegate
							{
								TriggerModuleRefresh();
							};
							obj = (object)val;
							_003C_003Ec._003C_003E9__18_1 = val;
						}
						EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)delayCall, (Delegate)obj);
					} });
					flag = true;
				}
				else if (UnityVersion.IsVersionOrGreater(2019, 1))
				{
					Debug.LogWarning((object)"Failed to find UnityEditor.Compilation.CompilationPipeline.compilationStarted event - Odin module automation may be broken in this version of Unity...");
				}
			}
			if (!flag)
			{
				EditorApplication.update = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.update, (Delegate)new CallbackFunction(EditorUpdate));
			}
			initialized = true;
		}

		private void OnEnable()
		{
			EnsureInitialized();
			SelectLastSelectedItem();
		}

		private void SelectLastSelectedItem()
		{
			EnsureInitialized();
			if (!EditorPrefs.HasKey("ODIN_LastSelectedModule"))
			{
				return;
			}
			string lastSelection = EditorPrefs.GetString("ODIN_LastSelectedModule");
			if (lastSelection == "ModuleConfig")
			{
				nextSelection = "ModuleConfig";
				hasNextSelection = true;
				return;
			}
			nextSelection = ModuleManager.Modules.FirstOrDefault((ModuleDefinition n) => n.GetType().Name == lastSelection);
			hasNextSelection = true;
		}

		private static void EditorUpdate()
		{
			EnsureInitialized();
			bool isCompiling = EditorApplication.get_isCompiling();
			if (isCompiling && !editorWasCompilingLastUpdate)
			{
				TriggerModuleRefresh();
			}
			editorWasCompilingLastUpdate = isCompiling;
		}

		private static void TriggerModuleRefresh()
		{
			try
			{
				if (!EditorApplication.get_isPlayingOrWillChangePlaymode())
				{
					RefreshModuleSetup();
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void RefreshModuleSetup()
		{
			EnsureInitialized();
			OdinModuleConfig odinModuleConfig = GlobalConfig<OdinModuleConfig>.Instance;
			if ((Object)(object)odinModuleConfig == (Object)null)
			{
				Debug.LogWarning((object)"Couldn't load Odin Module Config asset; Odin module automation will not work...");
			}
			else if (odinModuleConfig.ModuleManager.Refresh())
			{
				AssetDatabase.Refresh((ImportAssetOptions)257);
				if ((Object)(object)odinModuleConfig != (Object)null && odinModuleConfig.moduleTree != null)
				{
					odinModuleConfig.moduleTree = null;
					odinModuleConfig.SelectLastSelectedItem();
				}
			}
		}

		[OnInspectorGUI]
		private void OnInspectorGUI()
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Invalid comparison between Unknown and I4
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0200: Unknown result type (might be due to invalid IL or missing references)
			//IL_0202: Unknown result type (might be due to invalid IL or missing references)
			//IL_0233: Unknown result type (might be due to invalid IL or missing references)
			//IL_0257: Unknown result type (might be due to invalid IL or missing references)
			//IL_025d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0267: Unknown result type (might be due to invalid IL or missing references)
			//IL_026c: Unknown result type (might be due to invalid IL or missing references)
			EnsureInitialized();
			if (moduleTree == null)
			{
				moduleTree = CreateModuleTree();
			}
			EditorTimeHelper time = EditorTimeHelper.Time;
			EditorTimeHelper.Time = timeHelper;
			EditorTimeHelper.Time.Update();
			try
			{
				if ((int)Event.get_current().get_type() == 8)
				{
					if (moduleTree == null)
					{
						moduleTree = CreateModuleTree();
					}
					if (hasNextSelection)
					{
						if (selectedModuleTree != null)
						{
							selectedModuleTree.Dispose();
							selectedModuleTree = null;
						}
						if (nextSelection != null)
						{
							if (nextSelection is string && (string)nextSelection == "ModuleConfig")
							{
								nextSelection = moduleTree.MenuItems[0].Value;
							}
							selectedModuleTree = PropertyTree.Create(nextSelection);
						}
						if (nextSelection is ModuleDefinition)
						{
							(nextSelection as ModuleDefinition).OnSelectedInInspector();
							EditorPrefs.SetString("ODIN_LastSelectedModule", nextSelection.GetType().Name);
						}
						else
						{
							EditorPrefs.SetString("ODIN_LastSelectedModule", "ModuleConfig");
						}
						if (moduleTree.Selection.Count != 1)
						{
							moduleTree.MenuItems.FirstOrDefault((OdinMenuItem n) => n.Value == nextSelection)?.Select();
						}
						nextSelection = null;
						hasNextSelection = false;
					}
				}
				GUILayout.Space(-4f);
				EditorGUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
				GUILayout.Space(-4f);
				Rect val = EditorGUILayout.BeginVertical((GUILayoutOption[])GUILayoutOptions.Width(MenuWidth).ExpandHeight());
				EditorGUI.DrawRect(val.AddXMin(-3f).AddYMax(3f), SirenixGUIStyles.MenuBackgroundColor);
				Rect val2 = val;
				((Rect)(ref val2)).set_xMin(((Rect)(ref val)).get_xMax() - 4f);
				((Rect)(ref val2)).set_xMax(((Rect)(ref val2)).get_xMax() + 4f);
				if (ResizableMenuWidth)
				{
					EditorGUIUtility.AddCursorRect(val2, (MouseCursor)3);
					MenuWidth += SirenixEditorGUI.SlideRect(val2, (MouseCursor)5).x;
				}
				moduleTree.DrawMenuTree();
				EditorGUILayout.EndVertical();
				EditorGUILayout.BeginVertical((GUILayoutOption[])GUILayoutOptions.ExpandWidth().ExpandHeight());
				if (selectedModuleTree != null)
				{
					selectedModuleTree.Draw(applyUndo: false);
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();
				EditorGUI.DrawRect(val2.AlignCenter(1f).AddYMax(4f), SirenixGUIStyles.BorderColor);
				if (moduleTree != null)
				{
					moduleTree.HandleKeyboardMenuNavigation();
				}
			}
			finally
			{
				EditorTimeHelper.Time = time;
			}
		}

		private OdinMenuTree CreateModuleTree()
		{
			OdinMenuTree tree = new OdinMenuTree();
			tree.Config.DrawSearchToolbar = ModuleManager.Modules.Count > 10;
			tree.Selection.SupportsMultiSelect = false;
			tree.MenuItems.Add(new OdinMenuItem(tree, "Module Settings", new ModuleSettings
			{
				ModuleToggling = ModuleTogglingSettings,
				ModuleUpdating = ModuleUpdateSettings
			})
			{
				Icon = EditorIcons.SettingsCog.Active
			});
			foreach (ModuleDefinition module in ModuleManager.Modules)
			{
				tree.MenuItems.Add(new ModuleMenuItem(tree, module.NiceName, module));
			}
			tree.Selection.SelectionChanged += delegate
			{
				OdinMenuItem odinMenuItem = tree.Selection.LastOrDefault();
				if (odinMenuItem != null)
				{
					nextSelection = odinMenuItem.Value;
				}
				else
				{
					nextSelection = null;
				}
				hasNextSelection = true;
			};
			return tree;
		}
	}
}
