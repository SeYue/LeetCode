using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws short properties marked with <see cref="T:UnityEngine.DelayedAttribute" />.
	/// </summary>
	public sealed class DelayedAttributeInt16Drawer : OdinAttributeDrawer<DelayedAttribute, short>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			int num = SirenixEditorFields.DelayedIntField(label, (int)base.ValueEntry.SmartValue, (GUILayoutOption[])GUILayoutOptions.MinWidth(0f));
			if (num < -32768)
			{
				num = -32768;
			}
			else if (num > 32767)
			{
				num = 32767;
			}
			base.ValueEntry.SmartValue = (short)num;
		}
	}
}
