using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws all properties grouped together with the <see cref="T:Sirenix.OdinInspector.ButtonGroupAttribute" />
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.ButtonGroupAttribute" />
	public class ButtonGroupAttributeDrawer : OdinGroupDrawer<ButtonGroupAttribute>
	{
		private float buttonHeight;

		protected override void Initialize()
		{
			for (int i = 0; i < base.Property.Children.Count; i++)
			{
				ButtonAttribute buttonAttribute = base.Property.Children[i].GetAttribute<ButtonAttribute>();
				if (buttonAttribute != null && buttonAttribute.ButtonHeight > 0)
				{
					buttonHeight = buttonAttribute.ButtonHeight;
				}
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			InspectorProperty inspectorProperty = base.Property;
			SirenixEditorGUI.BeginIndentedHorizontal();
			for (int i = 0; i < inspectorProperty.Children.Count; i++)
			{
				GUIStyle val = null;
				if (inspectorProperty.Children.Count != 1)
				{
					val = ((i != 0) ? ((i != inspectorProperty.Children.Count - 1) ? SirenixGUIStyles.ButtonMid : SirenixGUIStyles.ButtonRight) : SirenixGUIStyles.ButtonLeft);
				}
				InspectorProperty inspectorProperty2 = inspectorProperty.Children[i];
				inspectorProperty2.Context.GetGlobal("ButtonHeight", buttonHeight).Value = buttonHeight;
				inspectorProperty2.Context.GetGlobal<GUIStyle>("ButtonStyle", val).Value = val;
				DefaultMethodDrawer.DontDrawMethodParameters = true;
				inspectorProperty2.Draw(inspectorProperty2.Label);
				DefaultMethodDrawer.DontDrawMethodParameters = false;
			}
			SirenixEditorGUI.EndIndentedHorizontal();
		}
	}
}
