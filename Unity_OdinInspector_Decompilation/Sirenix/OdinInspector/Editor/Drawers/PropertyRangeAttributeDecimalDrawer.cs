using System;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws decimal properties marked with <see cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />.
	/// </summary>
	public sealed class PropertyRangeAttributeDecimalDrawer : OdinAttributeDrawer<PropertyRangeAttribute, decimal>
	{
		private ValueResolver<decimal> getterMinValue;

		private ValueResolver<decimal> getterMaxValue;

		/// <summary>
		/// Initialized the drawer.
		/// </summary>
		protected override void Initialize()
		{
			if (base.Attribute.MinGetter != null)
			{
				getterMinValue = ValueResolver.Get<decimal>(base.Property, base.Attribute.MinGetter);
			}
			if (base.Attribute.MaxGetter != null)
			{
				getterMaxValue = ValueResolver.Get<decimal>(base.Property, base.Attribute.MaxGetter);
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			decimal val = ((getterMinValue != null) ? getterMinValue.GetValue() : ((decimal)base.Attribute.Min));
			decimal val2 = ((getterMaxValue != null) ? getterMaxValue.GetValue() : ((decimal)base.Attribute.Max));
			if (getterMinValue != null && getterMinValue.ErrorMessage != null)
			{
				SirenixEditorGUI.ErrorMessageBox(getterMinValue.ErrorMessage);
			}
			if (getterMaxValue != null && getterMaxValue.ErrorMessage != null)
			{
				SirenixEditorGUI.ErrorMessageBox(getterMaxValue.ErrorMessage);
			}
			EditorGUI.BeginChangeCheck();
			float num = SirenixEditorFields.RangeFloatField(label, (float)base.ValueEntry.SmartValue, (float)Math.Min(val, val2), (float)Math.Max(val, val2));
			if (EditorGUI.EndChangeCheck())
			{
				base.ValueEntry.SmartValue = (decimal)num;
			}
		}
	}
}
