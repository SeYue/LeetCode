using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Windows
{
	internal class OdinUnityContextMenuItems
	{
		private const int Group0 = -1000;

		private const int Group1 = 10000;

		private const int Group2 = 100000;

		private const int Group3 = 1000000;

		[MenuItem("Tools/Odin Inspector/Getting Started", priority = -999)]
		private static void OpenGettingStarted()
		{
			OdinGettingStartedWindow.ShowWindow();
		}

		[MenuItem("Tools/Odin Inspector/Attribute Overview", priority = -998)]
		public static void OpenAttributesOverview()
		{
			AttributesExampleWindow.OpenWindow(null);
		}

		[MenuItem("Tools/Odin Inspector/Static Inspector", priority = 10001)]
		private static void OpenStaticInspector()
		{
			StaticInspectorWindow.ShowWindow();
		}

		[MenuItem("Tools/Odin Inspector/Serialization Debugger", priority = 10002)]
		public static void ShowSerializationDebugger()
		{
			SerializationDebuggerWindow.ShowWindow();
		}

		[MenuItem("Tools/Odin Inspector/Preferences", priority = 100001)]
		public static void OpenSirenixPreferences()
		{
			SirenixPreferencesWindow.OpenSirenixPreferences();
		}

		[MenuItem("Tools/Odin Inspector/Help/Discord", priority = 1000001)]
		private static void Discord()
		{
			Application.OpenURL("https://discord.gg/WTYJEra");
		}

		[MenuItem("Tools/Odin Inspector/Help/Report An Issue", priority = 1000002)]
		private static void ReportAnIssue()
		{
			Application.OpenURL("https://bitbucket.org/sirenix/odin-inspector/issues");
		}

		[MenuItem("Tools/Odin Inspector/Help/Contact", priority = 1000003)]
		private static void Contact()
		{
			Application.OpenURL("https://odininspector.com/support");
		}

		[MenuItem("Tools/Odin Inspector/Release Notes", priority = 1000004)]
		private static void OpenReleaseNotes()
		{
			Application.OpenURL("https://odininspector.com/patch-notes");
		}

		[MenuItem("Tools/Odin Inspector/Check for updates", priority = 1000005)]
		private static void CheckForUpdates()
		{
			CheckForUpdatesWindow.OpenWindow();
		}

		[MenuItem("Tools/Odin Inspector/About", priority = 1000006)]
		private static void ShowAboutOdinInspector()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			Rect val = GUIHelper.GetEditorWindowRect().AlignCenter(465f).AlignMiddle(OdinInspectorVersion.HasLicensee ? 150f : 135f);
			OdinInspectorAboutWindow windowWithRect = EditorWindow.GetWindowWithRect<OdinInspectorAboutWindow>(val, true, "Odin Inspector & Serializer");
			((EditorWindow)windowWithRect).ShowUtility();
		}

		[MenuItem("CONTEXT/MonoBehaviour/Debug Serialization")]
		private static void ComponentContextMenuItem(MenuCommand menuCommand)
		{
			SerializationDebuggerWindow.ShowWindow(((object)menuCommand.context).GetType());
		}
	}
}
