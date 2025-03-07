using System;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws ushort properties marked with <see cref="T:UnityEngine.RangeAttribute" />.
	/// </summary>
	/// <seealso cref="T:UnityEngine.RangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	/// <seealso cref="T:UnityEngine.DelayedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	public sealed class RangeAttributeUInt16Drawer : OdinAttributeDrawer<RangeAttribute, ushort>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<ushort> propertyValueEntry = base.ValueEntry;
			RangeAttribute val = base.Attribute;
			int num = SirenixEditorFields.RangeIntField(label, propertyValueEntry.SmartValue, Math.Max(0, (int)val.min), Math.Min(65535, (int)val.max));
			if (num < 0)
			{
				num = 0;
			}
			else if (num > 65535)
			{
				num = 65535;
			}
			propertyValueEntry.SmartValue = (ushort)num;
		}
	}
}
