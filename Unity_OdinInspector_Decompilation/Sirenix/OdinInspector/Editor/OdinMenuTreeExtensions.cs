using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Class with utility methods for <see cref="T:Sirenix.OdinInspector.Editor.OdinMenuTree" />s and <see cref="T:Sirenix.OdinInspector.Editor.OdinMenuItem" />s.
	/// </summary>
	/// <example>
	/// <code>
	/// OdinMenuTree tree = new OdinMenuTree();
	/// tree.AddAllAssetsAtPath("Some Menu Item", "Some Asset Path", typeof(ScriptableObject), true)
	///     .AddThumbnailIcons();
	/// tree.AddAssetAtPath("Some Second Menu Item", "SomeAssetPath/SomeAssetFile.asset");
	/// // etc...
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTree" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuItem" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuStyle" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTreeSelection" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuEditorWindow" />
	public static class OdinMenuTreeExtensions
	{
		private class OdinMenuItemComparer : IComparer<OdinMenuItem>
		{
			public Comparison<OdinMenuItem> CustomComparison;

			public bool PlaceFoldersFirst;

			public bool IgnoreLeadingZeroes;

			public bool IgnoreWhiteSpace;

			public bool IgnoreCase;

			public OdinMenuItemComparer(Comparison<OdinMenuItem> customComparison = null)
			{
				CustomComparison = customComparison;
			}

			public int Compare(OdinMenuItem a, OdinMenuItem b)
			{
				if (CustomComparison != null)
				{
					return CustomComparison(a, b);
				}
				if (PlaceFoldersFirst)
				{
					if (a.ChildMenuItems.Count > 0 && b.ChildMenuItems.Count == 0)
					{
						return -1;
					}
					if (b.ChildMenuItems.Count > 0 && a.ChildMenuItems.Count == 0)
					{
						return 1;
					}
				}
				return StringUtilities.NumberAwareStringCompare(a.SmartName, b.SmartName, IgnoreLeadingZeroes, IgnoreWhiteSpace, IgnoreCase);
			}
		}

		[ShowOdinSerializedPropertiesInInspector]
		private class SerializedValueWrapper
		{
			private object instance;

			[HideLabel]
			[ShowInInspector]
			[HideReferenceObjectPicker]
			public object Instance
			{
				get
				{
					return instance;
				}
				set
				{
				}
			}

			public SerializedValueWrapper(object obj)
			{
				instance = obj;
			}
		}

		private static List<OdinMenuItem> cache = new List<OdinMenuItem>(5);

		/// <summary>
		/// Adds the menu item at the specified menu item path and populates the result list with all menu items created in order to add the menuItem at the specified path.
		/// </summary>
		/// <param name="tree">The tree instance.</param>
		/// <param name="result">The result list.</param>
		/// <param name="path">The menu item path.</param>
		/// <param name="menuItem">The menu item.</param>
		public static void AddMenuItemAtPath(this OdinMenuTree tree, ICollection<OdinMenuItem> result, string path, OdinMenuItem menuItem)
		{
			OdinMenuItem odinMenuItem = tree.Root;
			if (!string.IsNullOrEmpty(path))
			{
				if (path[0] == '/' || path[path.Length - 1] == '/')
				{
					path = path.Trim();
				}
				int num = 0;
				int num2 = 0;
				do
				{
					num2 = path.IndexOf('/', num);
					string text;
					if (num2 < 0)
					{
						num2 = path.Length - 1;
						text = path.Substring(num, num2 - num + 1);
					}
					else
					{
						text = path.Substring(num, num2 - num);
					}
					List<OdinMenuItem> childMenuItems = odinMenuItem.ChildMenuItems;
					OdinMenuItem odinMenuItem2 = null;
					for (int num3 = childMenuItems.Count - 1; num3 >= 0; num3--)
					{
						if (childMenuItems[num3].Name == text)
						{
							odinMenuItem2 = childMenuItems[num3];
							break;
						}
					}
					if (odinMenuItem2 == null)
					{
						odinMenuItem2 = new OdinMenuItem(tree, text, null);
						odinMenuItem.ChildMenuItems.Add(odinMenuItem2);
					}
					result.Add(odinMenuItem2);
					odinMenuItem = odinMenuItem2;
					num = num2 + 1;
				}
				while (num2 != path.Length - 1);
			}
			List<OdinMenuItem> childMenuItems2 = odinMenuItem.ChildMenuItems;
			OdinMenuItem odinMenuItem3 = null;
			for (int num4 = childMenuItems2.Count - 1; num4 >= 0; num4--)
			{
				if (childMenuItems2[num4].Name == menuItem.Name)
				{
					odinMenuItem3 = childMenuItems2[num4];
					break;
				}
			}
			if (odinMenuItem3 != null)
			{
				odinMenuItem.ChildMenuItems.Remove(odinMenuItem3);
				menuItem.ChildMenuItems.AddRange(odinMenuItem3.ChildMenuItems);
			}
			odinMenuItem.ChildMenuItems.Add(menuItem);
			result.Add(menuItem);
		}

		/// <summary>
		/// Adds the menu item at specified menu item path, and returns all menu items created in order to add the menuItem at the specified path.
		/// </summary>
		/// <param name="tree">The tree.</param>
		/// <param name="path">The menu item path.</param>
		/// <param name="menuItem">The menu item.</param>
		/// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
		public static IEnumerable<OdinMenuItem> AddMenuItemAtPath(this OdinMenuTree tree, string path, OdinMenuItem menuItem)
		{
			cache.Clear();
			tree.AddMenuItemAtPath(cache, path, menuItem);
			return cache;
		}

		/// <summary>
		/// Gets the menu item at the specified path, returns null non was found.
		/// </summary>
		public static OdinMenuItem GetMenuItem(this OdinMenuTree tree, string menuPath)
		{
			OdinMenuItem odinMenuItem = tree.Root;
			if (!string.IsNullOrEmpty(menuPath))
			{
				menuPath = menuPath.Trim('/') + "/";
				int num = 0;
				int num2 = 0;
				do
				{
					num2 = menuPath.IndexOf('/', num);
					string name = menuPath.Substring(num, num2 - num);
					OdinMenuItem odinMenuItem2 = odinMenuItem.ChildMenuItems.FirstOrDefault((OdinMenuItem x) => x.Name == name) ?? odinMenuItem.ChildMenuItems.FirstOrDefault((OdinMenuItem x) => x.SmartName == name);
					if (odinMenuItem2 == null)
					{
						return null;
					}
					odinMenuItem = odinMenuItem2;
					num = num2 + 1;
				}
				while (num2 != menuPath.Length - 1);
			}
			return odinMenuItem;
		}

		/// <summary>
		/// Adds all asset instances from the specified path and type into a single <see cref="T:Sirenix.OdinInspector.Editor.OdinMenuItem" /> at the specified menu item path, and returns all menu items created in order to add the menuItem at the specified path.. 
		/// </summary>
		/// <param name="tree">The tree.</param>
		/// <param name="menuPath">The menu item path.</param>
		/// <param name="assetFolderPath">The asset folder path.</param>
		/// <param name="type">The type of objects.</param>
		/// <param name="includeSubDirectories">Whether to search for assets in subdirectories as well.</param>
		/// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
		public static IEnumerable<OdinMenuItem> AddAllAssetsAtPathCombined(this OdinMenuTree tree, string menuPath, string assetFolderPath, Type type, bool includeSubDirectories = false)
		{
			assetFolderPath = (assetFolderPath ?? "").TrimEnd('/') + "/";
			string text = assetFolderPath.ToLower();
			if (!text.StartsWith("assets/") && !text.StartsWith("packages/"))
			{
				assetFolderPath = "Assets/" + assetFolderPath;
			}
			assetFolderPath = assetFolderPath.TrimEnd('/') + "/";
			List<Func<object>> value = (from x in AssetDatabase.GetAllAssetPaths()
				where includeSubDirectories ? x.StartsWith(assetFolderPath, StringComparison.InvariantCultureIgnoreCase) : (string.Compare(PathUtilities.GetDirectoryName(x).Trim('/'), assetFolderPath.Trim('/'), ignoreCase: true) == 0)
				select x).Select((Func<string, Func<object>>)delegate(string x)
			{
				Object tmp = null;
				return delegate
				{
					if (tmp == (Object)null)
					{
						tmp = AssetDatabase.LoadAssetAtPath(x, type);
					}
					return tmp;
				};
			}).ToList();
			SplitMenuPath(menuPath, out var path, out var name);
			return tree.AddMenuItemAtPath(path, new OdinMenuItem(tree, name, value));
		}

		/// <summary>
		/// Adds all assets at the specified path. Each asset found gets its own menu item inside the specified menu item path.
		/// </summary>
		/// <param name="tree">The tree.</param>
		/// <param name="menuPath">The menu item path.</param>
		/// <param name="assetFolderPath">The asset folder path.</param>
		/// <param name="type">The type.</param>
		/// <param name="includeSubDirectories">Whether to search for assets in subdirectories as well.</param>
		/// <param name="flattenSubDirectories">If true, sub-directories in the assetFolderPath will no longer get its own sub-menu item at the specified menu item path.</param>
		/// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
		public static IEnumerable<OdinMenuItem> AddAllAssetsAtPath(this OdinMenuTree tree, string menuPath, string assetFolderPath, Type type, bool includeSubDirectories = false, bool flattenSubDirectories = false)
		{
			assetFolderPath = (assetFolderPath ?? "").TrimEnd('/') + "/";
			string text = assetFolderPath.ToLower();
			if (!text.StartsWith("assets/") && !text.StartsWith("packages/"))
			{
				assetFolderPath = "Assets/" + assetFolderPath;
			}
			assetFolderPath = assetFolderPath.TrimEnd('/') + "/";
			IEnumerable<string> enumerable = from x in AssetDatabase.GetAllAssetPaths()
				where includeSubDirectories ? x.StartsWith(assetFolderPath, StringComparison.InvariantCultureIgnoreCase) : (string.Compare(PathUtilities.GetDirectoryName(x).Trim('/'), assetFolderPath.Trim('/'), ignoreCase: true) == 0)
				select x;
			menuPath = menuPath ?? "";
			menuPath = menuPath.TrimStart('/');
			HashSet<OdinMenuItem> result = new HashSet<OdinMenuItem>();
			foreach (string item in enumerable)
			{
				Object val = AssetDatabase.LoadAssetAtPath(item, type);
				if (val == (Object)null)
				{
					continue;
				}
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(item);
				string text2 = menuPath;
				if (!flattenSubDirectories)
				{
					string text3 = PathUtilities.GetDirectoryName(item).TrimEnd('/') + "/";
					text3 = text3.Substring(assetFolderPath.Length);
					if (text3.Length != 0)
					{
						text2 = text2.Trim('/') + "/" + text3;
					}
				}
				text2 = text2.Trim('/') + "/" + fileNameWithoutExtension;
				SplitMenuPath(text2, out text2, out var name);
				tree.AddMenuItemAtPath(result, text2, new OdinMenuItem(tree, name, val));
			}
			return result;
		}

		/// <summary>
		/// Adds all assets at the specified path. Each asset found gets its own menu item inside the specified menu item path.
		/// </summary>
		/// <param name="tree">The tree.</param>
		/// <param name="menuPath">The menu item path.</param>
		/// <param name="assetFolderPath">The asset folder path.</param>
		/// <param name="includeSubDirectories">Whether to search for assets in subdirectories as well.</param>
		/// <param name="flattenSubDirectories">If true, sub-directories in the assetFolderPath will no longer get its own sub-menu item at the specified menu item path.</param>
		/// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
		public static IEnumerable<OdinMenuItem> AddAllAssetsAtPath(this OdinMenuTree tree, string menuPath, string assetFolderPath, bool includeSubDirectories = false, bool flattenSubDirectories = false)
		{
			return tree.AddAllAssetsAtPath(menuPath, assetFolderPath, typeof(Object), includeSubDirectories, flattenSubDirectories);
		}

		/// <summary>
		/// Adds the asset at the specified menu item path and returns all menu items created in order to end up at the specified menu path.
		/// </summary>
		/// <param name="tree">The tree.</param>
		/// <param name="menuItemPath">The menu item path.</param>
		/// <param name="assetPath">The asset path.</param>
		/// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
		public static IEnumerable<OdinMenuItem> AddAssetAtPath(this OdinMenuTree tree, string menuItemPath, string assetPath)
		{
			return tree.AddAssetAtPath(menuItemPath, assetPath, typeof(Object));
		}

		/// <summary>
		/// Adds the asset at the specified menu item path and returns all menu items created in order to end up at the specified menu path.
		/// </summary>
		/// <param name="tree">The tree.</param>
		/// <param name="menuItemPath">The menu item path.</param>
		/// <param name="assetPath">The asset path.</param>
		/// <param name="type">The type.</param>
		/// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
		public static IEnumerable<OdinMenuItem> AddAssetAtPath(this OdinMenuTree tree, string menuItemPath, string assetPath, Type type)
		{
			string text = assetPath.ToLower();
			if (!text.StartsWith("assets/") && !text.StartsWith("packages/"))
			{
				assetPath = "Assets/" + assetPath;
			}
			Object value = AssetDatabase.LoadAssetAtPath(assetPath, type);
			SplitMenuPath(menuItemPath, out menuItemPath, out var name);
			return tree.AddMenuItemAtPath(menuItemPath, new OdinMenuItem(tree, name, value));
		}

		/// <summary>
		/// Sorts the entire tree of menu items recursively by name with respects to numbers.
		/// </summary>
		public static IEnumerable<OdinMenuItem> SortMenuItemsByName(this OdinMenuTree tree, bool placeFoldersFirst = true)
		{
			IEnumerable<OdinMenuItem> result = tree.EnumerateTree(includeRootNode: true).SortMenuItemsByName(placeFoldersFirst);
			tree.MarkDirty();
			return result;
		}

		/// <summary>
		/// Sorts the collection of menu items recursively by name with respects to numbers. This is a stable sort, meaning that equivalently ordered items will remain in the same order as they start.
		/// </summary>
		public static IEnumerable<OdinMenuItem> SortMenuItemsByName(this IEnumerable<OdinMenuItem> menuItems, bool placeFoldersFirst = true, bool ignoreLeadingZeroes = true, bool ignoreWhiteSpace = true, bool ignoreCase = false)
		{
			OdinMenuItemComparer odinMenuItemComparer = new OdinMenuItemComparer();
			odinMenuItemComparer.PlaceFoldersFirst = placeFoldersFirst;
			odinMenuItemComparer.IgnoreLeadingZeroes = ignoreLeadingZeroes;
			odinMenuItemComparer.IgnoreWhiteSpace = ignoreWhiteSpace;
			odinMenuItemComparer.IgnoreCase = ignoreCase;
			return SortMenuItemsByName(menuItems, odinMenuItemComparer);
		}

		/// <summary>
		/// Sorts the collection of menu items recursively using a given custom comparison. This is a stable sort, meaning that equivalently ordered items will remain in the same order as they start.
		/// </summary>
		public static IEnumerable<OdinMenuItem> SortMenuItemsByName(this IEnumerable<OdinMenuItem> menuItems, Comparison<OdinMenuItem> comparison)
		{
			if (comparison == null)
			{
				throw new ArgumentNullException("comparison");
			}
			OdinMenuItemComparer comparer = new OdinMenuItemComparer(comparison);
			return SortMenuItemsByName(menuItems, comparer);
		}

		private static IEnumerable<OdinMenuItem> SortMenuItemsByName(IEnumerable<OdinMenuItem> menuItems, IComparer<OdinMenuItem> comparer)
		{
			OdinMenuItem odinMenuItem = null;
			foreach (OdinMenuItem menuItem in menuItems)
			{
				if (odinMenuItem == null)
				{
					odinMenuItem = menuItem;
				}
				List<OdinMenuItem> childMenuItems = menuItem.ChildMenuItems.OrderBy((OdinMenuItem item) => item, comparer).ToList();
				menuItem.SetChildMenuItems(childMenuItems);
			}
			if (odinMenuItem != null && odinMenuItem.MenuTree != null)
			{
				odinMenuItem.MenuTree.MarkDirty();
			}
			return menuItems;
		}

		/// <summary>
		/// Adds the specified object at the specified menu item path and returns all menu items created in order to end up at the specified menu path.
		/// </summary>
		/// <param name="tree">The tree.</param>
		/// <param name="menuPath">The menu path.</param>
		/// <param name="instance">The object instance.</param>
		/// <param name="forceShowOdinSerializedMembers">Set this to true if you want Odin serialzied members such as dictionaries and generics to be shown as well.</param>
		/// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
		public static IEnumerable<OdinMenuItem> AddObjectAtPath(this OdinMenuTree tree, string menuPath, object instance, bool forceShowOdinSerializedMembers = false)
		{
			SplitMenuPath(menuPath, out menuPath, out var name);
			if (forceShowOdinSerializedMembers && !Object.op_Implicit(instance as Object))
			{
				return tree.AddMenuItemAtPath(menuPath, new OdinMenuItem(tree, name, new SerializedValueWrapper(instance)));
			}
			return tree.AddMenuItemAtPath(menuPath, new OdinMenuItem(tree, name, instance));
		}

		/// <summary>
		/// Assigns the specified icon to all menu items in the collection with the specified ObjectInstanceType.
		/// </summary>
		public static IEnumerable<OdinMenuItem> AddIcons<T>(this IEnumerable<OdinMenuItem> menuItems, Func<T, Texture> getIcon)
		{
			foreach (OdinMenuItem menuItem in menuItems)
			{
				if (menuItem.Value != null && menuItem.Value is T)
				{
					OdinMenuItem localItem = menuItem;
					localItem.IconGetter = () => getIcon((T)localItem.Value);
				}
			}
			return menuItems;
		}

		/// <summary>
		/// Assigns the specified icon to all menu items in the collection with the specified ObjectInstanceType.
		/// </summary>
		public static IEnumerable<OdinMenuItem> AddIcons<T>(this IEnumerable<OdinMenuItem> menuItems, Func<T, Sprite> getIcon)
		{
			foreach (OdinMenuItem menuItem in menuItems)
			{
				if (menuItem.Value != null && menuItem.Value is T)
				{
					OdinMenuItem localItem = menuItem;
					localItem.IconGetter = () => (Texture)(object)AssetPreview.GetAssetPreview((Object)(object)getIcon((T)localItem.Value));
				}
			}
			return menuItems;
		}

		/// <summary>
		/// Assigns the specified icon to all menu items in the collection.
		/// </summary>
		public static IEnumerable<OdinMenuItem> AddIcons(this IEnumerable<OdinMenuItem> menuItems, Func<OdinMenuItem, Texture> getIcon)
		{
			foreach (OdinMenuItem menuItem in menuItems)
			{
				OdinMenuItem localItem = menuItem;
				localItem.IconGetter = () => getIcon(localItem);
			}
			return menuItems;
		}

		/// <summary>
		/// Assigns the specified icon to all menu items in the collection.
		/// </summary>
		public static IEnumerable<OdinMenuItem> AddIcons(this IEnumerable<OdinMenuItem> menuItems, Func<OdinMenuItem, Sprite> getIcon)
		{
			foreach (OdinMenuItem menuItem in menuItems)
			{
				OdinMenuItem localItem = menuItem;
				localItem.IconGetter = () => (Texture)(object)AssetPreview.GetAssetPreview((Object)(object)getIcon(localItem));
			}
			return menuItems;
		}

		/// <summary>
		/// Assigns the specified icon to the last menu item in the collection.
		/// </summary>
		public static IEnumerable<OdinMenuItem> AddIcon(this IEnumerable<OdinMenuItem> menuItems, Sprite icon)
		{
			menuItems.AddIcon((Texture)(object)AssetPreview.GetAssetPreview((Object)(object)icon));
			return menuItems;
		}

		/// <summary>
		/// Assigns the specified icon to the last menu item in the collection.
		/// </summary>
		public static IEnumerable<OdinMenuItem> AddIcon(this IEnumerable<OdinMenuItem> menuItems, EditorIcon icon)
		{
			menuItems.AddIcon(icon.Highlighted, (Texture)(object)icon.Raw);
			return menuItems;
		}

		/// <summary>
		/// Assigns the specified icon to the last menu item in the collection.
		/// </summary>
		public static IEnumerable<OdinMenuItem> AddIcon(this IEnumerable<OdinMenuItem> menuItems, Texture icon)
		{
			OdinMenuItem odinMenuItem = menuItems.LastOrDefault();
			if (odinMenuItem != null)
			{
				odinMenuItem.Icon = icon;
				odinMenuItem.IconSelected = icon;
			}
			return menuItems;
		}

		/// <summary>
		/// Assigns the specified icon to the last menu item in the collection.
		/// </summary>
		public static IEnumerable<OdinMenuItem> AddIcon(this IEnumerable<OdinMenuItem> menuItems, Texture icon, Texture iconSelected)
		{
			OdinMenuItem odinMenuItem = menuItems.LastOrDefault();
			if (odinMenuItem != null)
			{
				odinMenuItem.Icon = icon;
				odinMenuItem.IconSelected = iconSelected;
			}
			return menuItems;
		}

		/// <summary>
		/// Assigns the specified icon to all menu items in the collection.
		/// </summary>
		public static IEnumerable<OdinMenuItem> AddIcons(this IEnumerable<OdinMenuItem> menuItems, EditorIcon icon)
		{
			foreach (OdinMenuItem menuItem in menuItems)
			{
				menuItem.Icon = icon.Highlighted;
				menuItem.IconSelected = (Texture)(object)icon.Raw;
			}
			return menuItems;
		}

		/// <summary>
		/// Assigns the specified icon to all menu items in the collection.
		/// </summary>
		public static IEnumerable<OdinMenuItem> AddIcons(this IEnumerable<OdinMenuItem> menuItems, Texture icon)
		{
			foreach (OdinMenuItem menuItem in menuItems)
			{
				menuItem.Icon = icon;
			}
			return menuItems;
		}

		/// <summary>
		/// Assigns the specified icon to all menu items in the collection.
		/// </summary>
		public static IEnumerable<OdinMenuItem> AddIcons(this IEnumerable<OdinMenuItem> menuItems, Texture icon, Texture iconSelected)
		{
			foreach (OdinMenuItem menuItem in menuItems)
			{
				menuItem.Icon = icon;
				menuItem.IconSelected = iconSelected;
			}
			return menuItems;
		}

		/// <summary>
		/// Assigns the asset mini thumbnail as an icon to all menu items in the collection. If the menu items object is null then a Unity folder icon is assigned.
		/// </summary>
		public static IEnumerable<OdinMenuItem> AddThumbnailIcons(this IEnumerable<OdinMenuItem> menuItems, bool preferAssetPreviewAsIcon = false)
		{
			foreach (OdinMenuItem menuItem in menuItems)
			{
				menuItem.AddThumbnailIcon(preferAssetPreviewAsIcon);
			}
			return menuItems;
		}

		/// <summary>
		/// Assigns the asset mini thumbnail as an icon to all menu items in the collection. If the menu items object is null then a Unity folder icon is assigned.
		/// </summary>
		public static OdinMenuItem AddThumbnailIcon(this OdinMenuItem item, bool preferAssetPreviewAsIcon)
		{
			object value = item.Value;
			Object unityObject = value as Object;
			if (Object.op_Implicit(unityObject))
			{
				if (preferAssetPreviewAsIcon)
				{
					item.IconGetter = () => (Texture)(object)GUIHelper.GetAssetThumbnail(unityObject, ((object)unityObject).GetType(), preferAssetPreviewAsIcon);
				}
				else
				{
					item.Icon = (Texture)(object)GUIHelper.GetAssetThumbnail(unityObject, ((object)unityObject).GetType(), preferAssetPreviewAsIcon);
				}
				return item;
			}
			Type type = value as Type;
			if (type != null)
			{
				if (preferAssetPreviewAsIcon)
				{
					item.IconGetter = () => (Texture)(object)GUIHelper.GetAssetThumbnail(null, type, preferAssetPreviewAsIcon);
				}
				else
				{
					item.Icon = (Texture)(object)GUIHelper.GetAssetThumbnail(null, type, preferAssetPreviewAsIcon);
				}
				return item;
			}
			string text = value as string;
			if (text != null && text != null)
			{
				if (File.Exists(text))
				{
					item.Icon = (Texture)(object)InternalEditorUtility.GetIconForFile(text);
				}
				else if (Directory.Exists(text))
				{
					item.Icon = (Texture)(object)EditorIcons.UnityFolderIcon;
				}
			}
			return item;
		}

		private static void SplitMenuPath(string menuPath, out string path, out string name)
		{
			menuPath = menuPath.Trim('/');
			int num = menuPath.LastIndexOf('/');
			if (num == -1)
			{
				path = "";
				name = menuPath;
			}
			else
			{
				path = menuPath.Substring(0, num);
				name = menuPath.Substring(num + 1);
			}
		}

		private static bool ReplaceDollarSignWithAssetName(ref string menuItem, string name)
		{
			if (menuItem == null)
			{
				return false;
			}
			if (menuItem == "$")
			{
				menuItem = name;
			}
			if (menuItem.StartsWith("$/"))
			{
				menuItem = name + menuItem.Substring(2);
			}
			if (menuItem.EndsWith("/$"))
			{
				menuItem = menuItem.Substring(0, menuItem.Length - 1) + name;
			}
			if (menuItem.Contains("/$/"))
			{
				menuItem = menuItem.Replace("/$/", "/" + name + "/");
				return true;
			}
			return false;
		}
	}
}
