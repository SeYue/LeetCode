using System;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws short properties marked with <see cref="T:UnityEngine.RangeAttribute" />.
	/// </summary>
	/// <seealso cref="T:UnityEngine.RangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	/// <seealso cref="T:UnityEngine.DelayedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	public sealed class RangeAttributeInt16Drawer : OdinAttributeDrawer<RangeAttribute, short>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<short> propertyValueEntry = base.ValueEntry;
			RangeAttribute val = base.Attribute;
			int num = SirenixEditorFields.RangeIntField(label, propertyValueEntry.SmartValue, Math.Max(-32768, (int)val.min), Math.Min(32767, (int)val.max));
			if (num < -32768)
			{
				num = -32768;
			}
			else if (num > 32767)
			{
				num = 32767;
			}
			propertyValueEntry.SmartValue = (short)num;
		}
	}
}
