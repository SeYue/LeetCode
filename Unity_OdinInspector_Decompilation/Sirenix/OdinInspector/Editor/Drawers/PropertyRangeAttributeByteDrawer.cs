using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws byte properties marked with <see cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	/// <seealso cref="T:UnityEngine.DelayedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	public sealed class PropertyRangeAttributeByteDrawer : OdinAttributeDrawer<PropertyRangeAttribute, byte>
	{
		private ValueResolver<byte> getterMinValue;

		private ValueResolver<byte> getterMaxValue;

		/// <summary>
		/// Initialized the drawer.
		/// </summary>
		protected override void Initialize()
		{
			if (base.Attribute.MinGetter != null)
			{
				getterMinValue = ValueResolver.Get<byte>(base.Property, base.Attribute.MinGetter);
			}
			if (base.Attribute.MaxGetter != null)
			{
				getterMaxValue = ValueResolver.Get<byte>(base.Property, base.Attribute.MaxGetter);
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			byte b = ((getterMinValue != null) ? getterMinValue.GetValue() : ((byte)base.Attribute.Min));
			byte b2 = ((getterMaxValue != null) ? getterMaxValue.GetValue() : ((byte)base.Attribute.Max));
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
				if (num < 0)
				{
					num = 0;
				}
				else if (num > 255)
				{
					num = 255;
				}
				base.ValueEntry.SmartValue = (byte)num;
			}
		}
	}
}
