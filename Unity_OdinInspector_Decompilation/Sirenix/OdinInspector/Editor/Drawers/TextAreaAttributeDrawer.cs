using System;
using System.Reflection;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// TextArea attribute drawer.
	/// </summary>
	public class TextAreaAttributeDrawer : OdinAttributeDrawer<TextAreaAttribute, string>
	{
		private delegate string ScrollableTextAreaInternalDelegate(Rect position, string text, ref Vector2 scrollPosition, GUIStyle style);

		private static readonly ScrollableTextAreaInternalDelegate EditorGUI_ScrollableTextAreaInternal;

		private static readonly FieldInfo EditorGUI_s_TextAreaHash_Field;

		private static readonly int EditorGUI_s_TextAreaHash;

		private Vector2 scrollPosition;

		static TextAreaAttributeDrawer()
		{
			MethodInfo method = typeof(EditorGUI).GetMethod("ScrollableTextAreaInternal", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (method != null)
			{
				EditorGUI_ScrollableTextAreaInternal = (ScrollableTextAreaInternalDelegate)Delegate.CreateDelegate(typeof(ScrollableTextAreaInternalDelegate), method);
			}
			EditorGUI_s_TextAreaHash_Field = typeof(EditorGUI).GetField("s_TextAreaHash", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (EditorGUI_s_TextAreaHash_Field != null)
			{
				EditorGUI_s_TextAreaHash = (int)EditorGUI_s_TextAreaHash_Field.GetValue(null);
			}
		}

		/// <summary>
		/// Draws the property in the Rect provided. This method does not support the GUILayout, and is only called by DrawPropertyImplementation if the GUICallType is set to Rect, which is not the default.
		/// If the GUICallType is set to Rect, both GetRectHeight and DrawPropertyRect needs to be implemented.
		/// If the GUICallType is set to GUILayout, implementing DrawPropertyLayout will suffice.
		/// </summary>
		/// <param name="label">The label. This can be null, so make sure your drawer supports that.</param>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Invalid comparison between Unknown and I4
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			IPropertyValueEntry<string> propertyValueEntry = base.ValueEntry;
			TextAreaAttribute val = base.Attribute;
			string smartValue = propertyValueEntry.SmartValue;
			float num = EditorStyles.get_textArea().CalcHeight(GUIHelper.TempContent(smartValue), GUIHelper.ContextWidth);
			int num2 = Mathf.Clamp(Mathf.CeilToInt(num / 13f), val.minLines, val.maxLines);
			float num3 = 32f + (float)((num2 - 1) * 13);
			Rect controlRect = EditorGUILayout.GetControlRect(label != null, num3, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			if (EditorGUI_ScrollableTextAreaInternal == null || EditorGUI_s_TextAreaHash_Field == null)
			{
				EditorGUI.LabelField(controlRect, label, GUIHelper.TempContent("Cannot draw TextArea because Unity's internal API has changed."));
				return;
			}
			if (label != null)
			{
				Rect rect = controlRect;
				((Rect)(ref rect)).set_height(16f);
				((Rect)(ref controlRect)).set_yMin(((Rect)(ref controlRect)).get_yMin() + ((Rect)(ref rect)).get_height());
				GUIHelper.IndentRect(ref rect);
				EditorGUI.HandlePrefixLabel(controlRect, rect, label);
			}
			if ((int)Event.get_current().get_type() == 8)
			{
				GUIUtility.GetControlID(EditorGUI_s_TextAreaHash, (FocusType)1, controlRect);
			}
			propertyValueEntry.SmartValue = EditorGUI_ScrollableTextAreaInternal(controlRect, propertyValueEntry.SmartValue, ref scrollPosition, EditorStyles.get_textArea());
		}
	}
}
