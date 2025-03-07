using System;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws long properties marked with <see cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	/// <seealso cref="T:UnityEngine.DelayedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	public sealed class PropertyRangeAttributeInt64Drawer : OdinAttributeDrawer<PropertyRangeAttribute, long>
	{
		private ValueResolver<long> getterMinValue;

		private ValueResolver<long> getterMaxValue;

		/// <summary>
		/// Initialized the drawer.
		/// </summary>
		protected override void Initialize()
		{
			if (base.Attribute.MinGetter != null)
			{
				getterMinValue = ValueResolver.Get<long>(base.Property, base.Attribute.MinGetter);
			}
			if (base.Attribute.MaxGetter != null)
			{
				getterMaxValue = ValueResolver.Get<long>(base.Property, base.Attribute.MaxGetter);
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			long val = ((getterMinValue != null) ? getterMinValue.GetValue() : ((long)base.Attribute.Min));
			long val2 = ((getterMaxValue != null) ? getterMaxValue.GetValue() : ((long)base.Attribute.Max));
			if (getterMinValue != null && getterMinValue.ErrorMessage != null)
			{
				SirenixEditorGUI.ErrorMessageBox(getterMinValue.ErrorMessage);
			}
			if (getterMaxValue != null && getterMaxValue.ErrorMessage != null)
			{
				SirenixEditorGUI.ErrorMessageBox(getterMaxValue.ErrorMessage);
			}
			EditorGUI.BeginChangeCheck();
			int num = SirenixEditorFields.RangeIntField(label, (int)base.ValueEntry.SmartValue, (int)Math.Min(val, val2), (int)Math.Max(val, val2));
			if (EditorGUI.EndChangeCheck())
			{
				base.ValueEntry.SmartValue = num;
			}
		}
	}
}
