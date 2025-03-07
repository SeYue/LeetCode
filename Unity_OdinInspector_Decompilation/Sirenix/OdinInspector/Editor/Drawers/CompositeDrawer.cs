using System;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Drawer for composite properties.
	/// </summary>
	[DrawerPriority(0.0, 0.0, 0.1)]
	public class CompositeDrawer : OdinDrawer
	{
		private PropertySearchFilter searchFilter;

		private string searchFieldControlName;

		protected override void Initialize()
		{
			SearchableAttribute attribute = base.Property.GetAttribute<SearchableAttribute>();
			if (attribute != null)
			{
				searchFilter = new PropertySearchFilter(base.Property, attribute);
				searchFieldControlName = "PropertyTreeSearchField_" + Guid.NewGuid().ToString();
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0145: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
			InspectorProperty inspectorProperty = base.Property;
			if (inspectorProperty.IsTreeRoot)
			{
				label = null;
			}
			if (inspectorProperty.Children.Count == 0)
			{
				if (inspectorProperty.ValueEntry != null)
				{
					if (label != null)
					{
						Rect controlRect = EditorGUILayout.GetControlRect((GUILayoutOption[])(object)new GUILayoutOption[0]);
						GUI.Label(controlRect, label);
					}
					return;
				}
				GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
				if (label != null)
				{
					EditorGUILayout.PrefixLabel(label);
				}
				SirenixEditorGUI.WarningMessageBox(string.Concat("There is no drawer defined for property ", inspectorProperty.NiceName, " of type ", inspectorProperty.Info.PropertyType, "."));
				GUILayout.EndHorizontal();
				return;
			}
			if (label == null)
			{
				if (searchFilter != null)
				{
					searchFilter.DrawDefaultSearchFieldLayout(null);
				}
				if (searchFilter != null && searchFilter.HasSearchResults)
				{
					searchFilter.DrawSearchResults();
					return;
				}
				for (int i = 0; i < inspectorProperty.Children.Count; i++)
				{
					InspectorProperty inspectorProperty2 = inspectorProperty.Children[i];
					inspectorProperty2.Draw(inspectorProperty2.Label);
				}
				return;
			}
			float fieldWidth = EditorGUIUtility.get_fieldWidth();
			EditorGUIUtility.set_fieldWidth(10f);
			Rect controlRect2 = EditorGUILayout.GetControlRect(false, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			EditorGUIUtility.set_fieldWidth(fieldWidth);
			if (searchFilter != null)
			{
				Rect rect = GUILayoutUtility.GetLastRect().AddXMin(GUIHelper.BetterLabelWidth).AddY(1f);
				string newTerm = SirenixEditorGUI.SearchField(rect, searchFilter.SearchTerm, forceFocus: false, searchFieldControlName);
				if (newTerm != searchFilter.SearchTerm)
				{
					searchFilter.SearchTerm = newTerm;
					base.Property.Tree.DelayActionUntilRepaint(delegate
					{
						if (!string.IsNullOrEmpty(newTerm))
						{
							base.Property.State.Expanded = true;
						}
						searchFilter.UpdateSearch();
						GUIHelper.RequestRepaint();
					});
				}
			}
			base.Property.State.Expanded = SirenixEditorGUI.Foldout(controlRect2, base.Property.State.Expanded, label);
			if (SirenixEditorGUI.BeginFadeGroup(this, base.Property.State.Expanded))
			{
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
				if (searchFilter != null && searchFilter.HasSearchResults)
				{
					searchFilter.DrawSearchResults();
				}
				else
				{
					for (int j = 0; j < inspectorProperty.Children.Count; j++)
					{
						InspectorProperty inspectorProperty3 = inspectorProperty.Children[j];
						inspectorProperty3.Draw(inspectorProperty3.Label);
					}
				}
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
			}
			SirenixEditorGUI.EndFadeGroup();
		}
	}
}
