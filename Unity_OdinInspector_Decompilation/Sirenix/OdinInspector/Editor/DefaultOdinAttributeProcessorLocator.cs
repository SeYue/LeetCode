using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector.Editor.TypeSearch;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Default implementation and the version that will be used when no other OdinAttributeProcessorLocator instance have been given to a PropertyTree.
	/// This implementation will find all AttributeProcessor definitions not marked with the <see cref="T:Sirenix.OdinInspector.Editor.OdinDontRegisterAttribute" />.
	/// </summary>
	public sealed class DefaultOdinAttributeProcessorLocator : OdinAttributeProcessorLocator
	{
		/// <summary>
		/// Singleton instance of the DefaultOdinAttributeProcessorLocator class.
		/// </summary>
		public static readonly DefaultOdinAttributeProcessorLocator Instance;

		/// <summary>
		/// Type search index used for matching <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeProcessor" /> to properties.
		/// </summary>
		public static readonly TypeSearchIndex SearchIndex;

		private static Dictionary<Type, OdinAttributeProcessor> ResolverInstanceMap;

		private List<TypeSearchResult[]> CachedMatchesList = new List<TypeSearchResult[]>();

		static DefaultOdinAttributeProcessorLocator()
		{
			Instance = new DefaultOdinAttributeProcessorLocator();
			SearchIndex = new TypeSearchIndex();
			ResolverInstanceMap = new Dictionary<Type, OdinAttributeProcessor>(FastTypeComparer.Instance);
			TypeSearchInfo typeToIndex;
			if (UnityTypeCacheUtility.IsAvailable)
			{
				using (SimpleProfiler.Section("DefaultOdinAttributeProcessorLocator - TypeCache"))
				{
					IList<Type> typesDerivedFrom = UnityTypeCacheUtility.GetTypesDerivedFrom(typeof(OdinAttributeProcessor));
					foreach (Type item in typesDerivedFrom)
					{
						if (!item.IsAbstract && !item.IsDefined<OdinDontRegisterAttribute>())
						{
							TypeSearchIndex searchIndex = SearchIndex;
							typeToIndex = new TypeSearchInfo
							{
								MatchType = item,
								Priority = ResolverUtilities.GetResolverPriority(item),
								Targets = ((!item.ImplementsOpenGenericClass(typeof(OdinAttributeProcessor<>))) ? Type.EmptyTypes : new Type[1] { item.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinAttributeProcessor<>))[0] })
							};
							searchIndex.AddIndexedType(typeToIndex);
						}
					}
				}
				return;
			}
			using (SimpleProfiler.Section("DefaultOdinAttributeProcessorLocator - Reflection"))
			{
				List<Assembly> resolverAssemblies = ResolverUtilities.GetResolverAssemblies();
				for (int i = 0; i < resolverAssemblies.Count; i++)
				{
					Type[] array = resolverAssemblies[i].SafeGetTypes();
					foreach (Type type in array)
					{
						if (!type.IsAbstract && typeof(OdinAttributeProcessor).IsAssignableFrom(type) && !type.IsDefined<OdinDontRegisterAttribute>())
						{
							TypeSearchIndex searchIndex2 = SearchIndex;
							typeToIndex = new TypeSearchInfo
							{
								MatchType = type,
								Priority = ResolverUtilities.GetResolverPriority(type),
								Targets = ((!type.ImplementsOpenGenericClass(typeof(OdinAttributeProcessor<>))) ? Type.EmptyTypes : new Type[1] { type.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinAttributeProcessor<>))[0] })
							};
							searchIndex2.AddIndexedType(typeToIndex);
						}
					}
				}
			}
		}

		private static void IndexType(Type type)
		{
		}

		/// <summary>
		/// Gets a list of <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeProcessor" /> to process attributes for the specified child member of the parent property.
		/// </summary>
		/// <param name="parentProperty">The parent of the member.</param>
		/// <param name="member">Child member of the parent property.</param>
		/// <returns>List of <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeProcessor" /> to process attributes for the specified member.</returns>
		public override List<OdinAttributeProcessor> GetChildProcessors(InspectorProperty parentProperty, MemberInfo member)
		{
			CachedMatchesList.Clear();
			CachedMatchesList.Add(SearchIndex.GetMatches(Type.EmptyTypes));
			if (parentProperty.ValueEntry != null)
			{
				CachedMatchesList.Add(SearchIndex.GetMatches(parentProperty.ValueEntry.TypeOfValue));
			}
			TypeSearchResult[] cachedMergedQueryResults = TypeSearchIndex.GetCachedMergedQueryResults(CachedMatchesList);
			List<OdinAttributeProcessor> list = new List<OdinAttributeProcessor>(cachedMergedQueryResults.Length);
			for (int i = 0; i < cachedMergedQueryResults.Length; i++)
			{
				TypeSearchResult typeSearchResult = cachedMergedQueryResults[i];
				OdinAttributeProcessor resolverInstance = GetResolverInstance(typeSearchResult.MatchedType);
				if (resolverInstance.CanProcessChildMemberAttributes(parentProperty, member))
				{
					list.Add(resolverInstance);
				}
			}
			return list;
		}

		/// <summary>
		/// Gets a list of <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeProcessor" /> to process attributes for the specified property.
		/// </summary>
		/// <param name="property">The property to find attribute porcessors for.</param>
		/// <returns>List of <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeProcessor" /> to process attributes for the speicied member.</returns>
		public override List<OdinAttributeProcessor> GetSelfProcessors(InspectorProperty property)
		{
			CachedMatchesList.Clear();
			CachedMatchesList.Add(SearchIndex.GetMatches(Type.EmptyTypes));
			if (property.ValueEntry != null)
			{
				CachedMatchesList.Add(SearchIndex.GetMatches(property.ValueEntry.TypeOfValue));
			}
			TypeSearchResult[] cachedMergedQueryResults = TypeSearchIndex.GetCachedMergedQueryResults(CachedMatchesList);
			List<OdinAttributeProcessor> list = new List<OdinAttributeProcessor>(cachedMergedQueryResults.Length);
			for (int i = 0; i < cachedMergedQueryResults.Length; i++)
			{
				TypeSearchResult typeSearchResult = cachedMergedQueryResults[i];
				OdinAttributeProcessor resolverInstance = GetResolverInstance(typeSearchResult.MatchedType);
				if (resolverInstance.CanProcessSelfAttributes(property))
				{
					list.Add(resolverInstance);
				}
			}
			return list;
		}

		private static OdinAttributeProcessor GetResolverInstance(Type type)
		{
			if (!ResolverInstanceMap.TryGetValue(type, out var value))
			{
				value = (OdinAttributeProcessor)Activator.CreateInstance(type);
				ResolverInstanceMap.Add(type, value);
			}
			return value;
		}
	}
}
