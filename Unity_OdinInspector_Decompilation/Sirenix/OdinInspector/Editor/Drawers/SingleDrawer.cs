using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Float property drawer.
	/// </summary>
	public sealed class SingleDrawer : OdinValueDrawer<float>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<float> propertyValueEntry = base.ValueEntry;
			propertyValueEntry.SmartValue = SirenixEditorFields.FloatField(label, propertyValueEntry.SmartValue, (GUILayoutOption[])GUILayoutOptions.MinWidth(0f));
		}
	}
}
