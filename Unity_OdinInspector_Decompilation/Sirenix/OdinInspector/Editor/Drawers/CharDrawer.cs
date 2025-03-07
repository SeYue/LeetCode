using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Char property drawer.
	/// </summary>
	public sealed class CharDrawer : OdinValueDrawer<char>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<char> propertyValueEntry = base.ValueEntry;
			EditorGUI.BeginChangeCheck();
			string value = new string(propertyValueEntry.SmartValue, 1);
			value = SirenixEditorFields.TextField(label, value);
			if (EditorGUI.EndChangeCheck() && value.Length > 0)
			{
				propertyValueEntry.SmartValue = value[0];
			}
		}
	}
}
