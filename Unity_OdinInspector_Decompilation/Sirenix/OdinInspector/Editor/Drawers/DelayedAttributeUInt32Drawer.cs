using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws uint properties marked with <see cref="T:UnityEngine.DelayedAttribute" />.
	/// </summary>
	public sealed class DelayedAttributeUInt32Drawer : OdinAttributeDrawer<DelayedAttribute, uint>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			long num = SirenixEditorFields.DelayedLongField(label, (long)base.ValueEntry.SmartValue, (GUILayoutOption[])GUILayoutOptions.MinWidth(0f));
			if (num < 0)
			{
				num = 0L;
			}
			else if (num > uint.MaxValue)
			{
				num = 4294967295L;
			}
			base.ValueEntry.SmartValue = (uint)num;
		}
	}
}
