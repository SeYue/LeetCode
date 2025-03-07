using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class DefaultValidationMemberSelector : IMemberSelector
	{
		public static readonly DefaultValidationMemberSelector Instance = new DefaultValidationMemberSelector();

		private static Dictionary<Type, List<MemberInfo>> ResultCache = new Dictionary<Type, List<MemberInfo>>(FastTypeComparer.Instance);

		private static readonly object LOCK = new object();

		public IList<MemberInfo> SelectMembers(Type type)
		{
			lock (LOCK)
			{
				if (!ResultCache.TryGetValue(type, out var value))
				{
					value = ScanForMembers(type);
					ResultCache[type] = value;
					return value;
				}
				return value;
			}
		}

		private static List<MemberInfo> ScanForMembers(Type type)
		{
			List<MemberInfo> list = new List<MemberInfo>();
			foreach (MemberInfo allMember in type.GetAllMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy))
			{
				if (allMember.DeclaringType == typeof(Object))
				{
					continue;
				}
				if (allMember is FieldInfo)
				{
					FieldInfo fieldInfo = allMember as FieldInfo;
					if (!fieldInfo.IsStatic || fieldInfo.IsDefined<ShowInInspectorAttribute>())
					{
						list.Add(allMember);
					}
				}
				else if (allMember is PropertyInfo)
				{
					PropertyInfo propertyInfo = allMember as PropertyInfo;
					if (!propertyInfo.IsStatic() || propertyInfo.IsDefined<ShowInInspectorAttribute>())
					{
						list.Add(propertyInfo);
					}
				}
			}
			return list;
		}
	}
}
