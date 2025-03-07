using System;
using Sirenix.Utilities.Editor;

namespace Sirenix.OdinInspector.Editor
{
	[ResolverPriority(1000.0)]
	public class AtomAndEnumPropertyResolver<TValue> : OdinPropertyResolver<TValue>, IMaySupportPrefabModifications
	{
		public bool MaySupportPrefabModifications => true;

		public override int ChildNameToIndex(string name)
		{
			return -1;
		}

		public override int ChildNameToIndex(ref StringSlice name)
		{
			return -1;
		}

		public override bool CanResolveForPropertyFilter(InspectorProperty property)
		{
			Type typeOfValue = property.ValueEntry.TypeOfValue;
			if (!typeOfValue.IsEnum)
			{
				return typeOfValue.IsMarkedAtomic();
			}
			return true;
		}

		protected override int GetChildCount(TValue value)
		{
			return 0;
		}

		public override InspectorPropertyInfo GetChildInfo(int childIndex)
		{
			throw new NotSupportedException();
		}
	}
}
