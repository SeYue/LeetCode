using System;
using System.Collections.Generic;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
	public class GenericParameterInferenceTypeMatcher : TypeMatcher
	{
		public class Creator : TypeMatcherCreator
		{
			public override bool TryCreateMatcher(TypeSearchInfo info, out TypeMatcher matcher)
			{
				matcher = null;
				int num = 0;
				if (!info.MatchType.IsGenericType)
				{
					return false;
				}
				for (int i = 0; i < info.Targets.Length; i++)
				{
					if (info.Targets[i].IsGenericParameter)
					{
						num++;
					}
				}
				if (num == 0)
				{
					return false;
				}
				bool[] array = new bool[info.Targets.Length];
				for (int j = 0; j < array.Length; j++)
				{
					array[j] = info.Targets[j].IsGenericParameter;
				}
				Type[] genericArguments = info.MatchType.GetGenericArguments();
				Type[] genericArguments2 = info.MatchType.GetGenericTypeDefinition().GetGenericArguments();
				matcher = new GenericParameterInferenceTypeMatcher
				{
					info = info,
					genericParameterTargetCount = num,
					targetIsGenericParameterCached = array,
					typeArrayOfGenericParameterTargetCountSize = new Type[num],
					matchTypeGenericArgs = new Type[genericArguments.Length],
					matchTypeGenericArgs_Backup = genericArguments,
					matchTypeGenericDefinitionArgs = new Type[genericArguments2.Length],
					matchTypeGenericDefinitionArgs_Backup = genericArguments2
				};
				return true;
			}
		}

		private static readonly Dictionary<Type, Type[]> GenericParameterConstraintsCache = new Dictionary<Type, Type[]>(FastTypeComparer.Instance);

		private static readonly Dictionary<Type, Type[]> GenericArgumentsCache = new Dictionary<Type, Type[]>(FastTypeComparer.Instance);

		private TypeSearchInfo info;

		private int genericParameterTargetCount;

		private bool[] targetIsGenericParameterCached;

		private Type[] typeArrayOfGenericParameterTargetCountSize;

		private Type[] matchTypeGenericArgs_Backup;

		private Type[] matchTypeGenericDefinitionArgs_Backup;

		private Type[] matchTypeGenericArgs;

		private Type[] matchTypeGenericDefinitionArgs;

		private static readonly object LOCK = new object();

		private static readonly Dictionary<Type, Type> GenericConstraintsSatisfactionInferredParameters = new Dictionary<Type, Type>();

		public override string Name => "Generic Parameter Inference ---> Type<T1 [, T2] : Match<T1> [where T1 : constraints [, T2]]";

		public override Type Match(Type[] targets, ref bool stopMatching)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (!targetIsGenericParameterCached[i] && info.Targets[i] != targets[i])
				{
					return null;
				}
			}
			lock (LOCK)
			{
				Type[] array;
				if (genericParameterTargetCount != targets.Length)
				{
					array = typeArrayOfGenericParameterTargetCountSize;
					int num = 0;
					for (int j = 0; j < info.Targets.Length; j++)
					{
						if (targetIsGenericParameterCached[j])
						{
							array[num++] = targets[j];
						}
					}
				}
				else
				{
					array = targets;
				}
				if (TryInferGenericParameters(info.MatchType, out var inferredParams, array))
				{
					return info.MatchType.GetGenericTypeDefinition().MakeGenericType(inferredParams);
				}
				return null;
			}
		}

		private bool TryInferGenericParameters(Type genericTypeDefinition, out Type[] inferredParams, params Type[] knownParameters)
		{
			if (genericTypeDefinition == null)
			{
				throw new ArgumentNullException("genericTypeDefinition");
			}
			if (knownParameters == null)
			{
				throw new ArgumentNullException("knownParameters");
			}
			if (!genericTypeDefinition.IsGenericType)
			{
				throw new ArgumentException("The genericTypeDefinition parameter must be a generic type.");
			}
			for (int i = 0; i < matchTypeGenericArgs.Length; i++)
			{
				matchTypeGenericArgs[i] = matchTypeGenericArgs_Backup[i];
			}
			for (int j = 0; j < matchTypeGenericDefinitionArgs.Length; j++)
			{
				matchTypeGenericDefinitionArgs[j] = matchTypeGenericDefinitionArgs_Backup[j];
			}
			Dictionary<Type, Type> genericConstraintsSatisfactionInferredParameters = GenericConstraintsSatisfactionInferredParameters;
			genericConstraintsSatisfactionInferredParameters.Clear();
			Type[] array = matchTypeGenericArgs;
			if (!genericTypeDefinition.IsGenericTypeDefinition)
			{
				Type[] array2 = array;
				genericTypeDefinition = genericTypeDefinition.GetGenericTypeDefinition();
				array = matchTypeGenericDefinitionArgs;
				int num = 0;
				for (int k = 0; k < array2.Length; k++)
				{
					if (!array2[k].IsGenericParameter && (!array2[k].IsGenericType || array2[k].IsFullyConstructedGenericType()))
					{
						genericConstraintsSatisfactionInferredParameters[array[k]] = array2[k];
					}
					else
					{
						num++;
					}
				}
				if (num == knownParameters.Length)
				{
					int num2 = 0;
					for (int l = 0; l < array2.Length; l++)
					{
						if (array2[l].IsGenericParameter)
						{
							array2[l] = knownParameters[num2++];
						}
					}
					if (TypeExtensions.AreGenericConstraintsSatisfiedBy(matchTypeGenericDefinitionArgs_Backup, array2))
					{
						inferredParams = array2;
						return true;
					}
				}
			}
			if (array.Length == knownParameters.Length && TypeExtensions.AreGenericConstraintsSatisfiedBy(matchTypeGenericDefinitionArgs_Backup, knownParameters))
			{
				inferredParams = knownParameters;
				return true;
			}
			Type[] array3 = array;
			foreach (Type type in array3)
			{
				if (genericConstraintsSatisfactionInferredParameters.ContainsKey(type))
				{
					continue;
				}
				Type[] genericParameterConstraintsCached = GetGenericParameterConstraintsCached(type);
				Type[] array4 = genericParameterConstraintsCached;
				foreach (Type type2 in array4)
				{
					if (!type2.IsGenericType)
					{
						continue;
					}
					Type genericTypeDefinition2 = type2.GetGenericTypeDefinition();
					Type[] genericArgumentsCached = GetGenericArgumentsCached(type2);
					foreach (Type type3 in knownParameters)
					{
						Type[] array5;
						if (type3.IsGenericType && genericTypeDefinition2 == type3.GetGenericTypeDefinition())
						{
							array5 = GetGenericArgumentsCached(type3);
						}
						else if (genericTypeDefinition2.IsInterface && type3.ImplementsOpenGenericInterface(genericTypeDefinition2))
						{
							array5 = type3.GetArgumentsOfInheritedOpenGenericInterface(genericTypeDefinition2);
						}
						else
						{
							if (!genericTypeDefinition2.IsClass || !type3.ImplementsOpenGenericClass(genericTypeDefinition2))
							{
								continue;
							}
							array5 = type3.GetArgumentsOfInheritedOpenGenericClass(genericTypeDefinition2);
						}
						genericConstraintsSatisfactionInferredParameters[type] = type3;
						for (int num4 = 0; num4 < genericArgumentsCached.Length; num4++)
						{
							if (genericArgumentsCached[num4].IsGenericParameter)
							{
								genericConstraintsSatisfactionInferredParameters[genericArgumentsCached[num4]] = array5[num4];
							}
						}
					}
				}
			}
			if (genericConstraintsSatisfactionInferredParameters.Count == array.Length)
			{
				inferredParams = new Type[genericConstraintsSatisfactionInferredParameters.Count];
				for (int num5 = 0; num5 < array.Length; num5++)
				{
					inferredParams[num5] = genericConstraintsSatisfactionInferredParameters[array[num5]];
				}
				if (TypeExtensions.AreGenericConstraintsSatisfiedBy(matchTypeGenericDefinitionArgs_Backup, inferredParams))
				{
					return true;
				}
			}
			inferredParams = null;
			return false;
		}

		private static Type[] GetGenericParameterConstraintsCached(Type type)
		{
			if (!GenericParameterConstraintsCache.TryGetValue(type, out var value))
			{
				value = type.GetGenericParameterConstraints();
				GenericParameterConstraintsCache.Add(type, value);
			}
			return value;
		}

		private static Type[] GetGenericArgumentsCached(Type type)
		{
			if (!GenericArgumentsCache.TryGetValue(type, out var value))
			{
				value = type.GetGenericArguments();
				GenericArgumentsCache.Add(type, value);
			}
			return value;
		}
	}
}
