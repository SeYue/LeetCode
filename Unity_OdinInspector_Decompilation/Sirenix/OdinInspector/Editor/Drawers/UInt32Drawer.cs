using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Uint property drawer.
	/// </summary>
	public sealed class UInt32Drawer : OdinValueDrawer<uint>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<uint> propertyValueEntry = base.ValueEntry;
			long num = SirenixEditorFields.LongField(label, propertyValueEntry.SmartValue);
			if (num > uint.MaxValue)
			{
				num = 4294967295L;
			}
			else if (num < 0)
			{
				num = 0L;
			}
			propertyValueEntry.SmartValue = (uint)num;
		}
	}
}
