using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Find attributes attached to the type definition of a property and adds to them to attribute list.
	/// </summary>
	[ResolverPriority(1000.0)]
	public class TypeDefinitionAttributeProcessor : OdinAttributeProcessor
	{
		private static readonly Dictionary<Type, bool> HadResultCache = new Dictionary<Type, bool>(FastTypeComparer.Instance);

		/// <summary>
		/// This attribute processor can only process for properties.
		/// </summary>
		/// <param name="parentProperty">The parent of the specified member.</param>
		/// <param name="member">The member to process.</param>
		/// <returns><c>false</c>.</returns>
		public override bool CanProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member)
		{
			return false;
		}

		/// <summary>
		/// This attribute processor can only process for properties with an attached value entry.
		/// </summary>
		/// <param name="property">The property to process.</param>
		/// <returns><c>true</c> if the specified property has a value entry. Otherwise <c>false</c>.</returns>
		public override bool CanProcessSelfAttributes(InspectorProperty property)
		{
			IPropertyValueEntry valueEntry = property.ValueEntry;
			if (valueEntry == null)
			{
				return false;
			}
			if (FastTypeComparer.Instance.Equals(valueEntry.TypeOfValue, valueEntry.BaseValueType) && HadResultCache.TryGetValue(valueEntry.BaseValueType, out var value))
			{
				return value;
			}
			return true;
		}

		/// <summary>
		/// Finds all attributes attached to the type and base types of the specified property value and adds them to the attribute list.
		/// </summary>
		/// <param name="property">The property to process.</param>
		/// <param name="attributes">The list of attributes for the property.</param>
		public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
		{
			Type type = property.ValueEntry.TypeOfValue;
			if (FastTypeComparer.Instance.Equals(type, property.ValueEntry.BaseValueType))
			{
				bool flag = false;
				Type key = type;
				if (HadResultCache.TryGetValue(key, out var value))
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
					if (type.IsDefined(typeof(Attribute), inherit: false))
					{
						value = true;
						attributes.AddRange(type.GetAttributes(inherit: false));
					}
					type = type.BaseType;
				}
				if (!flag)
				{
					HadResultCache.Add(key, value);
				}
				return;
			}
			while (type != null)
			{
				AssemblyTypeFlags assemblyTypeFlag2 = type.Assembly.GetAssemblyTypeFlag();
				if (assemblyTypeFlag2 == AssemblyTypeFlags.OtherTypes || assemblyTypeFlag2 == AssemblyTypeFlags.UnityTypes)
				{
					break;
				}
				if (type.IsDefined(typeof(Attribute), inherit: false))
				{
					attributes.AddRange(type.GetAttributes(inherit: false));
				}
				type = type.BaseType;
			}
			type = property.ValueEntry.BaseValueType;
			if (!type.IsInterface)
			{
				return;
			}
			while (type != null)
			{
				AssemblyTypeFlags assemblyTypeFlag3 = type.Assembly.GetAssemblyTypeFlag();
				if (assemblyTypeFlag3 != AssemblyTypeFlags.OtherTypes && assemblyTypeFlag3 != AssemblyTypeFlags.UnityTypes)
				{
					if (type.IsDefined(typeof(Attribute), inherit: false))
					{
						attributes.AddRange(type.GetAttributes(inherit: false));
					}
					type = type.BaseType;
					continue;
				}
				break;
			}
		}
	}
}
