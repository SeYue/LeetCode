using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Short property drawer.
	/// </summary>
	public sealed class Int16Drawer : OdinValueDrawer<short>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<short> propertyValueEntry = base.ValueEntry;
			int num = SirenixEditorFields.IntField(label, propertyValueEntry.SmartValue);
			if (num < -32768)
			{
				num = -32768;
			}
			else if (num > 32767)
			{
				num = 32767;
			}
			propertyValueEntry.SmartValue = (short)num;
		}
	}
}
