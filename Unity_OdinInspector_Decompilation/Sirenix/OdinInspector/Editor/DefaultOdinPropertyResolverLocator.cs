using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Sirenix.OdinInspector.Editor.TypeSearch;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Default implementation and the version that will be used by <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree" /> if no other <see cref="T:Sirenix.OdinInspector.Editor.OdinPropertyResolver" /> instance have been specified.
	/// </summary>
	public class DefaultOdinPropertyResolverLocator : OdinPropertyResolverLocator
	{
		/// <summary>
		/// Singleton instance of <see cref="T:Sirenix.OdinInspector.Editor.DefaultOdinPropertyResolverLocator" />.
		/// </summary>
		public static readonly DefaultOdinPropertyResolverLocator Instance;

		public static readonly TypeSearchIndex SearchIndex;

		private static Dictionary<Type, OdinPropertyResolver> resolverEmptyInstanceMap;

		private static readonly List<TypeSearchResult[]> QueryResultsList;

		private static readonly List<TypeSearchResult> MergedSearchResultsList;

		static DefaultOdinPropertyResolverLocator()
		{
			Instance = new DefaultOdinPropertyResolverLocator();
			SearchIndex = new TypeSearchIndex();
			resolverEmptyInstanceMap = new Dictionary<Type, OdinPropertyResolver>(FastTypeComparer.Instance);
			QueryResultsList = new List<TypeSearchResult[]>();
			MergedSearchResultsList = new List<TypeSearchResult>();
			if (UnityTypeCacheUtility.IsAvailable)
			{
				using (SimpleProfiler.Section("DefaultOdinPropertyResolverLocator Type Cache"))
				{
					IList<Type> typesDerivedFrom = UnityTypeCacheUtility.GetTypesDerivedFrom(typeof(OdinPropertyResolver));
					foreach (Type item in typesDerivedFrom)
					{
						if (!item.IsAbstract && !item.IsDefined<OdinDontRegisterAttribute>())
						{
							IndexType(item);
						}
					}
				}
				return;
			}
			using (SimpleProfiler.Section("DefaultOdinPropertyResolverLocator Reflection"))
			{
				List<Assembly> resolverAssemblies = ResolverUtilities.GetResolverAssemblies();
				for (int i = 0; i < resolverAssemblies.Count; i++)
				{
					Type[] array = resolverAssemblies[i].SafeGetTypes();
					foreach (Type type in array)
					{
						if (!type.IsAbstract && typeof(OdinPropertyResolver).IsAssignableFrom(type) && !type.IsDefined<OdinDontRegisterAttribute>(inherit: false))
						{
							IndexType(type);
						}
					}
				}
			}
		}

		private static void IndexType(Type type)
		{
			TypeSearchInfo typeSearchInfo = default(TypeSearchInfo);
			typeSearchInfo.MatchType = type;
			typeSearchInfo.Priority = ResolverUtilities.GetResolverPriority(type);
			TypeSearchInfo typeToIndex = typeSearchInfo;
			if (type.ImplementsOpenGenericType(typeof(OdinPropertyResolver<>)))
			{
				if (type.ImplementsOpenGenericType(typeof(OdinPropertyResolver<, >)))
				{
					typeToIndex.Targets = type.GetArgumentsOfInheritedOpenGenericType(typeof(OdinPropertyResolver<, >));
				}
				else
				{
					typeToIndex.Targets = type.GetArgumentsOfInheritedOpenGenericType(typeof(OdinPropertyResolver<>));
				}
			}
			else
			{
				typeToIndex.Targets = Type.EmptyTypes;
			}
			SearchIndex.AddIndexedType(typeToIndex);
		}

		/// <summary>
		/// Gets an <see cref="T:Sirenix.OdinInspector.Editor.OdinPropertyResolver" /> instance for the specified property.
		/// </summary>
		/// <param name="property">The property to get an <see cref="T:Sirenix.OdinInspector.Editor.OdinPropertyResolver" /> instance for.</param>
		/// <returns>An instance of <see cref="T:Sirenix.OdinInspector.Editor.OdinPropertyResolver" /> to resolver the specified property.</returns>
		public override OdinPropertyResolver GetResolver(InspectorProperty property)
		{
			if (property.Tree.IsStatic && property == property.Tree.RootProperty)
			{
				return OdinPropertyResolver.Create(typeof(StaticRootPropertyResolver<>).MakeGenericType(property.ValueEntry.TypeOfValue), property);
			}
			List<TypeSearchResult[]> queryResultsList = QueryResultsList;
			queryResultsList.Clear();
			queryResultsList.Add(SearchIndex.GetMatches(Type.EmptyTypes));
			Type type = ((property.ValueEntry != null) ? property.ValueEntry.TypeOfValue : null);
			if (type != null)
			{
				queryResultsList.Add(SearchIndex.GetMatches(type));
				for (int i = 0; i < property.Attributes.Count; i++)
				{
					queryResultsList.Add(SearchIndex.GetMatches(type, property.Attributes[i].GetType()));
				}
			}
			TypeSearchIndex.MergeQueryResultsIntoList(queryResultsList, MergedSearchResultsList);
			for (int j = 0; j < MergedSearchResultsList.Count; j++)
			{
				TypeSearchResult typeSearchResult = MergedSearchResultsList[j];
				if (GetEmptyResolverInstance(typeSearchResult.MatchedType).CanResolveForPropertyFilter(property))
				{
					return OdinPropertyResolver.Create(typeSearchResult.MatchedType, property);
				}
			}
			return OdinPropertyResolver.Create<EmptyPropertyResolver>(property);
		}

		public OdinPropertyResolver GetEmptyResolverInstance(Type resolverType)
		{
			if (!resolverEmptyInstanceMap.TryGetValue(resolverType, out var value))
			{
				value = (OdinPropertyResolver)FormatterServices.GetUninitializedObject(resolverType);
				resolverEmptyInstanceMap[resolverType] = value;
			}
			return value;
		}
	}
}
