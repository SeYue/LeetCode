using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	[ResolverPriority(-5.0)]
	public class ProcessedMemberPropertyResolver<T> : BaseMemberPropertyResolver<T>, IDisposable
	{
		private List<OdinPropertyProcessor> processors;

		public virtual void Dispose()
		{
			if (processors != null)
			{
				for (int i = 0; i < processors.Count; i++)
				{
					(processors[i] as IDisposable)?.Dispose();
				}
			}
		}

		protected override InspectorPropertyInfo[] GetPropertyInfos()
		{
			if (processors == null)
			{
				processors = OdinPropertyProcessorLocator.GetMemberProcessors(base.Property);
			}
			bool includeSpeciallySerializedMembers = !base.Property.ValueEntry.SerializationBackend.IsUnity;
			List<InspectorPropertyInfo> list = InspectorPropertyInfoUtility.CreateMemberProperties(base.Property, typeof(T), includeSpeciallySerializedMembers);
			for (int i = 0; i < processors.Count; i++)
			{
				ProcessedMemberPropertyResolverExtensions.ProcessingOwnerType = typeof(T);
				try
				{
					processors[i].ProcessMemberProperties(list);
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
			return InspectorPropertyInfoUtility.BuildPropertyGroupsAndFinalize(base.Property, typeof(T), list, includeSpeciallySerializedMembers);
		}
	}
}
