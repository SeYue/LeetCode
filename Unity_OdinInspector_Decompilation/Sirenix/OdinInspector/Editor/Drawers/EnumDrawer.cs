using System;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Enum property drawer.
	/// </summary>
	public sealed class EnumDrawer<T> : OdinValueDrawer<T>
	{
		/// <summary>
		/// Returns <c>true</c> if the drawer can draw the type.
		/// </summary>
		public override bool CanDrawTypeFilter(Type type)
		{
			return type.IsEnum;
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<T> propertyValueEntry = base.ValueEntry;
			if (GlobalConfig<GeneralDrawerConfig>.Instance.UseImprovedEnumDropDown)
			{
				propertyValueEntry.SmartValue = EnumSelector<T>.DrawEnumField(label, propertyValueEntry.SmartValue);
			}
			else
			{
				propertyValueEntry.WeakSmartValue = SirenixEditorFields.EnumDropdown(label, (Enum)propertyValueEntry.WeakSmartValue);
			}
		}
	}
}
