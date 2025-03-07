using System;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws uint properties marked with <see cref="T:UnityEngine.RangeAttribute" />.
	/// </summary>
	/// <seealso cref="T:UnityEngine.RangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	/// <seealso cref="T:UnityEngine.DelayedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	public sealed class RangeAttributeUInt32Drawer : OdinAttributeDrawer<RangeAttribute, uint>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<uint> propertyValueEntry = base.ValueEntry;
			RangeAttribute val = base.Attribute;
			uint num = propertyValueEntry.SmartValue;
			if (num > int.MaxValue)
			{
				num = 2147483647u;
			}
			int num2 = SirenixEditorFields.RangeIntField(label, (int)num, Math.Max(0, (int)val.min), (int)val.max);
			if (num2 < 0)
			{
				num2 = 0;
			}
			propertyValueEntry.SmartValue = (uint)num2;
		}
	}
}
