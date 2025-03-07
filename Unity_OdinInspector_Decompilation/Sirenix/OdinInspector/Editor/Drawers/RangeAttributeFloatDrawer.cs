using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws float properties marked with <see cref="T:UnityEngine.RangeAttribute" />.
	/// </summary>
	/// <seealso cref="T:UnityEngine.RangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	/// <seealso cref="T:UnityEngine.DelayedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	public sealed class RangeAttributeFloatDrawer : OdinAttributeDrawer<RangeAttribute, float>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<float> propertyValueEntry = base.ValueEntry;
			RangeAttribute val = base.Attribute;
			propertyValueEntry.SmartValue = SirenixEditorFields.RangeFloatField(label, propertyValueEntry.SmartValue, val.min, val.max);
		}
	}
}
