using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// A feature-rich type selector.
	/// </summary>
	/// <example>
	/// <code>
	/// Type[] selectedTypes;
	///
	/// void OnGUI()
	/// {
	///     // Use the selector manually. See the documentation for OdinSelector for more information.
	///     if (GUILayout.Button("Open My Selector"))
	///     {
	///         TypeSelector selector = new TypeSelector(customListOfTypes);
	///         TypeSelector selector = new TypeSelector(AssemblyTypeFlags.CustomTypes, supportsMultiSelect: true);
	///         selector.SetSelection(this.selectedTypes);
	///         selector.SelectionConfirmed += selection =&gt; this.selectedTypes = selection.ToArray();
	///         selector.ShowInPopup(); // Returns the Odin Editor Window instance, in case you want to mess around with that as well.
	///     }
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// <code>
	/// private static Type currentSelectedType;
	/// private static IEnumerable&lt;Type&gt; currentSource;
	/// private static Func&lt;Rect, OdinSelector&lt;Type&gt;&gt; createTypeSelector = (rect) =&gt;
	/// {
	///     TypeSelector selector = new TypeSelector(currentSource, false);
	///     selector.SetSelection(currentSelectedType);
	///     selector.ShowInPopup(rect);
	///     return selector;
	/// };
	///
	/// public static Type DrawTypeSelectorDropdown(GUIContent label, Type selectedType, IEnumerable&lt;Type&gt; source)
	/// {
	///     currentSource = source;
	///     currentSelectedType = selectedType;
	///
	///     var dropdownText = selectedType == null ? "None" : selectedType.GetNiceName();
	///     var selected = TypeSelector.DrawSelectorDropdown(label, dropdownText, createTypeSelector);
	///     if (selected != null &amp;&amp; selected.Any())
	///     {
	///         selectedType = selected.FirstOrDefault();
	///     }
	///     return selectedType;
	/// }
	/// </code>
	/// </example>
	public class TypeSelector : OdinSelector<Type>
	{
		private static Dictionary<AssemblyTypeFlags, List<OdinMenuItem>> cachedAllTypesMenuItems = new Dictionary<AssemblyTypeFlags, List<OdinMenuItem>>();

		private IEnumerable<Type> types;

		private AssemblyTypeFlags assemblyTypeFlags;

		private bool supportsMultiSelect;

		[HideInInspector]
		public bool HideNamespaces;

		[HideInInspector]
		public bool FlattenTree;

		private Type lastType;

		public override string Title => null;

		public TypeSelector(AssemblyTypeFlags assemblyFlags, bool supportsMultiSelect)
		{
			types = null;
			this.supportsMultiSelect = supportsMultiSelect;
			assemblyTypeFlags = assemblyFlags;
		}

		public TypeSelector(IEnumerable<Type> types, bool supportsMultiSelect)
		{
			this.types = ((types != null) ? OrderTypes(types) : types);
			this.supportsMultiSelect = supportsMultiSelect;
		}

		private static IEnumerable<Type> OrderTypes(IEnumerable<Type> types)
		{
			return from x in types
				orderby x.Namespace.IsNullOrWhitespace() descending, x.Namespace, x.Name
				select x;
		}

		public override bool IsValidSelection(IEnumerable<Type> collection)
		{
			return collection.Any();
		}

		/// <summary>
		/// Builds the selection tree.
		/// </summary>
		protected override void BuildSelectionTree(OdinMenuTree tree)
		{
			tree.Config.UseCachedExpandedStates = false;
			tree.DefaultMenuStyle.NotSelectedIconAlpha = 1f;
			if (types == null)
			{
				if (cachedAllTypesMenuItems.TryGetValue(assemblyTypeFlags, out var value))
				{
					AddRecursive(tree, value, tree.MenuItems);
				}
				else
				{
					IEnumerable<Type> enumerable = OrderTypes(AssemblyUtilities.GetTypes(assemblyTypeFlags).Where(delegate(Type x)
					{
						if (x.Name == null)
						{
							return false;
						}
						string text = x.Name.TrimStart();
						return text.Length != 0 && char.IsLetter(text[0]);
					}));
					foreach (Type item in enumerable)
					{
						string niceName = item.GetNiceName();
						string typeNamePath = GetTypeNamePath(item, niceName);
						OdinMenuItem odinMenuItem = tree.AddObjectAtPath(typeNamePath, item).AddThumbnailIcons().Last();
						odinMenuItem.SearchString = ((niceName == typeNamePath) ? typeNamePath : (niceName + "|" + typeNamePath));
					}
					cachedAllTypesMenuItems[assemblyTypeFlags] = tree.MenuItems;
				}
			}
			else
			{
				foreach (Type t2 in types)
				{
					string niceName2 = t2.GetNiceName();
					string typeNamePath2 = GetTypeNamePath(t2, niceName2);
					OdinMenuItem odinMenuItem2 = tree.AddObjectAtPath(typeNamePath2, t2).Last();
					odinMenuItem2.SearchString = ((niceName2 == typeNamePath2) ? typeNamePath2 : (niceName2 + "|" + typeNamePath2));
					if (FlattenTree && t2.Namespace != null)
					{
						odinMenuItem2.OnDrawItem = (Action<OdinMenuItem>)Delegate.Combine(odinMenuItem2.OnDrawItem, (Action<OdinMenuItem>)delegate(OdinMenuItem x)
						{
							//IL_0001: Unknown result type (might be due to invalid IL or missing references)
							//IL_0010: Unknown result type (might be due to invalid IL or missing references)
							//IL_001a: Unknown result type (might be due to invalid IL or missing references)
							GUI.Label(x.Rect.Padding(10f, 0f).AlignCenterY(16f), t2.Namespace, SirenixGUIStyles.RightAlignedGreyMiniLabel);
						});
					}
				}
				tree.EnumerateTree((OdinMenuItem x) => x.Value != null, includeRootNode: false).AddThumbnailIcons();
			}
			tree.Selection.SupportsMultiSelect = supportsMultiSelect;
			tree.Selection.SelectionChanged += delegate
			{
				lastType = base.SelectionTree.Selection.Select((OdinMenuItem x) => x.Value).OfType<Type>().LastOrDefault() ?? lastType;
			};
		}

		private string GetTypeNamePath(Type t, string niceName)
		{
			string text = niceName;
			if (!FlattenTree && !string.IsNullOrEmpty(t.Namespace) && !HideNamespaces)
			{
				text = string.Concat(str1: (FlattenTree ? '.' : '/').ToString(), str0: t.Namespace, str2: text);
			}
			return text;
		}

		private static void AddRecursive(OdinMenuTree tree, List<OdinMenuItem> source, List<OdinMenuItem> destination)
		{
			destination.Capacity = source.Count;
			for (int i = 0; i < source.Count; i++)
			{
				OdinMenuItem odinMenuItem = source[i];
				OdinMenuItem odinMenuItem2 = new OdinMenuItem(tree, odinMenuItem.Name, odinMenuItem.Value).AddThumbnailIcon(preferAssetPreviewAsIcon: false);
				odinMenuItem2.SearchString = odinMenuItem.SearchString;
				destination.Add(odinMenuItem2);
				if (odinMenuItem.ChildMenuItems.Count > 0)
				{
					AddRecursive(tree, odinMenuItem.ChildMenuItems, odinMenuItem2.ChildMenuItems);
				}
			}
		}

		/// <summary>
		/// 450
		/// </summary>
		protected override float DefaultWindowWidth()
		{
			return 450f;
		}

		[OnInspectorGUI]
		[PropertyOrder(10f)]
		private void ShowTypeInfo()
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			string text = "";
			string text2 = "";
			string text3 = "";
			int num = 16;
			Rect rect = GUILayoutUtility.GetRect(0f, (float)(num * 3 + 8)).Padding(10f, 4f).AlignTop(num);
			int num2 = 75;
			if (lastType != null)
			{
				text = lastType.GetNiceFullName();
				text2 = lastType.Assembly.GetName().Name;
				text3 = ((lastType.BaseType == null) ? "" : lastType.BaseType.GetNiceFullName());
			}
			GUIStyle leftAlignedGreyMiniLabel = SirenixGUIStyles.LeftAlignedGreyMiniLabel;
			GUI.Label(rect.AlignLeft(num2), "Type Name", leftAlignedGreyMiniLabel);
			GUI.Label(rect.AlignRight(((Rect)(ref rect)).get_width() - (float)num2), text, leftAlignedGreyMiniLabel);
			((Rect)(ref rect)).set_y(((Rect)(ref rect)).get_y() + (float)num);
			GUI.Label(rect.AlignLeft(num2), "Base Type", leftAlignedGreyMiniLabel);
			GUI.Label(rect.AlignRight(((Rect)(ref rect)).get_width() - (float)num2), text3, leftAlignedGreyMiniLabel);
			((Rect)(ref rect)).set_y(((Rect)(ref rect)).get_y() + (float)num);
			GUI.Label(rect.AlignLeft(num2), "Assembly", leftAlignedGreyMiniLabel);
			GUI.Label(rect.AlignRight(((Rect)(ref rect)).get_width() - (float)num2), text2, leftAlignedGreyMiniLabel);
		}

		/// <summary>
		/// Sets the selected types.
		/// </summary>
		public override void SetSelection(Type selected)
		{
			base.SetSelection(selected);
			base.SelectionTree.Selection.SelectMany((OdinMenuItem x) => x.GetParentMenuItemsRecursive(includeSelf: false)).ForEach(delegate(OdinMenuItem x)
			{
				x.Toggled = true;
			});
		}
	}
}
