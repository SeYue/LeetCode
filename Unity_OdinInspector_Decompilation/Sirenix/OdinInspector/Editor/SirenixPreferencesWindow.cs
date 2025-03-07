using System;
using Sirenix.OdinInspector.Editor.Modules;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Sirenix preferences window.
	/// </summary>
	public class SirenixPreferencesWindow : OdinMenuEditorWindow
	{
		protected override OdinMenuTree BuildMenuTree()
		{
			return new OdinMenuTree(supportsMultiSelect: true)
			{
				{
					"General",
					GlobalConfig<GeneralDrawerConfig>.Instance
				},
				{
					"Editor Types",
					GlobalConfig<InspectorConfig>.Instance
				},
				{
					"Persistent Context Cache",
					PersistentContextCache.Instance
				},
				{
					"Color Palettes",
					GlobalConfig<ColorPaletteManager>.Instance
				},
				{
					"Serialization",
					GlobalConfig<GlobalSerializationConfig>.Instance
				},
				{
					"Import Settings",
					GlobalConfig<ImportSettingsConfig>.Instance
				},
				{
					"AOT Generation",
					GlobalConfig<AOTGenerationConfig>.Instance
				},
				{
					"Editor Only Mode",
					EditorOnlyModeConfig.Instance
				},
				{
					"Modules",
					GlobalConfig<OdinModuleConfig>.Instance
				}
			};
		}

		protected override void DrawMenu()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			base.DrawMenu();
			Rect val = GUIHelper.GetCurrentLayoutRect().Padding(4f).AlignBottom(20f);
			GUI.Label(val, "Odin Inspector Version " + OdinInspectorVersion.Version, SirenixGUIStyles.CenteredGreyMiniLabel);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			DefaultLabelWidth = 278f;
			ResizableMenuWidth = false;
		}

		/// <summary>
		/// Open preferences page for configuration object.
		/// </summary>
		[Obsolete("Use OpenWindow instead.")]
		public static void OpenGlobalConfigWindow<T>(string title, Object selectedConfig)
		{
			OpenWindow(selectedConfig);
		}

		/// <summary>
		/// Opens the Odin inspector preferences window.
		/// </summary>
		public static void OpenSirenixPreferences()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Expected O, but got Unknown
			SirenixPreferencesWindow window = EditorWindow.GetWindow<SirenixPreferencesWindow>();
			((EditorWindow)window).set_position(GUIHelper.GetEditorWindowRect().AlignCenter(900f, 600f));
			((EditorWindow)window).set_titleContent(new GUIContent("Preferences", (Texture)(object)EditorIcons.OdinInspectorLogo));
		}

		/// <summary>
		/// Opens the Odin inspector preferences window.
		/// </summary>
		public static void OpenWindow(Object selectedItem)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Expected O, but got Unknown
			SirenixPreferencesWindow window = EditorWindow.GetWindow<SirenixPreferencesWindow>();
			window.TrySelectMenuItemWithObject(selectedItem);
			((EditorWindow)window).set_titleContent(new GUIContent("Preferences", (Texture)(object)EditorIcons.OdinInspectorLogo));
		}
	}
}
