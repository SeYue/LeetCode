using System;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws double properties marked with <see cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	/// <seealso cref="T:UnityEngine.DelayedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	public sealed class PropertyRangeAttributeDoubleDrawer : OdinAttributeDrawer<PropertyRangeAttribute, double>
	{
		private ValueResolver<double> getterMinValue;

		private ValueResolver<double> getterMaxValue;

		/// <summary>
		/// Initialized the drawer.
		/// </summary>
		protected override void Initialize()
		{
			if (base.Attribute.MinGetter != null)
			{
				getterMinValue = ValueResolver.Get<double>(base.Property, base.Attribute.MinGetter);
			}
			if (base.Attribute.MaxGetter != null)
			{
				getterMaxValue = ValueResolver.Get<double>(base.Property, base.Attribute.MaxGetter);
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			double num = base.ValueEntry.SmartValue;
			if (num < -3.4028234663852886E+38)
			{
				num = -3.4028234663852886E+38;
			}
			else if (num > 3.4028234663852886E+38)
			{
				num = 3.4028234663852886E+38;
			}
			double val = ((getterMinValue != null) ? getterMinValue.GetValue() : base.Attribute.Min);
			double val2 = ((getterMaxValue != null) ? getterMaxValue.GetValue() : base.Attribute.Max);
			if (getterMinValue != null && getterMinValue.ErrorMessage != null)
			{
				SirenixEditorGUI.ErrorMessageBox(getterMinValue.ErrorMessage);
			}
			if (getterMaxValue != null && getterMaxValue.ErrorMessage != null)
			{
				SirenixEditorGUI.ErrorMessageBox(getterMaxValue.ErrorMessage);
			}
			EditorGUI.BeginChangeCheck();
			num = SirenixEditorFields.RangeFloatField(label, (float)num, (float)Math.Min(val, val2), (float)Math.Max(val, val2));
			if (EditorGUI.EndChangeCheck())
			{
				base.ValueEntry.SmartValue = num;
			}
		}
	}
}
