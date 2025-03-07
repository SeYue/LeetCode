using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws all properties grouped together with the <see cref="T:Sirenix.OdinInspector.ToggleGroupAttribute" />
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.ToggleGroupAttribute" />
	public class ToggleGroupAttributeDrawer : OdinGroupDrawer<ToggleGroupAttribute>
	{
		private ValueResolver<string> titleHelper;

		private string errorMessage;

		private InspectorProperty toggleProperty;

		private PropertyContext<string> openToggleGlobalContext;

		protected override void Initialize()
		{
			toggleProperty = base.Property.Children.Get(base.Attribute.ToggleMemberName);
			if (toggleProperty == null)
			{
				errorMessage = "No property or field named " + base.Attribute.ToggleMemberName + " found. Make sure the property is part of the inspector and the group.";
			}
			else
			{
				titleHelper = ValueResolver.GetForString(base.Property, base.Attribute.ToggleGroupTitle ?? base.Attribute.GroupName);
				if (titleHelper.HasError)
				{
					errorMessage = titleHelper.ErrorMessage;
				}
			}
			if (base.Attribute.CollapseOthersOnExpand)
			{
				InspectorProperty inspectorProperty = base.Property.ParentValueProperty;
				while (inspectorProperty != null && !inspectorProperty.Info.HasBackingMembers)
				{
					inspectorProperty = inspectorProperty.ParentValueProperty;
				}
				if (inspectorProperty == null)
				{
					inspectorProperty = base.Property.Tree.RootProperty;
				}
				openToggleGlobalContext = inspectorProperty.Context.GetGlobal("OpenFoldoutToggleGroup", (string)null);
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (errorMessage != null)
			{
				SirenixEditorGUI.ErrorMessageBox(errorMessage);
				return;
			}
			if (base.Attribute.CollapseOthersOnExpand && openToggleGlobalContext != null && openToggleGlobalContext.Value != null && openToggleGlobalContext.Value != base.Property.Path)
			{
				base.Property.State.Expanded = false;
			}
			bool enabled = (bool)toggleProperty.ValueEntry.WeakSmartValue;
			string value = titleHelper.GetValue();
			bool expanded = base.Property.State.Expanded;
			bool visible = base.Property.State.Expanded;
			if (SirenixEditorGUI.BeginToggleGroup(UniqueDrawerKey.Create(base.Property, this), ref enabled, ref visible, value))
			{
				for (int i = 0; i < base.Property.Children.Count; i++)
				{
					InspectorProperty inspectorProperty = base.Property.Children[i];
					if (inspectorProperty != toggleProperty)
					{
						inspectorProperty.Draw(inspectorProperty.Label);
					}
				}
			}
			else
			{
				GUIHelper.BeginDrawToNothing();
				toggleProperty.Draw(toggleProperty.Label);
				GUIHelper.EndDrawToNothing();
			}
			SirenixEditorGUI.EndToggleGroup();
			base.Property.State.Expanded = visible;
			if (openToggleGlobalContext != null && expanded != base.Property.State.Expanded && base.Property.State.Expanded)
			{
				openToggleGlobalContext.Value = base.Property.Path;
			}
			toggleProperty.ValueEntry.WeakSmartValue = enabled;
		}
	}
}
