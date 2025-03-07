using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws uint properties marked with <see cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	/// <seealso cref="T:UnityEngine.DelayedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	public sealed class PropertyRangeAttributeUInt32Drawer : OdinAttributeDrawer<PropertyRangeAttribute, uint>
	{
		private ValueResolver<uint> getterMinValue;

		private ValueResolver<uint> getterMaxValue;

		/// <summary>
		/// Initialized the drawer.
		/// </summary>
		protected override void Initialize()
		{
			if (base.Attribute.MinGetter != null)
			{
				getterMinValue = ValueResolver.Get<uint>(base.Property, base.Attribute.MinGetter);
			}
			if (base.Attribute.MaxGetter != null)
			{
				getterMaxValue = ValueResolver.Get<uint>(base.Property, base.Attribute.MaxGetter);
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			uint num = ((getterMinValue != null) ? getterMinValue.GetValue() : ((uint)base.Attribute.Min));
			uint num2 = ((getterMaxValue != null) ? getterMaxValue.GetValue() : ((uint)base.Attribute.Max));
			if (getterMinValue != null && getterMinValue.ErrorMessage != null)
			{
				SirenixEditorGUI.ErrorMessageBox(getterMinValue.ErrorMessage);
			}
			if (getterMaxValue != null && getterMaxValue.ErrorMessage != null)
			{
				SirenixEditorGUI.ErrorMessageBox(getterMaxValue.ErrorMessage);
			}
			EditorGUI.BeginChangeCheck();
			int num3 = SirenixEditorFields.RangeIntField(label, (int)base.ValueEntry.SmartValue, (int)Mathf.Min((float)num, (float)num2), (int)Mathf.Max((float)num, (float)num2));
			if (EditorGUI.EndChangeCheck())
			{
				if ((long)num3 < 0L)
				{
					num3 = 0;
				}
				else
				{
					base.ValueEntry.SmartValue = (uint)num3;
				}
				base.ValueEntry.SmartValue = (uint)num3;
			}
		}
	}
}
