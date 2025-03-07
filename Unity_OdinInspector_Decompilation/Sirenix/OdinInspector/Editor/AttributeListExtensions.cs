using System;
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Extension method for List&lt;Attribute&gt;
	/// </summary>
	public static class AttributeListExtensions
	{
		/// <summary>
		/// Determines whether the list contains a specific attribute.
		/// </summary>
		/// <typeparam name="T">The type of attribute.</typeparam>
		/// <param name="attributeList">The attribute list.</param>
		/// <returns>
		///   <c>true</c> if the specified attribute list has attribute; otherwise, <c>false</c>.
		/// </returns>
		public static bool HasAttribute<T>(this IList<Attribute> attributeList) where T : Attribute
		{
			int count = attributeList.Count;
			for (int i = 0; i < count; i++)
			{
				if (attributeList[i] is T)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Adds the attribute if not exist.
		/// </summary>
		/// <typeparam name="T">The type of attribute.</typeparam>
		/// <param name="attributeList">The attribute list.</param>
		/// <returns></returns>
		public static T GetOrAddAttribute<T>(this List<Attribute> attributeList) where T : Attribute, new()
		{
			int count = attributeList.Count;
			T val;
			for (int i = 0; i < count; i++)
			{
				val = attributeList[i] as T;
				if (val != null)
				{
					return val;
				}
			}
			val = new T();
			attributeList.Add(val);
			return val;
		}

		/// <summary>
		/// Adds the attribute if not exist.
		/// </summary>
		/// <typeparam name="T">The type of attribute.</typeparam>
		/// <param name="attributeList">The attribute list.</param>
		/// <returns></returns>
		public static T GetAttribute<T>(this IList<Attribute> attributeList) where T : Attribute
		{
			for (int i = 0; i < attributeList.Count; i++)
			{
				T val = attributeList[i] as T;
				if (val != null)
				{
					return val;
				}
			}
			return null;
		}

		/// <summary>
		/// Adds the attribute if not exist.
		/// </summary>
		/// <typeparam name="T">The type of attribute.</typeparam>
		/// <param name="attributeList">The attribute list.</param>
		/// <returns></returns>
		public static T Add<T>(this List<Attribute> attributeList) where T : Attribute, new()
		{
			T val = new T();
			attributeList.Add(val);
			return val;
		}

		/// <summary>
		/// Adds the attribute if not exist.
		/// </summary>
		/// <typeparam name="T">The type of attribute.</typeparam>
		/// <param name="attributeList">The attribute list.</param>
		/// <param name="attr">The attribute.</param>
		/// <returns></returns>
		[Obsolete("This method is obsolete. Do something else!", true)]
		public static bool GetOrAddAttribute<T>(this List<Attribute> attributeList, T attr) where T : Attribute
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Removes the type of the attribute of.
		/// </summary>
		/// <typeparam name="T">The type of attribute.</typeparam>
		/// <param name="attributeList">The attribute list.</param>
		/// <returns></returns>
		public static bool RemoveAttributeOfType<T>(this List<Attribute> attributeList) where T : Attribute
		{
			int num = attributeList.Count;
			bool result = false;
			for (int i = 0; i < num; i++)
			{
				if (attributeList[i] is T)
				{
					attributeList.RemoveAt(i);
					i--;
					num--;
					result = true;
				}
			}
			return result;
		}
	}
}
