using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Ulong property drawer.
	/// </summary>
	public sealed class UInt64Drawer : OdinValueDrawer<ulong>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<ulong> propertyValueEntry = base.ValueEntry;
			long num = SirenixEditorFields.LongField(label, (long)propertyValueEntry.SmartValue);
			if (num < 0)
			{
				num = 0L;
			}
			propertyValueEntry.SmartValue = (ulong)num;
		}
	}
}
