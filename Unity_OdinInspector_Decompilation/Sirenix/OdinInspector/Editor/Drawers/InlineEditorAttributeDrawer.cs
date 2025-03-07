using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Static GUI information reguarding the InlineEditor attribute.
	/// </summary>
	public static class InlineEditorAttributeDrawer
	{
		/// <summary>
		/// Gets a value indicating how many InlineEditors we are currently in.
		/// </summary>
		public static int CurrentInlineEditorDrawDepth { get; internal set; }
	}
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.InlineEditorAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.InlineEditorAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.DrawWithUnityAttribute" />
	[DrawerPriority(0.0, 0.0, 3000.0)]
	public class InlineEditorAttributeDrawer<T> : OdinAttributeDrawer<InlineEditorAttribute, T>, IDisposable where T : Object
	{
		private struct LayoutSettings
		{
			public GUISkin Skin;

			public Color Color;

			public Color ContentColor;

			public Color BackgroundColor;

			public bool Enabled;

			public int IndentLevel;

			public float FieldWidth;

			public float LabelWidth;

			public bool HierarchyMode;

			public bool WideMode;
		}

		public static readonly bool IsGameObject = typeof(T) == typeof(GameObject);

		private static PropertyInfo materialForceVisibleProperty = typeof(MaterialEditor).GetProperty("forceVisible", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

		private static Stack<LayoutSettings> layoutSettingsStack = new Stack<LayoutSettings>();

		private Editor editor;

		private Editor previewEditor;

		private Object target;

		private Rect inlineEditorRect;

		private Vector2 scrollPos;

		private bool allowSceneObjects;

		private bool drawHeader;

		private bool drawGUI;

		private bool drawPreview;

		private bool alwaysVisible;

		private bool targetIsOpenForEdit;

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		protected override void Initialize()
		{
			if (base.Attribute.ExpandedHasValue && InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth == 0)
			{
				base.Property.State.Expanded = base.Attribute.Expanded;
			}
			allowSceneObjects = !base.Property.Attributes.OfType<AssetsOnlyAttribute>().Any();
		}

		/// <summary>
		/// Draws the property layout.
		/// </summary>
		/// <param name="label">The label.</param>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f8: Expected O, but got Unknown
			Rect valueRect;
			switch (base.Attribute.ObjectFieldMode)
			{
			case InlineEditorObjectFieldModes.Boxed:
				alwaysVisible = false;
				SirenixEditorGUI.BeginToolbarBox();
				SirenixEditorGUI.BeginToolbarBoxHeader();
				if (Object.op_Implicit((Object)(object)base.ValueEntry.SmartValue))
				{
					base.Property.State.Expanded = SirenixEditorGUI.Foldout(base.Property.State.Expanded, label, out valueRect);
					base.ValueEntry.WeakSmartValue = SirenixEditorFields.UnityObjectField(valueRect, (Object)(object)base.ValueEntry.SmartValue, base.ValueEntry.BaseValueType, allowSceneObjects);
				}
				else
				{
					base.ValueEntry.WeakSmartValue = SirenixEditorFields.UnityObjectField(label, (Object)(object)base.ValueEntry.SmartValue, base.ValueEntry.BaseValueType, allowSceneObjects);
				}
				SirenixEditorGUI.EndToolbarBoxHeader();
				GUIHelper.PushHierarchyMode(hierarchyMode: false);
				DrawEditor();
				GUIHelper.PopHierarchyMode();
				SirenixEditorGUI.EndToolbarBox();
				break;
			case InlineEditorObjectFieldModes.Foldout:
				alwaysVisible = false;
				if (Object.op_Implicit((Object)(object)base.ValueEntry.SmartValue))
				{
					base.Property.State.Expanded = SirenixEditorGUI.Foldout(base.Property.State.Expanded, label, out valueRect);
					base.ValueEntry.WeakSmartValue = SirenixEditorFields.UnityObjectField(valueRect, (Object)(object)base.ValueEntry.SmartValue, base.ValueEntry.BaseValueType, allowSceneObjects);
				}
				else
				{
					base.ValueEntry.WeakSmartValue = SirenixEditorFields.UnityObjectField(label, (Object)(object)base.ValueEntry.SmartValue, base.ValueEntry.BaseValueType, allowSceneObjects);
				}
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
				DrawEditor();
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
				break;
			case InlineEditorObjectFieldModes.Hidden:
				alwaysVisible = true;
				if (!Object.op_Implicit((Object)base.ValueEntry.WeakSmartValue))
				{
					base.ValueEntry.WeakSmartValue = SirenixEditorFields.UnityObjectField(label, (Object)(object)base.ValueEntry.SmartValue, base.ValueEntry.BaseValueType, allowSceneObjects);
				}
				DrawEditor();
				break;
			case InlineEditorObjectFieldModes.CompletelyHidden:
				alwaysVisible = true;
				DrawEditor();
				break;
			}
		}

		private void DrawEditor()
		{
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			T smartValue = base.ValueEntry.SmartValue;
			if (base.ValueEntry.ValueState == PropertyValueState.ReferencePathConflict)
			{
				SirenixEditorGUI.InfoMessageBox("reference-path-conflict");
				return;
			}
			if (alwaysVisible || SirenixEditorGUI.BeginFadeGroup(this, base.Property.State.Expanded))
			{
				UpdateEditors();
				if (base.Attribute.MaxHeight != 0f)
				{
					scrollPos = EditorGUILayout.BeginScrollView(scrollPos, (GUILayoutOption[])GUILayoutOptions.MaxHeight(200f));
				}
				bool showMixedValue = EditorGUI.get_showMixedValue();
				EditorGUI.set_showMixedValue(false);
				EditorGUI.BeginChangeCheck();
				DoTheDrawing();
				if (EditorGUI.EndChangeCheck())
				{
					PropertyValueEntry baseValueEntry = base.Property.BaseValueEntry;
					if (baseValueEntry != null)
					{
						for (int i = 0; i < baseValueEntry.ValueCount; i++)
						{
							baseValueEntry.TriggerOnChildValueChanged(i);
						}
					}
				}
				EditorGUI.set_showMixedValue(showMixedValue);
				if (base.Attribute.MaxHeight != 0f)
				{
					EditorGUILayout.EndScrollView();
				}
			}
			else if ((Object)(object)editor != (Object)null)
			{
				DestroyEditors();
			}
			if (!alwaysVisible)
			{
				SirenixEditorGUI.EndFadeGroup();
			}
		}

		private void DoTheDrawing()
		{
			//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c0: Invalid comparison between Unknown and I4
			//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0314: Unknown result type (might be due to invalid IL or missing references)
			//IL_0319: Unknown result type (might be due to invalid IL or missing references)
			//IL_0323: Unknown result type (might be due to invalid IL or missing references)
			//IL_0329: Invalid comparison between Unknown and I4
			//IL_033e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0340: Unknown result type (might be due to invalid IL or missing references)
			//IL_0346: Unknown result type (might be due to invalid IL or missing references)
			//IL_034b: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
			if (IsGameObject && !base.Attribute.DrawPreview)
			{
				SirenixEditorGUI.MessageBox("Odin does not currently have a full GameObject inspector window substitute implemented, so a GameObject's components cannot be directly inspected inline in the editor. Choose an InlineEditorMode that includes a preview to draw a GameObject preview.");
				SirenixEditorFields.UnityObjectField((Object)(object)base.ValueEntry.SmartValue, typeof(GameObject), true);
				GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
				GUIHelper.PushGUIEnabled((Object)(object)base.ValueEntry.SmartValue != (Object)null);
				string t = (((Object)(object)base.ValueEntry.SmartValue != (Object)null) ? ("Open Inspector window for " + ((Object)base.ValueEntry.SmartValue).get_name()) : "Open Inspector window (null)");
				if (GUILayout.Button(GUIHelper.TempContent(t), (GUILayoutOption[])(object)new GUILayoutOption[0]))
				{
					GUIHelper.OpenInspectorWindow((Object)(object)base.ValueEntry.SmartValue);
					GUIUtility.ExitGUI();
				}
				t = (((Object)(object)base.ValueEntry.SmartValue != (Object)null) ? ("Select " + ((Object)base.ValueEntry.SmartValue).get_name()) : "Select GO (null)");
				if (GUILayout.Button(GUIHelper.TempContent(t), (GUILayoutOption[])(object)new GUILayoutOption[0]))
				{
					Selection.set_activeObject((Object)(object)base.ValueEntry.SmartValue);
					GUIUtility.ExitGUI();
				}
				GUIHelper.PopGUIEnabled();
				GUILayout.EndHorizontal();
			}
			else
			{
				if (!((Object)(object)editor != (Object)null) || ((Object)(object)editor).SafeIsUnityNull())
				{
					return;
				}
				SaveLayoutSettings();
				InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth++;
				try
				{
					if (!targetIsOpenForEdit)
					{
						GUIHelper.PushGUIEnabled(enabled: false);
					}
					bool flag = drawGUI && drawPreview;
					if (flag)
					{
						GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
						if ((int)Event.get_current().get_type() == 7)
						{
							inlineEditorRect = GUIHelper.GetCurrentLayoutRect();
						}
						GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[0]);
					}
					if (drawHeader)
					{
						EventType rawType = Event.get_current().get_rawType();
						EditorGUILayout.BeginFadeGroup(0.9999f);
						Event.get_current().set_type(rawType);
						GUILayout.Space(0f);
						editor.DrawHeader();
						GUILayout.Space(1f);
						EditorGUILayout.EndFadeGroup();
					}
					else
					{
						GUIHelper.BeginDrawToNothing();
						editor.DrawHeader();
						GUIHelper.EndDrawToNothing();
					}
					if (drawGUI)
					{
						bool showMonoScriptInEditor = GlobalConfig<GeneralDrawerConfig>.Instance.ShowMonoScriptInEditor;
						try
						{
							GlobalConfig<GeneralDrawerConfig>.Instance.ShowMonoScriptInEditor = false;
							editor.OnInspectorGUI();
						}
						finally
						{
							GlobalConfig<GeneralDrawerConfig>.Instance.ShowMonoScriptInEditor = showMonoScriptInEditor;
						}
					}
					if (flag)
					{
						GUILayout.EndVertical();
					}
					if (!targetIsOpenForEdit)
					{
						GUIHelper.PopGUIEnabled();
					}
					if (drawPreview && (previewEditor.HasPreviewGUI() || previewEditor.get_target() is GameObject))
					{
						float num = (flag ? base.Attribute.PreviewWidth : base.Attribute.PreviewHeight);
						Rect val = ((!flag) ? GUILayoutUtility.GetRect(0f, num, (GUILayoutOption[])GUILayoutOptions.Height(num).ExpandWidth()) : GUILayoutUtility.GetRect(num + 15f, num, (GUILayoutOption[])GUILayoutOptions.Width(num).Height(num)));
						if ((!flag && (int)Event.get_current().get_type() == 7) || ((Rect)(ref inlineEditorRect)).get_height() < 1f)
						{
							inlineEditorRect = val;
						}
						Rect val2 = inlineEditorRect;
						if (flag)
						{
							((Rect)(ref val2)).set_xMin(((Rect)(ref val2)).get_xMin() + (((Rect)(ref val2)).get_width() - num));
						}
						((Rect)(ref val2)).set_height(Mathf.Clamp(((Rect)(ref val2)).get_height(), 30f, 1000f));
						((Rect)(ref val2)).set_width(Mathf.Clamp(((Rect)(ref val2)).get_width(), 30f, 1000f));
						bool enabled = GUI.get_enabled();
						GUI.set_enabled(true);
						previewEditor.DrawPreview(val2);
						GUI.set_enabled(enabled);
					}
					if (flag)
					{
						GUILayout.EndHorizontal();
					}
				}
				catch (Exception ex)
				{
					if (ex.IsExitGUIException())
					{
						throw ex.AsExitGUIException();
					}
					Debug.LogException(ex);
				}
				finally
				{
					InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth--;
					RestoreLayout();
				}
			}
		}

		private void UpdateEditors()
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected O, but got Unknown
			targetIsOpenForEdit = true;
			Object val = (Object)base.ValueEntry.WeakSmartValue;
			if ((Object)(object)editor != (Object)null && !Object.op_Implicit(val))
			{
				DestroyEditors();
			}
			bool flag = val != (Object)null && ((Object)(object)editor == (Object)null || target != val || target == (Object)null);
			if (flag && base.ValueEntry.ValueState == PropertyValueState.ReferenceValueConflict)
			{
				if (base.ValueEntry.WeakValues[0] == null)
				{
					flag = false;
				}
				if (flag)
				{
					Type type = base.ValueEntry.WeakValues[0].GetType();
					for (int i = 1; i < base.ValueEntry.ValueCount; i++)
					{
						if (!Object.op_Implicit((Object)(object)base.ValueEntry.Values[i]) || ((object)base.ValueEntry.Values[i]).GetType() != type)
						{
							flag = false;
							break;
						}
					}
				}
				if (!flag)
				{
					SirenixEditorGUI.InfoMessageBox("Cannot perform multi-editing on objects of different type.");
				}
			}
			if (flag)
			{
				target = val;
				bool flag2 = Object.op_Implicit((Object)(val as GameObject));
				drawHeader = (flag2 ? base.Attribute.DrawHeader : base.Attribute.DrawHeader);
				drawGUI = !flag2 && base.Attribute.DrawGUI;
				drawPreview = base.Attribute.DrawPreview || (flag2 && base.Attribute.DrawGUI);
				if ((Object)(object)editor != (Object)null)
				{
					DestroyEditors();
				}
				editor = Editor.CreateEditor(base.ValueEntry.WeakValues.FilterCast<Object>().ToArray());
				Object obj = target;
				Component val2 = obj as Component;
				if ((Object)(object)val2 != (Object)null)
				{
					previewEditor = Editor.CreateEditor((Object)(object)val2.get_gameObject());
				}
				else
				{
					previewEditor = editor;
				}
				Editor obj2 = editor;
				MaterialEditor val3 = obj2 as MaterialEditor;
				if ((Object)(object)val3 != (Object)null && materialForceVisibleProperty != null)
				{
					materialForceVisibleProperty.SetValue(val3, true, null);
				}
				if (base.Attribute.DisableGUIForVCSLockedAssets && AssetDatabase.Contains(target))
				{
					targetIsOpenForEdit = AssetDatabase.IsOpenForEdit(target);
				}
			}
		}

		private void DestroyEditors()
		{
			targetIsOpenForEdit = true;
			if ((Object)(object)previewEditor != (Object)(object)editor && (Object)(object)previewEditor != (Object)null)
			{
				try
				{
					Object.DestroyImmediate((Object)(object)previewEditor);
				}
				catch
				{
				}
				previewEditor = null;
			}
			if ((Object)(object)editor != (Object)null)
			{
				try
				{
					Object.DestroyImmediate((Object)(object)editor);
				}
				catch
				{
				}
				editor = null;
			}
		}

		private static void SaveLayoutSettings()
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			layoutSettingsStack.Push(new LayoutSettings
			{
				Skin = GUI.get_skin(),
				Color = GUI.get_color(),
				ContentColor = GUI.get_contentColor(),
				BackgroundColor = GUI.get_backgroundColor(),
				Enabled = GUI.get_enabled(),
				IndentLevel = EditorGUI.get_indentLevel(),
				FieldWidth = EditorGUIUtility.get_fieldWidth(),
				LabelWidth = GUIHelper.ActualLabelWidth,
				HierarchyMode = EditorGUIUtility.get_hierarchyMode(),
				WideMode = EditorGUIUtility.get_wideMode()
			});
		}

		private static void RestoreLayout()
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			LayoutSettings layoutSettings = layoutSettingsStack.Pop();
			GUI.set_skin(layoutSettings.Skin);
			GUI.set_color(layoutSettings.Color);
			GUI.set_contentColor(layoutSettings.ContentColor);
			GUI.set_backgroundColor(layoutSettings.BackgroundColor);
			GUI.set_enabled(layoutSettings.Enabled);
			EditorGUI.set_indentLevel(layoutSettings.IndentLevel);
			EditorGUIUtility.set_fieldWidth(layoutSettings.FieldWidth);
			GUIHelper.BetterLabelWidth = layoutSettings.LabelWidth;
			EditorGUIUtility.set_hierarchyMode(layoutSettings.HierarchyMode);
			EditorGUIUtility.set_wideMode(layoutSettings.WideMode);
		}

		void IDisposable.Dispose()
		{
			DestroyEditors();
		}
	}
}
