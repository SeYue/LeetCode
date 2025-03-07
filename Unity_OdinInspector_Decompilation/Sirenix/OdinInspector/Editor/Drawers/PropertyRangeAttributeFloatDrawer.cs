using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws float properties marked with <see cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	/// <seealso cref="T:UnityEngine.DelayedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	public sealed class PropertyRangeAttributeFloatDrawer : OdinAttributeDrawer<PropertyRangeAttribute, float>
	{
		private ValueResolver<float> getterMinValue;

		private ValueResolver<float> getterMaxValue;

		/// <summary>
		/// Initialized the drawer.
		/// </summary>
		protected override void Initialize()
		{
			if (base.Attribute.MinGetter != null)
			{
				getterMinValue = ValueResolver.Get<float>(base.Property, base.Attribute.MinGetter);
			}
			if (base.Attribute.MaxGetter != null)
			{
				getterMaxValue = ValueResolver.Get<float>(base.Property, base.Attribute.MaxGetter);
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			float num = ((getterMinValue != null) ? getterMinValue.GetValue() : ((float)base.Attribute.Min));
			float num2 = ((getterMaxValue != null) ? getterMaxValue.GetValue() : ((float)base.Attribute.Max));
			if (getterMinValue != null && getterMinValue.ErrorMessage != null)
			{
				SirenixEditorGUI.ErrorMessageBox(getterMinValue.ErrorMessage);
			}
			if (getterMaxValue != null && getterMaxValue.ErrorMessage != null)
			{
				SirenixEditorGUI.ErrorMessageBox(getterMaxValue.ErrorMessage);
			}
			EditorGUI.BeginChangeCheck();
			float smartValue = SirenixEditorFields.RangeFloatField(label, base.ValueEntry.SmartValue, Mathf.Min(num, num2), Mathf.Max(num, num2));
			if (EditorGUI.EndChangeCheck())
			{
				base.ValueEntry.SmartValue = smartValue;
			}
		}
	}
}
