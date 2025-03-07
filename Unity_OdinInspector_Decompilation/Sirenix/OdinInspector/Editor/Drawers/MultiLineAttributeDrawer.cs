using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws string properties marked with <see cref="T:UnityEngine.MultilineAttribute" />.
	/// This drawer only works for string fields, unlike <see cref="T:Sirenix.OdinInspector.Editor.Drawers.MultiLinePropertyAttributeDrawer" />.
	/// </summary>
	/// <seealso cref="T:UnityEngine.MultilineAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.Drawers.MultiLineAttributeDrawer" />
	/// <seealso cref="T:Sirenix.OdinInspector.DisplayAsStringAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.InfoBoxAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.DetailedInfoBoxAttribute" />
	public sealed class MultiLineAttributeDrawer : OdinAttributeDrawer<MultilineAttribute, string>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			IPropertyValueEntry<string> propertyValueEntry = base.ValueEntry;
			MultilineAttribute val = base.Attribute;
			Rect controlRect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.get_singleLineHeight() * (float)val.lines, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			((Rect)(ref controlRect)).set_height(((Rect)(ref controlRect)).get_height() - 2f);
			if (label == null)
			{
				propertyValueEntry.SmartValue = EditorGUI.TextArea(controlRect, propertyValueEntry.SmartValue, EditorStyles.get_textArea());
				return;
			}
			int controlID = GUIUtility.GetControlID(label, (FocusType)1, controlRect);
			Rect val2 = EditorGUI.PrefixLabel(controlRect, controlID, label, EditorStyles.get_label());
			propertyValueEntry.SmartValue = EditorGUI.TextArea(val2, propertyValueEntry.SmartValue, EditorStyles.get_textArea());
		}
	}
}
