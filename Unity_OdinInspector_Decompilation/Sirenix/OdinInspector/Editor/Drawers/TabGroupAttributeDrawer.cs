using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws all properties grouped together with the <see cref="T:Sirenix.OdinInspector.TabGroupAttribute" />
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.TabGroupAttribute" />
	public class TabGroupAttributeDrawer : OdinGroupDrawer<TabGroupAttribute>, IOnSelfStateChangedNotification
	{
		private class Tab
		{
			public string TabName;

			public List<InspectorProperty> InspectorProperties = new List<InspectorProperty>();

			public ValueResolver<string> Title;
		}

		public const string CurrentTabIndexKey = "CurrentTabIndex";

		public const string CurrentTabNameKey = "CurrentTabName";

		public const string TabCountKey = "TabCount";

		private bool isChangingTabName;

		private GUITabGroup tabGroup;

		private List<Tab> tabs;

		private bool initialized;

		protected override void Initialize()
		{
			tabGroup = SirenixEditorGUI.CreateAnimatedTabGroup(base.Property);
			tabs = new List<Tab>();
			List<Tab> list = new List<Tab>();
			for (int i = 0; i < base.Property.Children.Count; i++)
			{
				InspectorProperty inspectorProperty = base.Property.Children[i];
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
			base.Property.State.Create("CurrentTabIndex", persistent: true, 0);
			base.Property.State.Create("TabCount", persistent: false, tabs.Count);
			int clampedCurrentIndex = GetClampedCurrentIndex();
			Tab tab3 = tabs[clampedCurrentIndex];
			GUITabPage currentPage = tabGroup.RegisterTab(tab3.TabName);
			tabGroup.SetCurrentPage(currentPage);
			isChangingTabName = true;
			base.Property.State.Create("CurrentTabName", persistent: false, tab3.TabName);
			isChangingTabName = false;
			initialized = true;
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			InspectorProperty inspectorProperty = base.Property;
			TabGroupAttribute tabGroupAttribute = base.Attribute;
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
			GetClampedCurrentIndex();
			SirenixEditorGUI.BeginIndentedVertical(SirenixGUIStyles.PropertyPadding);
			tabGroup.BeginGroup(drawToolbar: true, tabGroupAttribute.Paddingless ? GUIStyle.get_none() : null);
			base.Property.State.Set("TabCount", tabs.Count);
			for (int k = 0; k < tabs.Count; k++)
			{
				GUITabPage gUITabPage = tabGroup.RegisterTab(tabs[k].TabName);
				gUITabPage.Title = tabs[k].Title.GetValue();
				if (tabGroup.NextPage == null && tabGroup.CurrentPage == gUITabPage)
				{
					base.Property.State.Set("CurrentTabIndex", k);
				}
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

		private int GetClampedCurrentIndex()
		{
			int num = base.Property.State.Get<int>("CurrentTabIndex");
			if (num < 0)
			{
				num = 0;
				base.Property.State.Set("CurrentTabIndex", num);
			}
			else if (num >= tabs.Count)
			{
				num = tabs.Count - 1;
				base.Property.State.Set("CurrentTabIndex", num);
			}
			return num;
		}

		public void OnSelfStateChanged(string state)
		{
			if (!initialized)
			{
				return;
			}
			if (state == "CurrentTabIndex")
			{
				int clampedCurrentIndex = GetClampedCurrentIndex();
				Tab tab = tabs[clampedCurrentIndex];
				isChangingTabName = true;
				base.Property.State.Set("CurrentTabName", tab.TabName);
				isChangingTabName = false;
				tabGroup.GoToPage(tabs[clampedCurrentIndex].TabName);
			}
			else
			{
				if (!(state == "CurrentTabName") || isChangingTabName)
				{
					return;
				}
				string text = base.Property.State.Get<string>("CurrentTabName");
				int num = -1;
				for (int i = 0; i < tabs.Count; i++)
				{
					if (tabs[i].TabName == text)
					{
						num = i;
						break;
					}
				}
				if (num == -1)
				{
					Debug.LogError((object)("There is no tab named '" + text + "' in the tab group '" + base.Property.NiceName + "'!"));
					num = base.Property.State.Get<int>("CurrentTabIndex");
					isChangingTabName = true;
					base.Property.State.Set("CurrentTabName", tabs[num].TabName);
					isChangingTabName = false;
				}
				else
				{
					base.Property.State.Set("CurrentTabIndex", num);
				}
			}
		}
	}
}
