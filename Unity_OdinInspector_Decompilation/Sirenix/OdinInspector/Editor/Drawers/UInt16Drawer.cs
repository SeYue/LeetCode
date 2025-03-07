using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Ushort property drawer.
	/// </summary>
	public sealed class UInt16Drawer : OdinValueDrawer<ushort>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<ushort> propertyValueEntry = base.ValueEntry;
			int num = SirenixEditorFields.IntField(label, propertyValueEntry.SmartValue);
			if (num < 0)
			{
				num = 0;
			}
			else if (num > 65535)
			{
				num = 65535;
			}
			propertyValueEntry.SmartValue = (ushort)num;
		}
	}
}
