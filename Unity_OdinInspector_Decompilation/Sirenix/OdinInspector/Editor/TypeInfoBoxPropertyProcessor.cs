using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor
{
	[ResolverPriority(-10.0)]
	public class TypeInfoBoxPropertyProcessor<T> : OdinPropertyProcessor<T, TypeInfoBoxAttribute>
	{
		public override void ProcessMemberProperties(List<InspectorPropertyInfo> memberInfos)
		{
			TypeInfoBoxAttribute attribute = base.Property.GetAttribute<TypeInfoBoxAttribute>();
			memberInfos.AddDelegate("InjectedTypeInfoBox", delegate
			{
			}, -100000f, new InfoBoxAttribute(attribute.Message), new OnInspectorGUIAttribute("@"));
		}
	}
}
