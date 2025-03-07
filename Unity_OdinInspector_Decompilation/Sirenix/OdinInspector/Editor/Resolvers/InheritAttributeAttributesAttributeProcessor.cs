using System;
using System.Collections.Generic;
using System.Reflection;

namespace Sirenix.OdinInspector.Editor.Resolvers
{
	/// <summary>
	/// This attribute processor will take any attribute already applied to the property with the <see cref="T:Sirenix.OdinInspector.IncludeMyAttributesAttribute" /> applied to,
	/// and take all attributes applied to the attribute (except any <see cref="T:System.AttributeUsageAttribute" />) and add to them to the property.
	/// This allows for adding attributes to attributes in the property system.
	/// </summary>
	[ResolverPriority(-100000.0)]
	public class InheritAttributeAttributesAttributeProcessor : OdinAttributeProcessor
	{
		private static readonly Type TypeOf_OnInspectorInitAttribute = typeof(OnInspectorInitAttribute);

		private static readonly Type TypeOf_PropertyOrderAttribute = typeof(PropertyOrderAttribute);

		/// <summary>
		/// Looks for attributes in the attributes list with a <see cref="T:Sirenix.OdinInspector.IncludeMyAttributesAttribute" /> applied, and adds the attribute from those attributes to the property.
		/// </summary>
		/// <param name="parentProperty">The parent of the member.</param>
		/// <param name="member">The member that is being processed.</param>
		/// <param name="attributes">The list of attributes currently applied to the property.</param>
		public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
		{
			bool flag = false;
			bool flag2 = false;
			for (int num = attributes.Count - 1; num >= 0; num--)
			{
				Type type = attributes[num].GetType();
				if (type.IsDefined(typeof(IncludeMyAttributesAttribute), inherit: false))
				{
					object[] customAttributes = type.GetCustomAttributes(inherit: false);
					foreach (object obj in customAttributes)
					{
						if (!(obj is AttributeUsageAttribute))
						{
							attributes.Add(obj as Attribute);
						}
					}
				}
				if (type == TypeOf_OnInspectorInitAttribute)
				{
					flag = true;
				}
				else if (type == TypeOf_PropertyOrderAttribute)
				{
					flag2 = true;
				}
			}
			if (flag && !flag2 && member is MethodInfo)
			{
				attributes.Add(new PropertyOrderAttribute(-2.14748365E+09f));
			}
		}
	}
}
