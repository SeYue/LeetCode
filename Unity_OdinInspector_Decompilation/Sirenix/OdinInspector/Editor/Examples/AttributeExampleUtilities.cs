using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.Examples
{
	public static class AttributeExampleUtilities
	{
		private class CategoryComparer : IComparer<OdinMenuItem>
		{
			private static readonly Dictionary<string, int> Order = new Dictionary<string, int>
			{
				{ "Essentials", -10 },
				{ "Misc", 8 },
				{ "Meta", 9 },
				{ "Unity", 10 },
				{ "Debug", 50 }
			};

			public int Compare(OdinMenuItem x, OdinMenuItem y)
			{
				if (!Order.TryGetValue(x.Name, out var value))
				{
					value = 0;
				}
				if (!Order.TryGetValue(y.Name, out var value2))
				{
					value2 = 0;
				}
				if (value == value2)
				{
					return x.Name.CompareTo(y.Name);
				}
				return value.CompareTo(value2);
			}
		}

		private static readonly CategoryComparer CategorySorter;

		private static readonly Type[] AttributeTypes;

		private static readonly Dictionary<Type, OdinRegisterAttributeAttribute> AttributeRegisterMap;

		static AttributeExampleUtilities()
		{
			CategorySorter = new CategoryComparer();
			AttributeRegisterMap = (from OdinRegisterAttributeAttribute attr in AssemblyUtilities.GetAllAssemblies().SelectMany((Assembly a) => a.GetAttributes<OdinRegisterAttributeAttribute>(inherit: true)).Concat(InternalAttributeRegistry.Attributes)
				where OdinInspectorVersion.IsEnterprise || !attr.IsEnterprise
				select attr).ToDictionary((OdinRegisterAttributeAttribute x) => x.AttributeType);
			AttributeTypes = AttributeRegisterMap.Keys.ToArray();
		}

		public static IEnumerable<Type> GetAllOdinAttributes()
		{
			return AttributeTypes;
		}

		public static IEnumerable<string> GetAttributeCategories(Type attributeType)
		{
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			if (!AttributeRegisterMap.TryGetValue(attributeType, out var value) || value.Categories == null)
			{
				return new string[1] { "Uncategorized" };
			}
			return from x in value.Categories.Split(',')
				select x.Trim();
		}

		public static string GetAttributeDescription(Type attributeType)
		{
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			if (AttributeRegisterMap.TryGetValue(attributeType, out var value))
			{
				return value.Description;
			}
			return null;
		}

		public static string GetOnlineDocumentationUrl(Type attributeType)
		{
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			if (AttributeRegisterMap.TryGetValue(attributeType, out var value))
			{
				return value.DocumentationUrl;
			}
			return null;
		}

		public static bool GetIsEnterprise(Type attributeType)
		{
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			if (AttributeRegisterMap.TryGetValue(attributeType, out var value))
			{
				return value.IsEnterprise;
			}
			return false;
		}

		public static void BuildMenuTree(OdinMenuTree tree)
		{
			foreach (Type allOdinAttribute in GetAllOdinAttributes())
			{
				string searchString = allOdinAttribute.Name + " " + string.Join(" ", (from x in GetAttributeExampleInfos(allOdinAttribute)
					select x.Name).ToArray());
				foreach (string attributeCategory in GetAttributeCategories(allOdinAttribute))
				{
					OdinMenuItem menuItem = new OdinMenuItem(tree, allOdinAttribute.GetNiceName().Replace("Attribute", "").SplitPascalCase(), allOdinAttribute)
					{
						Value = allOdinAttribute,
						SearchString = searchString
					};
					searchString = null;
					tree.AddMenuItemAtPath(attributeCategory, menuItem);
				}
			}
			tree.MenuItems.Sort(CategorySorter);
			tree.MarkDirty();
		}

		public static AttributeExampleInfo[] GetAttributeExampleInfos(Type attributeType)
		{
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			if (!InternalAttributeExampleInfoMap.Map.TryGetValue(attributeType, out var value))
			{
				return new AttributeExampleInfo[0];
			}
			return value;
		}

		public static OdinAttributeExampleItem GetExample<T>() where T : Attribute
		{
			return GetExample(typeof(T));
		}

		public static OdinAttributeExampleItem GetExample(Type attributeType)
		{
			AttributeRegisterMap.TryGetValue(attributeType, out var value);
			return new OdinAttributeExampleItem(attributeType, value);
		}
	}
}
