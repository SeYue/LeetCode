using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Byte property drawer.
	/// </summary>
	public sealed class ByteDrawer : OdinValueDrawer<byte>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<byte> propertyValueEntry = base.ValueEntry;
			int num = SirenixEditorFields.IntField(label, propertyValueEntry.SmartValue);
			if (num < 0)
			{
				num = 0;
			}
			else if (num > 255)
			{
				num = 255;
			}
			propertyValueEntry.SmartValue = (byte)num;
		}
	}
}
