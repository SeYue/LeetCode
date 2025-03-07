using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws char properties marked with <see cref="T:Sirenix.OdinInspector.DelayedPropertyAttribute" />.
	/// </summary>
	public sealed class DelayedPropertyAttributeCharDrawer : OdinAttributeDrawer<DelayedPropertyAttribute, char>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			EditorGUI.BeginChangeCheck();
			string value = new string(base.ValueEntry.SmartValue, 1);
			value = SirenixEditorFields.DelayedTextField(label, value, (GUILayoutOption[])GUILayoutOptions.MinWidth(0f));
			if (EditorGUI.EndChangeCheck() && value.Length > 0)
			{
				base.ValueEntry.SmartValue = value[0];
			}
		}
	}
}
