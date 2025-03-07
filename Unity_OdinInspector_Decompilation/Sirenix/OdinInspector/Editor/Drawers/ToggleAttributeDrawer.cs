using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.ToggleAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.ToggleAttribute" />
	public class ToggleAttributeDrawer : OdinAttributeDrawer<ToggleAttribute>
	{
		private InspectorProperty toggleProperty;

		private PropertyContext<string> openToggleGlobalContext;

		protected override void Initialize()
		{
			toggleProperty = base.Property.Children.Get(base.Attribute.ToggleMemberName);
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
			if (toggleProperty == null)
			{
				SirenixEditorGUI.ErrorMessageBox(base.Attribute.ToggleMemberName + " is not a member of " + base.Property.NiceName + ".");
				return;
			}
			if (toggleProperty.ValueEntry.TypeOfValue != typeof(bool))
			{
				SirenixEditorGUI.ErrorMessageBox(base.Attribute.ToggleMemberName + " on " + base.Property.NiceName + "  must be a boolean.");
				return;
			}
			bool enabled = (bool)toggleProperty.ValueEntry.WeakSmartValue;
			if (base.Attribute.CollapseOthersOnExpand && openToggleGlobalContext != null && openToggleGlobalContext.Value != null && openToggleGlobalContext.Value != base.Property.Path)
			{
				base.Property.State.Expanded = false;
			}
			bool expanded = base.Property.State.Expanded;
			bool visible = base.Property.State.Expanded;
			if (SirenixEditorGUI.BeginToggleGroup(UniqueDrawerKey.Create(base.Property, this), ref enabled, ref visible, (label != null) ? label.get_text() : base.Property.NiceName))
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
