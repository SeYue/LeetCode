using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor.ActionResolvers;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Adds a generic menu option to properties marked with <see cref="T:Sirenix.OdinInspector.CustomContextMenuAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.CustomContextMenuAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.DisableContextMenuAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.OnInspectorGUIAttribute" />
	[DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
	public sealed class CustomContextMenuAttributeDrawer : OdinAttributeDrawer<CustomContextMenuAttribute>, IDefinesGenericMenuItems
	{
		private class ContextMenuInfo
		{
			public string Name;

			public ActionResolver Action;
		}

		private ContextMenuInfo info;

		private PropertyContext<Dictionary<CustomContextMenuAttribute, ContextMenuInfo>> contextMenuInfos;

		private PropertyContext<bool> populated;

		/// <summary>
		/// Populates the generic menu for the property.
		/// </summary>
		public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Expected O, but got Unknown
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Expected O, but got Unknown
			//IL_00ff: Expected O, but got Unknown
			if (populated.Value)
			{
				return;
			}
			populated.Value = true;
			if (contextMenuInfos.Value == null || contextMenuInfos.Value.Count <= 0)
			{
				return;
			}
			if (genericMenu.GetItemCount() > 0)
			{
				genericMenu.AddSeparator("");
			}
			foreach (KeyValuePair<CustomContextMenuAttribute, ContextMenuInfo> item in contextMenuInfos.Value.OrderBy((KeyValuePair<CustomContextMenuAttribute, ContextMenuInfo> n) => n.Key.MenuItem ?? ""))
			{
				ContextMenuInfo info = item.Value;
				if (info.Action == null)
				{
					genericMenu.AddDisabledItem(new GUIContent(item.Key.MenuItem + " (Invalid)"));
					continue;
				}
				genericMenu.AddItem(new GUIContent(info.Name), false, (MenuFunction)delegate
				{
					base.Property.RecordForUndo(info.Name);
					info.Action.DoActionForAllSelectionIndices();
				});
			}
		}

		protected override void Initialize()
		{
			InspectorProperty inspectorProperty = base.Property;
			CustomContextMenuAttribute customContextMenuAttribute = base.Attribute;
			contextMenuInfos = inspectorProperty.Context.GetGlobal("CustomContextMenu", (Dictionary<CustomContextMenuAttribute, ContextMenuInfo>)null);
			populated = inspectorProperty.Context.GetGlobal("CustomContextMenu_Populated", defaultValue: false);
			if (contextMenuInfos.Value == null)
			{
				contextMenuInfos.Value = new Dictionary<CustomContextMenuAttribute, ContextMenuInfo>();
			}
			if (!contextMenuInfos.Value.TryGetValue(customContextMenuAttribute, out info))
			{
				info = new ContextMenuInfo();
				info.Name = customContextMenuAttribute.MenuItem;
				info.Action = ActionResolver.Get(base.Property, customContextMenuAttribute.Action);
				contextMenuInfos.Value[customContextMenuAttribute] = info;
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			populated.Value = false;
			if (info.Action.HasError)
			{
				info.Action.DrawError();
			}
			CallNextDrawer(label);
		}
	}
}
