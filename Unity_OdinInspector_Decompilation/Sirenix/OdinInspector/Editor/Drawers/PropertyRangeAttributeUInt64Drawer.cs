using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws ulong properties marked with <see cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	/// <seealso cref="T:UnityEngine.DelayedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	public sealed class PropertyRangeAttributeUInt64Drawer : OdinAttributeDrawer<PropertyRangeAttribute, ulong>
	{
		private ValueResolver<ulong> getterMinValue;

		private ValueResolver<ulong> getterMaxValue;

		/// <summary>
		/// Initialized the drawer.
		/// </summary>
		protected override void Initialize()
		{
			if (base.Attribute.MinGetter != null)
			{
				getterMinValue = ValueResolver.Get<ulong>(base.Property, base.Attribute.MinGetter);
			}
			if (base.Attribute.MaxGetter != null)
			{
				getterMaxValue = ValueResolver.Get<ulong>(base.Property, base.Attribute.MaxGetter);
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			ulong num = ((getterMinValue != null) ? getterMinValue.GetValue() : ((ulong)base.Attribute.Min));
			ulong num2 = ((getterMaxValue != null) ? getterMaxValue.GetValue() : ((ulong)base.Attribute.Max));
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
				if (num3 < 0)
				{
					num3 = 0;
				}
				else
				{
					base.ValueEntry.SmartValue = (ulong)num3;
				}
				base.ValueEntry.SmartValue = (ulong)num3;
			}
		}
	}
}
