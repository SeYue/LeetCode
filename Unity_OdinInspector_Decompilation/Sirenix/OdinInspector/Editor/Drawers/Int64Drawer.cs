using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Long property drawer.
	/// </summary>
	public sealed class Int64Drawer : OdinValueDrawer<long>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<long> propertyValueEntry = base.ValueEntry;
			propertyValueEntry.SmartValue = SirenixEditorFields.LongField(label, propertyValueEntry.SmartValue);
		}
	}
}
