using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.DisplayAsStringAttribute" />.
	/// Calls the properties ToString method to get the string to draw.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.HideLabelAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.LabelTextAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.InfoBoxAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.DetailedInfoBoxAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MultiLinePropertyAttribute" />
	/// <seealso cref="T:UnityEngine.MultilineAttribute" />
	public sealed class DisplayAsStringAttributeDrawer<T> : OdinAttributeDrawer<DisplayAsStringAttribute, T>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			IPropertyValueEntry<T> propertyValueEntry = base.ValueEntry;
			DisplayAsStringAttribute displayAsStringAttribute = base.Attribute;
			if (propertyValueEntry.Property.ChildResolver is ICollectionResolver)
			{
				CallNextDrawer(label);
				return;
			}
			string text = ((propertyValueEntry.SmartValue == null) ? "Null" : propertyValueEntry.SmartValue.ToString());
			if (label == null)
			{
				EditorGUILayout.LabelField(text, (!displayAsStringAttribute.Overflow) ? SirenixGUIStyles.MultiLineLabel : EditorStyles.get_label(), (GUILayoutOption[])GUILayoutOptions.MinWidth(0f));
			}
			else if (!displayAsStringAttribute.Overflow)
			{
				GUIContent val = GUIHelper.TempContent(text);
				GUIStyle multiLineLabel = SirenixGUIStyles.MultiLineLabel;
				Rect lastDrawnValueRect = propertyValueEntry.Property.LastDrawnValueRect;
				Rect controlRect = EditorGUILayout.GetControlRect(false, multiLineLabel.CalcHeight(val, ((Rect)(ref lastDrawnValueRect)).get_width() - GUIHelper.BetterLabelWidth), (GUILayoutOption[])GUILayoutOptions.MinWidth(0f));
				Rect val2 = EditorGUI.PrefixLabel(controlRect, label);
				GUI.Label(val2, val, SirenixGUIStyles.MultiLineLabel);
			}
			else
			{
				SirenixEditorGUI.GetFeatureRichControlRect(label, out var _, out var _, out var valueRect);
				GUI.Label(valueRect, text);
			}
		}
	}
}
