using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws double properties marked with <see cref="T:UnityEngine.RangeAttribute" />.
	/// </summary>
	/// <seealso cref="T:UnityEngine.RangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	/// <seealso cref="T:UnityEngine.DelayedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	public sealed class RangeAttributeDoubleDrawer : OdinAttributeDrawer<RangeAttribute, double>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<double> propertyValueEntry = base.ValueEntry;
			RangeAttribute val = base.Attribute;
			double num = propertyValueEntry.SmartValue;
			if (num < -3.4028234663852886E+38)
			{
				num = -3.4028234663852886E+38;
			}
			else if (num > 3.4028234663852886E+38)
			{
				num = 3.4028234663852886E+38;
			}
			EditorGUI.BeginChangeCheck();
			num = SirenixEditorFields.RangeFloatField(label, (float)num, val.min, val.max);
			if (EditorGUI.EndChangeCheck())
			{
				propertyValueEntry.SmartValue = num;
			}
		}
	}
}
