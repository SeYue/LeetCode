using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	internal class OdinMenuTreeDrawer : OdinValueDrawer<OdinMenuTree>
	{
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<OdinMenuTree> propertyValueEntry = base.ValueEntry;
			OdinMenuTree smartValue = propertyValueEntry.SmartValue;
			if (smartValue != null)
			{
				smartValue.DrawMenuTree();
				smartValue.HandleKeyboardMenuNavigation();
			}
		}
	}
}
