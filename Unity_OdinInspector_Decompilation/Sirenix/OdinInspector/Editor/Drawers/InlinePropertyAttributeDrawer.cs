using System;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Drawer for the <see cref="T:Sirenix.OdinInspector.InlinePropertyAttribute" /> attribute.
	/// </summary>
	[DrawerPriority(0.0, 0.0, 0.11)]
	public class InlinePropertyAttributeDrawer : OdinAttributeDrawer<InlinePropertyAttribute>
	{
		private PropertySearchFilter searchFilter;

		private string searchFieldControlName;

		protected override void Initialize()
		{
			SearchableAttribute searchableAttribute = base.Property.GetAttribute<SearchableAttribute>();
			if (searchableAttribute != null)
			{
				searchFilter = new PropertySearchFilter(base.Property, searchableAttribute);
				searchFieldControlName = "PropertyTreeSearchField_" + Guid.NewGuid().ToString();
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			bool flag = base.Attribute.LabelWidth > 0;
			if (label == null)
			{
				if (flag)
				{
					GUIHelper.PushLabelWidth(base.Attribute.LabelWidth);
				}
				CallNextDrawer(label);
				if (flag)
				{
					GUIHelper.PopLabelWidth();
				}
				return;
			}
			SirenixEditorGUI.BeginVerticalPropertyLayout(label);
			if (flag)
			{
				GUIHelper.PushLabelWidth(base.Attribute.LabelWidth);
			}
			if (searchFilter != null)
			{
				Rect controlRect = EditorGUILayout.GetControlRect((GUILayoutOption[])(object)new GUILayoutOption[0]);
				string text = SirenixEditorGUI.SearchField(controlRect, searchFilter.SearchTerm, forceFocus: false, searchFieldControlName);
				if (text != searchFilter.SearchTerm)
				{
					searchFilter.SearchTerm = text;
					base.Property.Tree.DelayActionUntilRepaint(delegate
					{
						searchFilter.UpdateSearch();
						GUIHelper.RequestRepaint();
					});
				}
			}
			if (searchFilter != null && searchFilter.HasSearchResults)
			{
				searchFilter.DrawSearchResults();
			}
			else
			{
				for (int i = 0; i < base.Property.Children.Count; i++)
				{
					InspectorProperty inspectorProperty = base.Property.Children[i];
					inspectorProperty.Draw(inspectorProperty.Label);
				}
			}
			if (flag)
			{
				GUIHelper.PopLabelWidth();
			}
			GUILayout.Space(2f);
			SirenixEditorGUI.EndVerticalPropertyLayout();
		}
	}
}
