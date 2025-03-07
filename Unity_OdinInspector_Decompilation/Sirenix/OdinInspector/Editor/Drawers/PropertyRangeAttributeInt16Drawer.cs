using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws short properties marked with <see cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	/// <seealso cref="T:UnityEngine.DelayedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	public sealed class PropertyRangeAttributeInt16Drawer : OdinAttributeDrawer<PropertyRangeAttribute, short>
	{
		private ValueResolver<short> getterMinValue;

		private ValueResolver<short> getterMaxValue;

		/// <summary>
		/// Initialized the drawer.
		/// </summary>
		protected override void Initialize()
		{
			if (base.Attribute.MinGetter != null)
			{
				getterMinValue = ValueResolver.Get<short>(base.Property, base.Attribute.MinGetter);
			}
			if (base.Attribute.MaxGetter != null)
			{
				getterMaxValue = ValueResolver.Get<short>(base.Property, base.Attribute.MaxGetter);
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			short num = ((getterMinValue != null) ? getterMinValue.GetValue() : ((short)base.Attribute.Min));
			short num2 = ((getterMaxValue != null) ? getterMaxValue.GetValue() : ((short)base.Attribute.Max));
			if (getterMinValue != null && getterMinValue.ErrorMessage != null)
			{
				SirenixEditorGUI.ErrorMessageBox(getterMinValue.ErrorMessage);
			}
			if (getterMaxValue != null && getterMaxValue.ErrorMessage != null)
			{
				SirenixEditorGUI.ErrorMessageBox(getterMaxValue.ErrorMessage);
			}
			EditorGUI.BeginChangeCheck();
			int num3 = SirenixEditorFields.RangeIntField(label, base.ValueEntry.SmartValue, Mathf.Min((int)num, (int)num2), Mathf.Max((int)num, (int)num2));
			if (EditorGUI.EndChangeCheck())
			{
				if (num3 < -32768)
				{
					num3 = -32768;
				}
				else if (num3 > 32767)
				{
					num3 = 32767;
				}
				base.ValueEntry.SmartValue = (short)num3;
			}
		}
	}
}
