using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	[OdinDontRegister]
	public class StaticRootPropertyResolver<T> : BaseMemberPropertyResolver<T>
	{
		private Type targetType;

		private PropertyContext<bool> allowObsoleteMembers;

		protected override bool AllowNullValues => true;

		protected override InspectorPropertyInfo[] GetPropertyInfos()
		{
			targetType = base.ValueEntry.TypeOfValue;
			IEnumerable<MemberInfo> allMembers = targetType.GetAllMembers(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			List<InspectorPropertyInfo> list = new List<InspectorPropertyInfo>();
			allowObsoleteMembers = base.Property.Context.GetGlobal("ALLOW_OBSOLETE_STATIC_MEMBERS", defaultValue: false);
			foreach (MemberInfo item in allMembers.Where(Filter).OrderBy(Order))
			{
				List<Attribute> list2 = new List<Attribute>();
				InspectorPropertyInfoUtility.ProcessAttributes(base.Property, item, list2);
				if (item is MethodInfo)
				{
					if ((item as MethodInfo).IsGenericMethodDefinition)
					{
						continue;
					}
					if (!list2.HasAttribute<ButtonAttribute>() && !list2.HasAttribute<OnInspectorGUIAttribute>())
					{
						list2.Add(new ButtonAttribute(ButtonSizes.Medium));
					}
				}
				SerializationBackend serializationBackend = ((item is MethodInfo) ? SerializationBackend.None : StaticInspectorSerializationBackend.Default);
				InspectorPropertyInfo inspectorPropertyInfo = InspectorPropertyInfo.CreateForMember(item, allowEditable: true, serializationBackend, list2);
				InspectorPropertyInfo inspectorPropertyInfo2 = null;
				int index = -1;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].PropertyName == inspectorPropertyInfo.PropertyName)
					{
						index = i;
						inspectorPropertyInfo2 = list[i];
						break;
					}
				}
				if (inspectorPropertyInfo2 != null)
				{
					bool flag = true;
					if (item.SignaturesAreEqual(inspectorPropertyInfo2.GetMemberInfo()))
					{
						flag = false;
						list.RemoveAt(index);
					}
					if (flag)
					{
						MemberInfo privateMemberAlias = InspectorPropertyInfoUtility.GetPrivateMemberAlias(inspectorPropertyInfo2.GetMemberInfo(), inspectorPropertyInfo2.TypeOfOwner.GetNiceName(), " -> ");
						list[index] = InspectorPropertyInfo.CreateForMember(privateMemberAlias, allowEditable: true, serializationBackend, list2);
					}
				}
				list.Add(inspectorPropertyInfo);
			}
			return InspectorPropertyInfoUtility.BuildPropertyGroupsAndFinalize(base.Property, targetType, list, includeSpeciallySerializedMembers: false);
		}

		private int Order(MemberInfo arg1)
		{
			if (arg1 is FieldInfo)
			{
				return 1;
			}
			if (arg1 is PropertyInfo)
			{
				return 2;
			}
			if (arg1 is MethodInfo)
			{
				return 3;
			}
			return 4;
		}

		private bool Filter(MemberInfo member)
		{
			if (member.DeclaringType == typeof(object) && targetType != typeof(object))
			{
				return false;
			}
			if (!(member is FieldInfo) && !(member is PropertyInfo) && !(member is MethodInfo))
			{
				return false;
			}
			if (member is FieldInfo && (member as FieldInfo).IsSpecialName)
			{
				return false;
			}
			if (member is MethodInfo && (member as MethodInfo).IsSpecialName)
			{
				return false;
			}
			if (member is PropertyInfo && (member as PropertyInfo).IsSpecialName)
			{
				return false;
			}
			if (member.IsDefined<CompilerGeneratedAttribute>())
			{
				return false;
			}
			if (!allowObsoleteMembers.Value && member.IsDefined<ObsoleteAttribute>())
			{
				return false;
			}
			return true;
		}
	}
}
