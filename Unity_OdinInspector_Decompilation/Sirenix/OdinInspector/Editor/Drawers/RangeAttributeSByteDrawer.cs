using System;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws sbyte properties marked with <see cref="T:UnityEngine.RangeAttribute" />.
	/// </summary>
	/// <seealso cref="T:UnityEngine.RangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	/// <seealso cref="T:UnityEngine.DelayedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	public sealed class RangeAttributeSByteDrawer : OdinAttributeDrawer<RangeAttribute, sbyte>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<sbyte> propertyValueEntry = base.ValueEntry;
			RangeAttribute val = base.Attribute;
			int num = SirenixEditorFields.RangeIntField(label, propertyValueEntry.SmartValue, Math.Max(-128, (int)val.min), Math.Min(127, (int)val.max));
			if (num < -128)
			{
				num = -128;
			}
			else if (num > 127)
			{
				num = 127;
			}
			propertyValueEntry.SmartValue = (sbyte)num;
		}
	}
}
