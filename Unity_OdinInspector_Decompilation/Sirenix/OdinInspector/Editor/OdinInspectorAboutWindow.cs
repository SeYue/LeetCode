using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Adds menu items to the Unity Editor, draws the About window, and the preference window found under Edit &gt; Preferences &gt; Odin Inspector.
	/// </summary>
	public class OdinInspectorAboutWindow : EditorWindow
	{
		private void OnGUI()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			Rect position = ((EditorWindow)this).get_position();
			float num = ((Rect)(ref position)).get_width() - 20f;
			position = ((EditorWindow)this).get_position();
			GUILayout.BeginArea(new Rect(10f, 10f, num, ((Rect)(ref position)).get_height() - 5f));
			string buildName = OdinInspectorVersion.BuildName;
			SirenixEditorGUI.Title("Odin Inspector & Serializer", buildName, (TextAlignment)0, horizontalLine: true);
			if (OdinInspectorVersion.HasLicensee)
			{
				GUILayout.Label("Licensed to " + OdinInspectorVersion.Licensee, SirenixGUIStyles.LeftAlignedGreyMiniLabel, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			}
			DrawAboutGUI();
			GUILayout.EndArea();
			((EditorWindow)(object)this).RepaintIfRequested();
		}

		[PreferenceItem("Odin Inspector")]
		private static void OnPreferencesGUI()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			DrawAboutGUI();
			Rect controlRect = EditorGUILayout.GetControlRect((GUILayoutOption[])(object)new GUILayoutOption[0]);
			Rect val = default(Rect);
			((Rect)(ref val))._002Ector(controlRect);
			((Rect)(ref val)).set_y(((Rect)(ref controlRect)).get_y() + 70f);
			((Rect)(ref val)).set_height(25f);
			if (GUI.Button(val, "Get started using Odin"))
			{
				OdinGettingStartedWindow.ShowWindow();
			}
			((Rect)(ref val))._002Ector(controlRect);
			((Rect)(ref val)).set_y(((Rect)(ref controlRect)).get_y() + 70f + 30f);
			((Rect)(ref val)).set_height(25f);
			if (GUI.Button(val, "Show Odin Preferences"))
			{
				SirenixPreferencesWindow.OpenSirenixPreferences();
			}
			GUIHelper.CurrentWindow.RepaintIfRequested();
		}

		internal static void DrawAboutGUI()
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
			Rect val = default(Rect);
			((Rect)(ref val))._002Ector(EditorGUILayout.GetControlRect((GUILayoutOption[])(object)new GUILayoutOption[0]));
			((Rect)(ref val)).set_height(90f);
			Rect val2 = val;
			GUI.DrawTexture(val2.SetWidth(86f).SetHeight(75f).AddY(4f)
				.AddX(-5f), (Texture)(object)EditorIcons.OdinInspectorLogo, (ScaleMode)1);
			((Rect)(ref val))._002Ector(val2);
			((Rect)(ref val)).set_x(((Rect)(ref val2)).get_x() + 82f);
			((Rect)(ref val)).set_y(((Rect)(ref val2)).get_y() + 0f - 2f);
			((Rect)(ref val)).set_height(18f);
			GUI.Label(val, OdinInspectorVersion.Version, SirenixGUIStyles.LeftAlignedGreyMiniLabel);
			((Rect)(ref val))._002Ector(val2);
			((Rect)(ref val)).set_x(((Rect)(ref val2)).get_x() + 82f);
			((Rect)(ref val)).set_y(((Rect)(ref val2)).get_y() + 20f - 2f);
			((Rect)(ref val)).set_height(18f);
			GUI.Label(val, "Developed and published by Sirenix", SirenixGUIStyles.LeftAlignedGreyMiniLabel);
			((Rect)(ref val))._002Ector(val2);
			((Rect)(ref val)).set_x(((Rect)(ref val2)).get_x() + 82f);
			((Rect)(ref val)).set_y(((Rect)(ref val2)).get_y() + 40f - 2f);
			((Rect)(ref val)).set_height(18f);
			GUI.Label(val, "All rights reserved", SirenixGUIStyles.LeftAlignedGreyMiniLabel);
			GUIStyle miniButton = EditorStyles.get_miniButton();
			float x = miniButton.CalcSize(GUIHelper.TempContent("www.odininspector.com")).x;
			((Rect)(ref val))._002Ector(val2);
			((Rect)(ref val)).set_x(((Rect)(ref val2)).get_xMax() - x);
			((Rect)(ref val)).set_y(((Rect)(ref val2)).get_y() + 0f);
			((Rect)(ref val)).set_width(x);
			((Rect)(ref val)).set_height(14f);
			DrawLink(val, "www.odininspector.com", "https://odininspector.com", miniButton);
		}

		private static void DrawLink(Rect rect, string label, string link, GUIStyle style)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			if (GUI.Button(rect, label, style))
			{
				Application.OpenURL(link);
			}
		}

		public OdinInspectorAboutWindow()
			: this()
		{
		}
	}
}
