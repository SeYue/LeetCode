using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws all properties grouped together with the <see cref="T:Sirenix.OdinInspector.BoxGroupAttribute" />
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.BoxGroupAttribute" />
	public class BoxGroupAttributeDrawer : OdinGroupDrawer<BoxGroupAttribute>
	{
		private ValueResolver<string> labelGetter;

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		protected override void Initialize()
		{
			labelGetter = ValueResolver.GetForString(base.Property, base.Attribute.LabelText ?? base.Attribute.GroupName);
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			labelGetter.DrawError();
			string text = null;
			if (base.Attribute.ShowLabel)
			{
				text = labelGetter.GetValue();
				if (string.IsNullOrEmpty(text))
				{
					text = "Null";
				}
			}
			SirenixEditorGUI.BeginBox(text, base.Attribute.CenterLabel);
			for (int i = 0; i < base.Property.Children.Count; i++)
			{
				InspectorProperty inspectorProperty = base.Property.Children[i];
				inspectorProperty.Draw(inspectorProperty.Label);
			}
			SirenixEditorGUI.EndBox();
		}
	}
}
