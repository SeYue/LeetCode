using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws int properties marked with <see cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	/// <seealso cref="T:UnityEngine.DelayedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	public sealed class PropertyRangeAttributeInt32Drawer : OdinAttributeDrawer<PropertyRangeAttribute, int>
	{
		private ValueResolver<int> getterMinValue;

		private ValueResolver<int> getterMaxValue;

		/// <summary>
		/// Initialized the drawer.
		/// </summary>
		protected override void Initialize()
		{
			if (base.Attribute.MinGetter != null)
			{
				getterMinValue = ValueResolver.Get<int>(base.Property, base.Attribute.MinGetter);
			}
			if (base.Attribute.MaxGetter != null)
			{
				getterMaxValue = ValueResolver.Get<int>(base.Property, base.Attribute.MaxGetter);
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			int num = ((getterMinValue != null) ? getterMinValue.GetValue() : ((int)base.Attribute.Min));
			int num2 = ((getterMaxValue != null) ? getterMaxValue.GetValue() : ((int)base.Attribute.Max));
			if (getterMinValue != null && getterMinValue.ErrorMessage != null)
			{
				SirenixEditorGUI.ErrorMessageBox(getterMinValue.ErrorMessage);
			}
			if (getterMaxValue != null && getterMaxValue.ErrorMessage != null)
			{
				SirenixEditorGUI.ErrorMessageBox(getterMaxValue.ErrorMessage);
			}
			EditorGUI.BeginChangeCheck();
			int num3 = SirenixEditorFields.RangeIntField(label, base.ValueEntry.SmartValue, Mathf.Min(num, num2), Mathf.Max(num, num2));
			if (EditorGUI.EndChangeCheck())
			{
				if (num3 < int.MinValue)
				{
					num3 = int.MinValue;
				}
				else if (num3 > int.MaxValue)
				{
					num3 = int.MaxValue;
				}
				base.ValueEntry.SmartValue = num3;
			}
		}
	}
}
