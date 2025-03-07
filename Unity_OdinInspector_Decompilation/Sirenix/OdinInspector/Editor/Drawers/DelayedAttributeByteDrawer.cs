using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws byte properties marked with <see cref="T:UnityEngine.DelayedAttribute" />.
	/// </summary>
	public sealed class DelayedAttributeByteDrawer : OdinAttributeDrawer<DelayedAttribute, byte>
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
			else if (num > 255)
			{
				num = 255;
			}
			base.ValueEntry.SmartValue = (byte)num;
		}
	}
}
