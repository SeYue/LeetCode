using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Decimal property drawer.
	/// </summary>
	public sealed class DecimalDrawer : OdinValueDrawer<decimal>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<decimal> propertyValueEntry = base.ValueEntry;
			propertyValueEntry.SmartValue = SirenixEditorFields.DecimalField(label, propertyValueEntry.SmartValue);
		}
	}
}
