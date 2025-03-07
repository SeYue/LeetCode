using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// SByte property drawer.
	/// </summary>
	public sealed class SByteDrawer : OdinValueDrawer<sbyte>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<sbyte> propertyValueEntry = base.ValueEntry;
			int num = SirenixEditorFields.IntField(label, propertyValueEntry.SmartValue);
			if (num < -128)
			{
				num = -128;
			}
			else if (num > 127)
			{
				num = 127;
			}
			propertyValueEntry.SmartValue = (sbyte)num;
		}
	}
}
