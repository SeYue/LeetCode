using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public class TabSubGroupAttributeDrawer : OdinGroupDrawer<TabGroupAttribute.TabSubGroupAttribute>
	{
		private class Tab
		{
			public string TabName;

			public List<InspectorProperty> InspectorProperties = new List<InspectorProperty>();

			public ValueResolver<string> Title;
		}

		private GUITabGroup tabGroup;

		private List<Tab> tabs;

		private TabGroupAttribute tabGroupAttribute;

		private GUITabPage forcedTabGroup;

		protected override bool CanDrawGroup(InspectorProperty property)
		{
			if (property.Parent != null && property.Parent.Info.PropertyType == PropertyType.Group)
			{
				return property.Parent.GetAttribute<TabGroupAttribute>() != null;
			}
			return false;
		}

		protected override void Initialize()
		{
			tabGroupAttribute = base.Property.Parent.GetAttribute<TabGroupAttribute>();
			tabGroup = SirenixEditorGUI.CreateAnimatedTabGroup(base.Property);
			tabGroup.DrawNonSelectedTabsAsDisabled = true;
			tabs = new List<Tab>();
			List<Tab> list = new List<Tab>();
			for (int i = 0; i < base.Property.Parent.Children.Count; i++)
			{
				InspectorProperty inspectorProperty = base.Property.Parent.Children[i];
				bool flag = false;
				if (inspectorProperty.Info.PropertyType == PropertyType.Group)
				{
					Type type = inspectorProperty.GetAttribute<PropertyGroupAttribute>().GetType();
					if (type.IsNested && type.DeclaringType == typeof(TabGroupAttribute))
					{
						Tab tab = new Tab();
						tab.TabName = inspectorProperty.NiceName;
						tab.Title = ValueResolver.GetForString(base.Property, inspectorProperty.Name.TrimStart('#'));
						for (int j = 0; j < inspectorProperty.Children.Count; j++)
						{
							tab.InspectorProperties.Add(inspectorProperty.Children[j]);
						}
						tabs.Add(tab);
						flag = true;
					}
				}
				if (!flag)
				{
					Tab tab2 = new Tab();
					tab2.TabName = inspectorProperty.NiceName;
					tab2.Title = ValueResolver.GetForString(base.Property, inspectorProperty.Name.TrimStart('#'));
					tab2.InspectorProperties.Add(inspectorProperty);
					list.Add(tab2);
				}
			}
			foreach (Tab item in list)
			{
				tabs.Add(item);
			}
			for (int k = 0; k < tabs.Count; k++)
			{
				tabGroup.RegisterTab(tabs[k].TabName);
			}
			forcedTabGroup = tabGroup.RegisterTab(base.Property.Name.TrimStart('#'));
			tabGroup.SetCurrentPage(forcedTabGroup);
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Invalid comparison between Unknown and I4
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			InspectorProperty inspectorProperty = base.Property;
			TabGroupAttribute tabGroupAttribute = this.tabGroupAttribute;
			if (tabGroupAttribute.HideTabGroupIfTabGroupOnlyHasOneTab && tabs.Count <= 1)
			{
				for (int i = 0; i < tabs.Count; i++)
				{
					int count = tabs[i].InspectorProperties.Count;
					for (int j = 0; j < count; j++)
					{
						InspectorProperty inspectorProperty2 = tabs[i].InspectorProperties[j];
						inspectorProperty2.Update();
						inspectorProperty2.Draw(inspectorProperty2.Label);
					}
				}
				return;
			}
			tabGroup.AnimationSpeed = 1f / SirenixEditorGUI.TabPageSlideAnimationDuration;
			tabGroup.FixedHeight = tabGroupAttribute.UseFixedHeight;
			if ((int)Event.get_current().get_type() == 8)
			{
				tabGroup.SetCurrentPage(forcedTabGroup);
			}
			SirenixEditorGUI.BeginIndentedVertical(SirenixGUIStyles.PropertyPadding);
			tabGroup.BeginGroup(drawToolbar: true, tabGroupAttribute.Paddingless ? GUIStyle.get_none() : null);
			for (int k = 0; k < tabs.Count; k++)
			{
				GUITabPage gUITabPage = tabGroup.RegisterTab(tabs[k].TabName);
				gUITabPage.Title = tabs[k].Title.GetValue();
				if (gUITabPage.BeginPage())
				{
					int count2 = tabs[k].InspectorProperties.Count;
					for (int l = 0; l < count2; l++)
					{
						InspectorProperty inspectorProperty3 = tabs[k].InspectorProperties[l];
						inspectorProperty3.Update();
						inspectorProperty3.Draw(inspectorProperty3.Label);
					}
				}
				gUITabPage.EndPage();
			}
			tabGroup.EndGroup();
			SirenixEditorGUI.EndIndentedVertical();
		}
	}
}
