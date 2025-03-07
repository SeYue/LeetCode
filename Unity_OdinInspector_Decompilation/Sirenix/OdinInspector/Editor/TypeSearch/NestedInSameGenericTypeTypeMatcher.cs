using System;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
	public class NestedInSameGenericTypeTypeMatcher : TypeMatcher
	{
		public class Creator : TypeMatcherCreator
		{
			public override bool TryCreateMatcher(TypeSearchInfo info, out TypeMatcher matcher)
			{
				matcher = null;
				if (info.Targets.Length == 0 || !info.MatchType.IsNested || !info.Targets[0].IsNested)
				{
					return false;
				}
				if (!info.MatchType.DeclaringType.IsGenericType || !info.Targets[0].DeclaringType.IsGenericType)
				{
					return false;
				}
				Type genericTypeDefinition = info.MatchType.DeclaringType.GetGenericTypeDefinition();
				if (genericTypeDefinition != info.Targets[0].DeclaringType.GetGenericTypeDefinition())
				{
					return false;
				}
				matcher = new NestedInSameGenericTypeTypeMatcher
				{
					info = info,
					matchTypeGenericDefinition = genericTypeDefinition
				};
				return true;
			}
		}

		private TypeSearchInfo info;

		private Type matchTypeGenericDefinition;

		public override string Name => "Nested In Same Generic Type ---> Type<T1, [, T2]>.NestedType : Type<T1, [, T2]>.Match<Target>";

		public override Type Match(Type[] targets, ref bool stopMatching)
		{
			if (targets.Length != 1)
			{
				return null;
			}
			Type type = targets[0];
			if (!type.IsNested)
			{
				return null;
			}
			if (!type.DeclaringType.IsGenericType)
			{
				return null;
			}
			if (matchTypeGenericDefinition != type.DeclaringType.GetGenericTypeDefinition())
			{
				return null;
			}
			Type[] genericArguments = type.GetGenericArguments();
			if (info.MatchType.AreGenericConstraintsSatisfiedBy(genericArguments))
			{
				return info.MatchType.MakeGenericType(genericArguments);
			}
			return null;
		}
	}
}
