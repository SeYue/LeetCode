using System;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	internal class AcceptEULAWindow : OdinEditorWindow
	{
		private static int WIDTH = 600;

		private static int HEIGHT = 244;

		public static string HAS_ACCEPTED_EULA_PREFS_KEY = "ACCEPTED_ODIN_3_0_PERSONAL_EULA";

		[BoxGroup("Box", true, false, 0f, LabelText = "EULA Agreement Needed", CenterLabel = true)]
		[ShowInInspector]
		[ToggleLeft]
		[LabelText("I have read and understood the EULA, and the restrictions that apply to the use of Odin Personal")]
		private bool hasReadAndUnderstood;

		[BoxGroup("Box", true, false, 0f)]
		[ShowInInspector]
		[ToggleLeft]
		[LabelText("I or the entity I work for had less than $200,000 USD revenue or funding in the past 12 months")]
		private bool isUnderRevenueCap;

		private static bool IsHeadlessMode => (int)SystemInfo.get_graphicsDeviceType() == 4;

		public static bool HasAcceptedEULA => EditorPrefs.GetBool(HAS_ACCEPTED_EULA_PREFS_KEY, false);

		[InitializeOnLoadMethod]
		private static void OpenIfNotAccepted()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Expected O, but got Unknown
			EditorApplication.update = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.update, (Delegate)new CallbackFunction(OnUpdate));
		}

		private static void OnUpdate()
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Expected O, but got Unknown
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Expected O, but got Unknown
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Expected O, but got Unknown
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Expected O, but got Unknown
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Expected O, but got Unknown
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Expected O, but got Unknown
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Expected O, but got Unknown
			if (HasAcceptedEULA || IsHeadlessMode || InternalEditorUtility.get_inBatchMode())
			{
				EditorApplication.update = (CallbackFunction)Delegate.Remove((Delegate)(object)EditorApplication.update, (Delegate)new CallbackFunction(OnUpdate));
			}
			else
			{
				if (EditorApplication.get_isCompiling())
				{
					return;
				}
				try
				{
					AcceptEULAWindow window = EditorWindow.GetWindow<AcceptEULAWindow>();
					window.hasReadAndUnderstood = false;
					window.isUnderRevenueCap = false;
					((Object)window).set_name("EULA Agreement Needed");
					if (UnityVersion.IsVersionOrGreater(2019, 3))
					{
						HEIGHT += 2;
					}
					((EditorWindow)window).Show();
					((EditorWindow)window).set_minSize(new Vector2((float)WIDTH, (float)HEIGHT));
					((EditorWindow)window).set_maxSize(new Vector2((float)WIDTH, (float)HEIGHT));
					((EditorWindow)window).set_titleContent(new GUIContent("Odin EULA"));
					((EditorWindow)window).set_position(GUIHelper.GetEditorWindowRect().AlignCenter(WIDTH, HEIGHT));
					EditorApplication.update = (CallbackFunction)Delegate.Remove((Delegate)(object)EditorApplication.update, (Delegate)new CallbackFunction(OnUpdate));
				}
				catch (Exception innerException)
				{
					EditorApplication.update = (CallbackFunction)Delegate.Remove((Delegate)(object)EditorApplication.update, (Delegate)new CallbackFunction(OnUpdate));
					Debug.LogException(new Exception("An exception happened while attempting to open Odin's EULA popup window.", innerException));
				}
			}
		}

		[OnInspectorGUI]
		[PropertyOrder(-1f)]
		private void DrawEULA()
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Expected O, but got Unknown
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			GUI.set_color(new Color(1f, 1f, 1f, 0.15f));
			GUI.DrawTexture(new Rect((float)WIDTH - (float)HEIGHT * 1f, (float)HEIGHT * 0f, (float)HEIGHT * 1f, (float)HEIGHT * 1f), (Texture)(object)EditorIcons.OdinInspectorLogo, (ScaleMode)2, true);
			GUI.set_color(Color.get_white());
			GUIContent val = new GUIContent("Odin Personal EULA");
			GUILayout.Label(val, SirenixGUIStyles.SectionHeaderCentered, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			Rect lastRect = GUILayoutUtility.GetLastRect();
			float x = SirenixGUIStyles.SectionHeaderCentered.CalcSize(val).x;
			GUI.DrawTexture(lastRect.AlignCenter(40f, 40f).AddX(-25f + x * -0.5f), (Texture)(object)EditorIcons.OdinInspectorLogo, (ScaleMode)2);
			GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
			GUILayout.Space(8f);
			GUILayout.Label("In order to use Odin Personal, you must read and accept the Odin Personal EULA!\r\n\r\nMost notably, the EULA restricts the use of the Odin Personal license by people or entities with revenue or funding in excess of $200,000 USD in the past 12 months.", SirenixGUIStyles.MultiLineLabel, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			GUILayout.Space(8f);
			GUILayout.EndHorizontal();
			GUILayout.Space(5f);
			GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Read the full EULA", (GUILayoutOption[])GUILayoutOptions.Height(31f).Width(130f)))
			{
				Application.OpenURL("https://odininspector.com/eula");
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(10f);
		}

		[PropertySpace(5f)]
		[BoxGroup("Box", true, false, 0f)]
		[Button(ButtonSizes.Large)]
		[LabelText("I agree to the EULA")]
		[EnableIf("@hasReadAndUnderstood && isUnderRevenueCap")]
		private void Agree()
		{
			EditorPrefs.SetBool(HAS_ACCEPTED_EULA_PREFS_KEY, true);
			((EditorWindow)EditorWindow.GetWindow<AcceptEULAWindow>()).Close();
		}
	}
}
