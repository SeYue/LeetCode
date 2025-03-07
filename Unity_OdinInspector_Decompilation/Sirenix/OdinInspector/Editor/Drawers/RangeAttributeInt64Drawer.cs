using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws long properties marked with <see cref="T:UnityEngine.RangeAttribute" />.
	/// </summary>
	/// <seealso cref="T:UnityEngine.RangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	/// <seealso cref="T:UnityEngine.DelayedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	public sealed class RangeAttributeInt64Drawer : OdinAttributeDrawer<RangeAttribute, long>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<long> propertyValueEntry = base.ValueEntry;
			RangeAttribute val = base.Attribute;
			long num = propertyValueEntry.SmartValue;
			if (num < int.MinValue)
			{
				num = -2147483648L;
			}
			else if (num > int.MaxValue)
			{
				num = 2147483647L;
			}
			int num2 = SirenixEditorFields.RangeIntField(label, (int)num, (int)val.min, (int)val.max);
			propertyValueEntry.SmartValue = num2;
		}
	}
}
