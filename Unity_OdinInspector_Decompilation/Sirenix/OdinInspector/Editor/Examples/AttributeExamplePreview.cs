using System;
using System.IO;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	internal class AttributeExamplePreview
	{
		private static GUIStyle exampleGroupStyle;

		private static GUIStyle previewStyle;

		private static GUIStyle codeTextStyle;

		private static Color previewBackgroundColorDark = Color32.op_Implicit(new Color32((byte)56, (byte)56, (byte)56, byte.MaxValue));

		private static Color previewBackgroundColorLight = Color32.op_Implicit(new Color32((byte)194, (byte)194, (byte)194, byte.MaxValue));

		public AttributeExampleInfo ExampleInfo;

		private PropertyTree tree;

		private string highlightedCode;

		private string highlightedCodeAsComponent;

		private Vector2 scrollPosition;

		private bool showComponent;

		public AttributeExamplePreview(AttributeExampleInfo exampleInfo)
		{
			ExampleInfo = exampleInfo;
			try
			{
				highlightedCode = SyntaxHighlighter.Parse(ExampleInfo.Code);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				highlightedCode = ExampleInfo.Code;
			}
			try
			{
				highlightedCodeAsComponent = SyntaxHighlighter.Parse(ExampleInfo.CodeAsComponent);
			}
			catch (Exception ex2)
			{
				Debug.LogException(ex2);
				highlightedCodeAsComponent = ExampleInfo.CodeAsComponent;
			}
		}

		public void Draw(bool drawCodeExample)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0026: Expected O, but got Unknown
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Expected O, but got Unknown
			//IL_004b: Expected O, but got Unknown
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_026a: Unknown result type (might be due to invalid IL or missing references)
			//IL_026f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0271: Unknown result type (might be due to invalid IL or missing references)
			//IL_0278: Unknown result type (might be due to invalid IL or missing references)
			//IL_027d: Unknown result type (might be due to invalid IL or missing references)
			//IL_028d: Unknown result type (might be due to invalid IL or missing references)
			//IL_037a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0384: Expected O, but got Unknown
			//IL_038e: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0420: Unknown result type (might be due to invalid IL or missing references)
			//IL_0426: Invalid comparison between Unknown and I4
			//IL_042d: Unknown result type (might be due to invalid IL or missing references)
			//IL_043d: Unknown result type (might be due to invalid IL or missing references)
			//IL_045b: Unknown result type (might be due to invalid IL or missing references)
			//IL_046e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0473: Unknown result type (might be due to invalid IL or missing references)
			//IL_0478: Unknown result type (might be due to invalid IL or missing references)
			//IL_0485: Unknown result type (might be due to invalid IL or missing references)
			//IL_048c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0496: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a7: Unknown result type (might be due to invalid IL or missing references)
			if (exampleGroupStyle == null)
			{
				GUIStyle val = new GUIStyle(GUIStyle.get_none());
				val.set_padding(new RectOffset(1, 1, 10, 0));
				exampleGroupStyle = val;
			}
			if (previewStyle == null)
			{
				GUIStyle val2 = new GUIStyle(GUIStyle.get_none());
				val2.set_padding(new RectOffset(0, 0, 0, 0));
				previewStyle = val2;
			}
			GUILayout.BeginVertical(exampleGroupStyle, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			bool flag = false;
			if (ExampleInfo.Description != null)
			{
				SirenixEditorGUI.BeginBox();
				GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
				GUILayout.Space(23f);
				GUILayout.Label(ExampleInfo.Description, SirenixGUIStyles.MultiLineLabel, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				Rect lastRect = GUILayoutUtility.GetLastRect();
				EditorIcons.X.Draw(lastRect.SubX(26f).SetWidth(26f).AlignCenterXY(20f), EditorIcons.Info.Active);
				GUILayout.EndHorizontal();
				SirenixEditorGUI.EndBox();
				flag = true;
			}
			if (ExampleInfo.ExampleType.IsDefined(typeof(ShowOdinSerializedPropertiesInInspectorAttribute), inherit: false))
			{
				SirenixEditorGUI.BeginBox();
				GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
				GUILayout.Space(23f);
				GUILayout.Label("Note that this example requires Odin's serialization to be enabled to work, since it uses types that Unity will not serialize. If you copy the example as a component using the 'Copy Component' or 'Create Component Script' buttons, the code will have been set up with Odin's serialization enabled already.", SirenixGUIStyles.MultiLineLabel, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				Rect lastRect2 = GUILayoutUtility.GetLastRect();
				GUI.DrawTexture(lastRect2.SubX(26f).SetWidth(26f).AlignCenterXY(20f), (Texture)(object)EditorIcons.UnityWarningIcon);
				GUILayout.EndHorizontal();
				SirenixEditorGUI.EndBox();
				flag = true;
			}
			if (flag)
			{
				GUILayout.Space(12f);
			}
			GUILayout.Label("Interactive Preview", SirenixGUIStyles.BoldTitle, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			GUILayout.BeginVertical(previewStyle, (GUILayoutOption[])GUILayoutOptions.ExpandWidth());
			Rect rect = GUIHelper.GetCurrentLayoutRect().Expand(4f, 0f);
			SirenixEditorGUI.DrawSolidRect(rect, EditorGUIUtility.get_isProSkin() ? previewBackgroundColorDark : previewBackgroundColorLight);
			SirenixEditorGUI.DrawBorders(rect, 1);
			GUILayout.Space(8f);
			tree = tree ?? PropertyTree.Create(ExampleInfo.PreviewObject);
			tree.Draw(applyUndo: false);
			GUILayout.Space(8f);
			GUILayout.EndVertical();
			if (drawCodeExample && ExampleInfo.Code != null)
			{
				GUILayout.Space(12f);
				GUILayout.Label("Code", SirenixGUIStyles.BoldTitle, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				Rect rect2 = SirenixEditorGUI.BeginToolbarBox();
				SirenixEditorGUI.DrawSolidRect(rect2.HorizontalPadding(1f), SyntaxHighlighter.BackgroundColor);
				SirenixEditorGUI.BeginToolbarBoxHeader();
				if (SirenixEditorGUI.ToolbarButton(showComponent ? "View Shortened Code" : "View Component Code"))
				{
					showComponent = !showComponent;
				}
				GUILayout.FlexibleSpace();
				if (SirenixEditorGUI.ToolbarButton("Copy View"))
				{
					if (showComponent)
					{
						Clipboard.Copy(ExampleInfo.CodeAsComponent);
					}
					else
					{
						Clipboard.Copy(ExampleInfo.Code);
					}
				}
				if (ExampleInfo.CodeAsComponent != null && SirenixEditorGUI.ToolbarButton("Save Component Script"))
				{
					string text = EditorUtility.SaveFilePanelInProject("Create Component File", ExampleInfo.ExampleType.Name + "Component.cs", "cs", "Choose a location to save the example as a component script.");
					if (!string.IsNullOrEmpty(text))
					{
						File.WriteAllText(text, ExampleInfo.CodeAsComponent);
						AssetDatabase.Refresh();
					}
					GUIUtility.ExitGUI();
				}
				SirenixEditorGUI.EndToolbarBoxHeader();
				if (codeTextStyle == null)
				{
					codeTextStyle = new GUIStyle(SirenixGUIStyles.MultiLineLabel);
					codeTextStyle.get_normal().set_textColor(SyntaxHighlighter.TextColor);
					codeTextStyle.get_active().set_textColor(SyntaxHighlighter.TextColor);
					codeTextStyle.get_focused().set_textColor(SyntaxHighlighter.TextColor);
					codeTextStyle.set_wordWrap(false);
				}
				GUIContent val3 = (showComponent ? GUIHelper.TempContent(highlightedCodeAsComponent) : GUIHelper.TempContent(highlightedCode));
				Vector2 val4 = codeTextStyle.CalcSize(val3);
				GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
				GUILayout.Space(-3f);
				GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[0]);
				GUIHelper.PushEventType((EventType)(((int)Event.get_current().get_type() == 6) ? 12 : ((int)Event.get_current().get_type())));
				scrollPosition = GUILayout.BeginScrollView(scrollPosition, true, false, GUI.get_skin().get_horizontalScrollbar(), GUIStyle.get_none(), (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.MinHeight(val4.y + 20f) });
				Rect val5 = GUILayoutUtility.GetRect(val4.x + 50f, val4.y).AddXMin(4f).AddY(2f);
				GUI.Label(val5, val3, codeTextStyle);
				GUILayout.EndScrollView();
				GUIHelper.PopEventType();
				GUILayout.EndVertical();
				GUILayout.Space(-3f);
				GUILayout.EndHorizontal();
				GUILayout.Space(-3f);
				SirenixEditorGUI.EndToolbarBox();
			}
			GUILayout.EndVertical();
		}

		public void OnDeselected()
		{
			if (tree != null)
			{
				tree.Dispose();
				tree = null;
			}
		}
	}
}
