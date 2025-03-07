using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws all properties grouped together with the <see cref="T:Sirenix.OdinInspector.FoldoutGroupAttribute" />
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.FoldoutGroupAttribute" />
	public class FoldoutGroupAttributeDrawer : OdinGroupDrawer<FoldoutGroupAttribute>
	{
		private ValueResolver<string> titleGetter;

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		protected override void Initialize()
		{
			titleGetter = ValueResolver.GetForString(base.Property, base.Attribute.GroupName);
			if (base.Attribute.HasDefinedExpanded)
			{
				base.Property.State.Expanded = base.Attribute.Expanded;
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			InspectorProperty inspectorProperty = base.Property;
			FoldoutGroupAttribute foldoutGroupAttribute = base.Attribute;
			if (titleGetter.HasError)
			{
				SirenixEditorGUI.ErrorMessageBox(titleGetter.ErrorMessage);
			}
			SirenixEditorGUI.BeginBox();
			SirenixEditorGUI.BeginBoxHeader();
			GUIContent label2 = GUIHelper.TempContent(titleGetter.HasError ? inspectorProperty.Label.get_text() : titleGetter.GetValue());
			base.Property.State.Expanded = SirenixEditorGUI.Foldout(base.Property.State.Expanded, label2);
			SirenixEditorGUI.EndBoxHeader();
			if (SirenixEditorGUI.BeginFadeGroup(this, base.Property.State.Expanded))
			{
				for (int i = 0; i < inspectorProperty.Children.Count; i++)
				{
					InspectorProperty inspectorProperty2 = inspectorProperty.Children[i];
					inspectorProperty2.Draw(inspectorProperty2.Label);
				}
			}
			SirenixEditorGUI.EndFadeGroup();
			SirenixEditorGUI.EndBox();
		}
	}
}
