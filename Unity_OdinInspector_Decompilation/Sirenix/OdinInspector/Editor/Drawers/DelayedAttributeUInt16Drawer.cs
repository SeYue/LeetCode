using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws ushort properties marked with <see cref="T:UnityEngine.DelayedAttribute" />.
	/// </summary>
	public sealed class DelayedAttributeUInt16Drawer : OdinAttributeDrawer<DelayedAttribute, ushort>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			int num = SirenixEditorFields.DelayedIntField(label, (int)base.ValueEntry.SmartValue, (GUILayoutOption[])GUILayoutOptions.MinWidth(0f));
			if (num < 0)
			{
				num = 0;
			}
			else if (num > 65535)
			{
				num = 65535;
			}
			base.ValueEntry.SmartValue = (ushort)num;
		}
	}
}
