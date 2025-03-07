using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.ToggleLeftAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.ToggleLeftAttribute" />
	public sealed class ToggleLeftAttributeDrawer : OdinAttributeDrawer<ToggleLeftAttribute, bool>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<bool> propertyValueEntry = base.ValueEntry;
			propertyValueEntry.SmartValue = ((label == null) ? EditorGUILayout.ToggleLeft(GUIContent.none, propertyValueEntry.SmartValue, (GUILayoutOption[])(object)new GUILayoutOption[0]) : EditorGUILayout.ToggleLeft(label, propertyValueEntry.SmartValue, (GUILayoutOption[])(object)new GUILayoutOption[0]));
		}
	}
}
