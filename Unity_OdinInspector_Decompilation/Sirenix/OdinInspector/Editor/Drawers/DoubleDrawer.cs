using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Double property drawer.
	/// </summary>
	public sealed class DoubleDrawer : OdinValueDrawer<double>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<double> propertyValueEntry = base.ValueEntry;
			propertyValueEntry.SmartValue = SirenixEditorFields.DoubleField(label, propertyValueEntry.SmartValue);
		}
	}
}
