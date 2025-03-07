using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	[ResolverPriority(1000.0)]
	public class TypeDefinitionGroupAttributeProcessor : OdinAttributeProcessor
	{
		private static readonly Dictionary<Type, bool> HadResultCache = new Dictionary<Type, bool>(FastTypeComparer.Instance);

		public override bool CanProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member)
		{
			Type returnType = member.GetReturnType();
			if (returnType == null)
			{
				return false;
			}
			if (HadResultCache.TryGetValue(returnType, out var value))
			{
				return value;
			}
			return true;
		}

		public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
		{
			Type returnType = member.GetReturnType();
			Type type = returnType;
			if (type == null)
			{
				return;
			}
			bool flag = false;
			if (HadResultCache.TryGetValue(returnType, out var value))
			{
				if (!value)
				{
					return;
				}
				flag = true;
			}
			while (type != null)
			{
				AssemblyTypeFlags assemblyTypeFlag = type.Assembly.GetAssemblyTypeFlag();
				if (assemblyTypeFlag == AssemblyTypeFlags.OtherTypes || assemblyTypeFlag == AssemblyTypeFlags.UnityTypes)
				{
					break;
				}
				if (type.IsDefined(typeof(PropertyGroupAttribute), inherit: false))
				{
					value = true;
					object[] customAttributes = type.GetCustomAttributes(typeof(PropertyGroupAttribute), inherit: false);
					for (int i = 0; i < customAttributes.Length; i++)
					{
						attributes.Add(customAttributes[i] as Attribute);
					}
				}
				type = type.BaseType;
			}
			if (!flag)
			{
				HadResultCache.Add(returnType, value);
			}
		}
	}
}
