using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// String property drawer.
	/// </summary>
	public sealed class StringDrawer : OdinValueDrawer<string>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<string> propertyValueEntry = base.ValueEntry;
			propertyValueEntry.SmartValue = ((label == null) ? EditorGUILayout.TextField(propertyValueEntry.SmartValue, EditorStyles.get_textField(), (GUILayoutOption[])(object)new GUILayoutOption[0]) : EditorGUILayout.TextField(label, propertyValueEntry.SmartValue, EditorStyles.get_textField(), (GUILayoutOption[])(object)new GUILayoutOption[0]));
		}
	}
}
