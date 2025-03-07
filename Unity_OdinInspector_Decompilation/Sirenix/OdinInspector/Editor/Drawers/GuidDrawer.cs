using System;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Int property drawer.
	/// </summary>
	public sealed class GuidDrawer : OdinValueDrawer<Guid>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<Guid> propertyValueEntry = base.ValueEntry;
			propertyValueEntry.SmartValue = SirenixEditorFields.GuidField(label, propertyValueEntry.SmartValue);
		}
	}
}
