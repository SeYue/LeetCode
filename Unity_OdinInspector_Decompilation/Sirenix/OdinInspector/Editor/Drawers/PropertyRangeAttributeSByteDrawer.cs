using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws sbyte properties marked with <see cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	/// <seealso cref="T:UnityEngine.DelayedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	public sealed class PropertyRangeAttributeSByteDrawer : OdinAttributeDrawer<PropertyRangeAttribute, sbyte>
	{
		private ValueResolver<sbyte> getterMinValue;

		private ValueResolver<sbyte> getterMaxValue;

		/// <summary>
		/// Initialized the drawer.
		/// </summary>
		protected override void Initialize()
		{
			if (base.Attribute.MinGetter != null)
			{
				getterMinValue = ValueResolver.Get<sbyte>(base.Property, base.Attribute.MinGetter);
			}
			if (base.Attribute.MaxGetter != null)
			{
				getterMaxValue = ValueResolver.Get<sbyte>(base.Property, base.Attribute.MaxGetter);
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			sbyte b = ((getterMinValue != null) ? getterMinValue.GetValue() : ((sbyte)base.Attribute.Min));
			sbyte b2 = ((getterMaxValue != null) ? getterMaxValue.GetValue() : ((sbyte)base.Attribute.Max));
			if (getterMinValue != null && getterMinValue.ErrorMessage != null)
			{
				SirenixEditorGUI.ErrorMessageBox(getterMinValue.ErrorMessage);
			}
			if (getterMaxValue != null && getterMaxValue.ErrorMessage != null)
			{
				SirenixEditorGUI.ErrorMessageBox(getterMaxValue.ErrorMessage);
			}
			EditorGUI.BeginChangeCheck();
			int num = SirenixEditorFields.RangeIntField(label, base.ValueEntry.SmartValue, Mathf.Min((int)b, (int)b2), Mathf.Max((int)b, (int)b2));
			if (EditorGUI.EndChangeCheck())
			{
				if (num < -128)
				{
					num = -128;
				}
				else if (num > 127)
				{
					num = 127;
				}
				base.ValueEntry.SmartValue = (sbyte)num;
			}
		}
	}
}
