using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// LayerMask property drawer.
	/// </summary>
	public class LayerMaskDrawer : OdinValueDrawer<LayerMask>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			IPropertyValueEntry<LayerMask> propertyValueEntry = base.ValueEntry;
			propertyValueEntry.SmartValue = SirenixEditorFields.LayerMaskField(label, propertyValueEntry.SmartValue);
		}
	}
}
