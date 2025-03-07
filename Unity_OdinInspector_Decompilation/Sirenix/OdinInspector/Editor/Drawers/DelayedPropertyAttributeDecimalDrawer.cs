using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws decimal properties marked with <see cref="T:Sirenix.OdinInspector.DelayedPropertyAttribute" />.
	/// </summary>
	public sealed class DelayedPropertyAttributeDecimalDrawer : OdinAttributeDrawer<DelayedPropertyAttribute, decimal>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			decimal result = base.ValueEntry.SmartValue;
			string value = result.ToString();
			value = SirenixEditorFields.DelayedTextField(label, value, (GUILayoutOption[])GUILayoutOptions.MinWidth(0f));
			if (GUI.get_changed() && decimal.TryParse(value, out result))
			{
				base.ValueEntry.SmartValue = result;
			}
		}
	}
}
