using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws decimal properties marked with <see cref="T:UnityEngine.RangeAttribute" />.
	/// </summary>
	public sealed class RangeAttributeDecimalDrawer : OdinAttributeDrawer<RangeAttribute, decimal>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<decimal> propertyValueEntry = base.ValueEntry;
			RangeAttribute val = base.Attribute;
			EditorGUI.BeginChangeCheck();
			float num = SirenixEditorFields.RangeFloatField(label, (float)propertyValueEntry.SmartValue, val.min, val.max);
			if (EditorGUI.EndChangeCheck())
			{
				propertyValueEntry.SmartValue = (decimal)num;
			}
		}
	}
}
