using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Int property drawer.
	/// </summary>
	public sealed class Int32Drawer : OdinValueDrawer<int>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			base.ValueEntry.SmartValue = SirenixEditorFields.IntField(label, base.ValueEntry.SmartValue);
		}
	}
}
