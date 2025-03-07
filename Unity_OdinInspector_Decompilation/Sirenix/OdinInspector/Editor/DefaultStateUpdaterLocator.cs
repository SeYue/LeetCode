using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using Sirenix.OdinInspector.Editor.TypeSearch;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	public class DefaultStateUpdaterLocator : StateUpdaterLocator
	{
		public static readonly DefaultStateUpdaterLocator Instance;

		public static readonly TypeSearchIndex SearchIndex;

		private static readonly Dictionary<Type, Func<StateUpdater>> FastCreators;

		private static readonly Dictionary<Type, StateUpdater> EmptyInstances;

		private static readonly StateUpdater[] EmptyResult;

		private static TypeSearchResult[][] CachedQueryResultArray;

		private static StateUpdater[] CachedResultBuilderArray;

		static DefaultStateUpdaterLocator()
		{
			Instance = new DefaultStateUpdaterLocator();
			SearchIndex = new TypeSearchIndex
			{
				MatchedTypeLogName = "state updater"
			};
			FastCreators = new Dictionary<Type, Func<StateUpdater>>(FastTypeComparer.Instance);
			EmptyInstances = new Dictionary<Type, StateUpdater>(FastTypeComparer.Instance);
			EmptyResult = new StateUpdater[0];
			CachedQueryResultArray = new TypeSearchResult[32][];
			CachedResultBuilderArray = new StateUpdater[16];
			List<Assembly> resolverAssemblies = ResolverUtilities.GetResolverAssemblies();
			for (int i = 0; i < resolverAssemblies.Count; i++)
			{
				object[] array;
				try
				{
					array = resolverAssemblies[i].SafeGetCustomAttributes(typeof(RegisterStateUpdaterAttribute), inherit: false);
				}
				catch
				{
					continue;
				}
				for (int j = 0; j < array.Length; j++)
				{
					RegisterStateUpdaterAttribute registerStateUpdaterAttribute = (RegisterStateUpdaterAttribute)array[j];
					if (!registerStateUpdaterAttribute.Type.IsAbstract && typeof(StateUpdater).IsAssignableFrom(registerStateUpdaterAttribute.Type))
					{
						IndexType(registerStateUpdaterAttribute.Type, registerStateUpdaterAttribute.Priority);
					}
				}
			}
		}

		private static void IndexType(Type type, double priority)
		{
			TypeSearchInfo typeSearchInfo = default(TypeSearchInfo);
			typeSearchInfo.MatchType = type;
			typeSearchInfo.Priority = priority;
			TypeSearchInfo typeToIndex = typeSearchInfo;
			if (type.ImplementsOpenGenericType(typeof(AttributeStateUpdater<>)))
			{
				if (type.ImplementsOpenGenericType(typeof(AttributeStateUpdater<, >)))
				{
					typeToIndex.Targets = type.GetArgumentsOfInheritedOpenGenericType(typeof(AttributeStateUpdater<, >));
				}
				else
				{
					typeToIndex.Targets = type.GetArgumentsOfInheritedOpenGenericType(typeof(AttributeStateUpdater<>));
				}
			}
			else if (type.ImplementsOpenGenericType(typeof(ValueStateUpdater<>)))
			{
				typeToIndex.Targets = type.GetArgumentsOfInheritedOpenGenericType(typeof(ValueStateUpdater<>));
			}
			else
			{
				typeToIndex.Targets = Type.EmptyTypes;
			}
			SearchIndex.AddIndexedType(typeToIndex);
		}

		public override StateUpdater[] GetStateUpdaters(InspectorProperty property)
		{
			int resultsCount = 0;
			CachedQueryResultArray[resultsCount++] = SearchIndex.GetMatches(Type.EmptyTypes);
			IPropertyValueEntry valueEntry = property.ValueEntry;
			if (valueEntry != null)
			{
				CachedQueryResultArray[resultsCount++] = SearchIndex.GetMatches(valueEntry.TypeOfValue);
			}
			int num = 2 + property.Attributes.Count * 2;
			while (CachedQueryResultArray.Length <= num)
			{
				ExpandArray(ref CachedQueryResultArray);
			}
			for (int i = 0; i < property.Attributes.Count; i++)
			{
				Type type = property.Attributes[i].GetType();
				CachedQueryResultArray[resultsCount++] = SearchIndex.GetMatches(type);
				if (valueEntry != null)
				{
					CachedQueryResultArray[resultsCount++] = SearchIndex.GetMatches(type, valueEntry.TypeOfValue);
				}
			}
			TypeSearchResult[] cachedMergedQueryResults = TypeSearchIndex.GetCachedMergedQueryResults(CachedQueryResultArray, resultsCount);
			int num2 = 0;
			while (CachedResultBuilderArray.Length < cachedMergedQueryResults.Length)
			{
				ExpandArray(ref CachedResultBuilderArray);
			}
			for (int j = 0; j < cachedMergedQueryResults.Length; j++)
			{
				TypeSearchResult typeSearchResult = cachedMergedQueryResults[j];
				if (GetEmptyUpdaterInstance(typeSearchResult.MatchedType).CanUpdateProperty(property))
				{
					CachedResultBuilderArray[num2++] = CreateStateUpdater(typeSearchResult.MatchedType);
				}
			}
			if (num2 == 0)
			{
				return EmptyResult;
			}
			StateUpdater[] array = new StateUpdater[num2];
			for (int k = 0; k < num2; k++)
			{
				array[k] = CachedResultBuilderArray[k];
				CachedResultBuilderArray[k] = null;
			}
			return array;
		}

		public StateUpdater GetEmptyUpdaterInstance(Type type)
		{
			if (!EmptyInstances.TryGetValue(type, out var value))
			{
				value = (StateUpdater)FormatterServices.GetUninitializedObject(type);
				EmptyInstances[type] = value;
			}
			return value;
		}

		private static void ExpandArray<T>(ref T[] array)
		{
			T[] array2 = new T[array.Length * 2];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = array[i];
			}
			array = array2;
		}

		private static StateUpdater CreateStateUpdater(Type type)
		{
			if (!FastCreators.TryGetValue(type, out var value))
			{
				ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
				DynamicMethod dynamicMethod = new DynamicMethod("FastCreator", typeof(StateUpdater), Type.EmptyTypes);
				ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
				iLGenerator.Emit(OpCodes.Newobj, constructor);
				iLGenerator.Emit(OpCodes.Ret);
				value = (Func<StateUpdater>)dynamicMethod.CreateDelegate(typeof(Func<StateUpdater>));
				FastCreators.Add(type, value);
			}
			return value();
		}
	}
}
