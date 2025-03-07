using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	[ResolverPriority(-1000000.0)]
	public class DelayedAttributeProcessor<T> : OdinPropertyProcessor<T, DelayedAttribute> where T : struct
	{
		public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected O, but got Unknown
			for (int i = 0; i < propertyInfos.Count; i++)
			{
				propertyInfos[i].GetEditableAttributesList().Add((Attribute)new DelayedAttribute());
			}
		}
	}
}
