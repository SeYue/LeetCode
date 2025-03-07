using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws sbyte properties marked with <see cref="T:Sirenix.OdinInspector.DelayedPropertyAttribute" />.
	/// </summary>
	public sealed class DelayedPropertyAttributeSByteDrawer : OdinAttributeDrawer<DelayedPropertyAttribute, sbyte>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			int num = SirenixEditorFields.DelayedIntField(label, (int)base.ValueEntry.SmartValue, (GUILayoutOption[])GUILayoutOptions.MinWidth(0f));
			if (num < -128)
			{
				num = -128;
			}
			else if (num > 127)
			{
				num = 127;
			}
			base.ValueEntry.SmartValue = (sbyte)num;
		}
	}
}
