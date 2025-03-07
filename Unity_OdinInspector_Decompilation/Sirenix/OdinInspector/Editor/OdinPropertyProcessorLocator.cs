using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Sirenix.OdinInspector.Editor.TypeSearch;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	public static class OdinPropertyProcessorLocator
	{
		private static readonly Dictionary<Type, OdinPropertyProcessor> EmptyInstances;

		public static readonly TypeSearchIndex SearchIndex;

		private static readonly List<TypeSearchResult[]> CachedQueryList;

		static OdinPropertyProcessorLocator()
		{
			EmptyInstances = new Dictionary<Type, OdinPropertyProcessor>(FastTypeComparer.Instance);
			SearchIndex = new TypeSearchIndex
			{
				MatchedTypeLogName = "member property processor"
			};
			CachedQueryList = new List<TypeSearchResult[]>();
			if (UnityTypeCacheUtility.IsAvailable)
			{
				using (SimpleProfiler.Section("OdinPropertyProcessorLocator Type Cache"))
				{
					IList<Type> typesDerivedFrom = UnityTypeCacheUtility.GetTypesDerivedFrom(typeof(OdinPropertyProcessor));
					foreach (Type item in typesDerivedFrom)
					{
						if (!item.IsAbstract && !item.IsDefined<OdinDontRegisterAttribute>(inherit: false))
						{
							IndexType(item);
						}
					}
				}
				return;
			}
			using (SimpleProfiler.Section("OdinPropertyProcessorLocator Reflection"))
			{
				List<Assembly> resolverAssemblies = ResolverUtilities.GetResolverAssemblies();
				for (int i = 0; i < resolverAssemblies.Count; i++)
				{
					Type[] array = resolverAssemblies[i].SafeGetTypes();
					foreach (Type type in array)
					{
						if (!type.IsAbstract && typeof(OdinPropertyProcessor).IsAssignableFrom(type) && !type.IsDefined<OdinDontRegisterAttribute>(inherit: false))
						{
							IndexType(type);
						}
					}
				}
			}
		}

		private static void IndexType(Type type)
		{
			TypeSearchInfo typeToIndex;
			if (type.ImplementsOpenGenericClass(typeof(OdinPropertyProcessor<>)))
			{
				if (type.ImplementsOpenGenericClass(typeof(OdinPropertyProcessor<, >)))
				{
					TypeSearchIndex searchIndex = SearchIndex;
					typeToIndex = new TypeSearchInfo
					{
						MatchType = type,
						Targets = type.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinPropertyProcessor<, >)),
						Priority = ResolverUtilities.GetResolverPriority(type)
					};
					searchIndex.AddIndexedType(typeToIndex);
				}
				else
				{
					TypeSearchIndex searchIndex2 = SearchIndex;
					typeToIndex = new TypeSearchInfo
					{
						MatchType = type,
						Targets = type.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinPropertyProcessor<>)),
						Priority = ResolverUtilities.GetResolverPriority(type)
					};
					searchIndex2.AddIndexedType(typeToIndex);
				}
			}
			else
			{
				TypeSearchIndex searchIndex3 = SearchIndex;
				typeToIndex = new TypeSearchInfo
				{
					MatchType = type,
					Targets = Type.EmptyTypes,
					Priority = ResolverUtilities.GetResolverPriority(type)
				};
				searchIndex3.AddIndexedType(typeToIndex);
			}
		}

		public static List<OdinPropertyProcessor> GetMemberProcessors(InspectorProperty property)
		{
			List<TypeSearchResult[]> cachedQueryList = CachedQueryList;
			cachedQueryList.Clear();
			cachedQueryList.Add(SearchIndex.GetMatches(Type.EmptyTypes));
			if (property.ValueEntry != null)
			{
				Type typeOfValue = property.ValueEntry.TypeOfValue;
				cachedQueryList.Add(SearchIndex.GetMatches(typeOfValue));
				for (int i = 0; i < property.Attributes.Count; i++)
				{
					cachedQueryList.Add(SearchIndex.GetMatches(typeOfValue, property.Attributes[i].GetType()));
				}
			}
			TypeSearchResult[] cachedMergedQueryResults = TypeSearchIndex.GetCachedMergedQueryResults(cachedQueryList);
			List<OdinPropertyProcessor> list = new List<OdinPropertyProcessor>();
			for (int j = 0; j < cachedMergedQueryResults.Length; j++)
			{
				TypeSearchResult typeSearchResult = cachedMergedQueryResults[j];
				if (GetEmptyInstance(typeSearchResult.MatchedType).CanProcessForProperty(property))
				{
					list.Add(OdinPropertyProcessor.Create(typeSearchResult.MatchedType, property));
				}
			}
			return list;
		}

		private static OdinPropertyProcessor GetEmptyInstance(Type type)
		{
			if (!EmptyInstances.TryGetValue(type, out var value))
			{
				value = (OdinPropertyProcessor)FormatterServices.GetUninitializedObject(type);
				EmptyInstances[type] = value;
			}
			return value;
		}
	}
}
