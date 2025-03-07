using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Drawer for the <see cref="T:Sirenix.OdinInspector.VerticalGroupAttribute" />
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.VerticalGroupAttribute" />
	public class VerticalGroupAttributeDrawer : OdinGroupDrawer<VerticalGroupAttribute>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			InspectorProperty inspectorProperty = base.Property;
			VerticalGroupAttribute verticalGroupAttribute = base.Attribute;
			GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[0]);
			if (verticalGroupAttribute.PaddingTop != 0f)
			{
				GUILayout.Space(verticalGroupAttribute.PaddingTop);
			}
			for (int i = 0; i < inspectorProperty.Children.Count; i++)
			{
				InspectorProperty inspectorProperty2 = inspectorProperty.Children[i];
				inspectorProperty2.Draw(inspectorProperty2.Label);
			}
			if (verticalGroupAttribute.PaddingBottom != 0f)
			{
				GUILayout.Space(verticalGroupAttribute.PaddingBottom);
			}
			GUILayout.EndVertical();
		}
	}
}
