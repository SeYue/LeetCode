using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
	public sealed class TypeSearchIndex
	{
		private class ProcessedTypeSearchInfo
		{
			public TypeSearchInfo Info;

			public List<TypeMatcher> Matchers;
		}

		private class TypeArrayEqualityComparer : IEqualityComparer<Type[]>
		{
			public bool Equals(Type[] x, Type[] y)
			{
				if (x == y)
				{
					return true;
				}
				if (x == null || y == null)
				{
					return false;
				}
				if (x.Length != y.Length)
				{
					return false;
				}
				for (int i = 0; i < x.Length; i++)
				{
					if (x[i] != y[i])
					{
						return false;
					}
				}
				return true;
			}

			public int GetHashCode(Type[] obj)
			{
				if (obj == null)
				{
					return 0;
				}
				int num = 1;
				for (int i = 0; i < obj.Length; i++)
				{
					int num2 = obj[i]?.GetHashCode() ?? 1;
					num = 137 * num + (num2 ^ (num2 >> 16));
				}
				return num;
			}
		}

		private struct QueryResult
		{
			public int CurrentIndex;

			public double CurrentPriority;

			public TypeSearchResult[] Result;

			public QueryResult(TypeSearchResult[] result)
			{
				Result = result;
				CurrentIndex = 0;
				CurrentPriority = result[0].MatchedInfo.Priority;
			}
		}

		private class MergeSignatureComparer : IEqualityComparer<MergeSignature>
		{
			public bool Equals(MergeSignature x, MergeSignature y)
			{
				if (x.Hash != y.Hash)
				{
					return false;
				}
				if (x.Results == y.Results)
				{
					return true;
				}
				int resultCount = x.ResultCount;
				if (resultCount != y.ResultCount)
				{
					return false;
				}
				for (int i = 0; i < resultCount; i++)
				{
					if (x.Results[i] != y.Results[i])
					{
						return false;
					}
				}
				return true;
			}

			public int GetHashCode(MergeSignature obj)
			{
				return obj.Hash;
			}
		}

		private struct MergeSignature
		{
			public int Hash;

			public IList<TypeSearchResult[]> Results;

			public int ResultCount;

			public MergeSignature(IList<TypeSearchResult[]> results, int count)
			{
				Results = results;
				ResultCount = count;
				int num = 1;
				for (int i = 0; i < count; i++)
				{
					int hashCode = results[i].GetHashCode();
					num = 137 * num + (hashCode ^ (hashCode >> 16));
				}
				Hash = num;
			}

			public override int GetHashCode()
			{
				return Hash;
			}
		}

		public string MatchedTypeLogName = "matched type";

		public List<TypeMatchIndexingRule> IndexingRules = new List<TypeMatchIndexingRule>();

		public List<TypeMatchRule> MatchRules = new List<TypeMatchRule>();

		public Action<string, TypeSearchInfo> LogInvalidTypeInfo = delegate(string message, TypeSearchInfo info)
		{
			Debug.LogError((object)message);
		};

		private readonly List<ProcessedTypeSearchInfo> indexedTypes = new List<ProcessedTypeSearchInfo>();

		private readonly Type[] CachedTargetArray1 = new Type[1];

		private readonly Type[] CachedTargetArray2 = new Type[2];

		/// <summary>
		/// To safely change anything in the type cache, you must be holding this lock.
		/// </summary>
		public readonly object LOCK = new object();

		public List<TypeMatcherCreator> TypeMatcherCreators = new List<TypeMatcherCreator>();

		private Dictionary<Type[], TypeSearchResult[]> resultCache = new Dictionary<Type[], TypeSearchResult[]>(new TypeArrayEqualityComparer());

		private static readonly List<QueryResult> CachedQueryResultList = new List<QueryResult>();

		private static readonly object STATIC_LOCK = new object();

		private static readonly Dictionary<MergeSignature, TypeSearchResult[]> KnownMergeSignatures = new Dictionary<MergeSignature, TypeSearchResult[]>(new MergeSignatureComparer());

		private static readonly List<TypeSearchResult> CachedFastMergeList = new List<TypeSearchResult>();

		private static readonly TypeSearchResult[] EmptyResultArray = new TypeSearchResult[0];

		public TypeSearchIndex(bool addDefaultValidationRules = true, bool addDefaultMatchRules = true)
		{
			if (addDefaultValidationRules)
			{
				AddDefaultIndexingRules();
			}
			if (addDefaultMatchRules)
			{
				AddDefaultMatchRules();
				AddDefaultMatchCreators();
			}
		}

		public static List<List<Type[]>> GetAllCachedMergeSignatures(TypeSearchIndex index)
		{
			lock (STATIC_LOCK)
			{
				List<List<Type[]>> list = new List<List<Type[]>>();
				foreach (MergeSignature key in KnownMergeSignatures.Keys)
				{
					if (!IsMergeSignatureForIndex(key, index))
					{
						continue;
					}
					List<Type[]> list2 = new List<Type[]>();
					for (int i = 0; i < key.ResultCount; i++)
					{
						TypeSearchResult[] array = key.Results[i];
						if (array.Length == 0)
						{
							list2.Add(Type.EmptyTypes);
						}
						else
						{
							list2.Add(array[0].MatchedTargets);
						}
					}
					list.Add(list2);
				}
				return list;
			}
		}

		private static bool IsMergeSignatureForIndex(MergeSignature signature, TypeSearchIndex index)
		{
			for (int i = 0; i < signature.ResultCount; i++)
			{
				TypeSearchResult[] array = signature.Results[i];
				if (array.Length != 0)
				{
					return array[0].MatchedIndex == index;
				}
			}
			return false;
		}

		public static TypeSearchResult[] GetCachedMergedQueryResults(TypeSearchResult[][] results, int resultsCount)
		{
			switch (resultsCount)
			{
			case 0:
				return EmptyResultArray;
			case 1:
				return results[0];
			default:
				lock (STATIC_LOCK)
				{
					MergeSignature key = new MergeSignature(results, resultsCount);
					if (KnownMergeSignatures.TryGetValue(key, out var value))
					{
						return value;
					}
					List<TypeSearchResult> cachedFastMergeList = CachedFastMergeList;
					cachedFastMergeList.Clear();
					List<QueryResult> cachedQueryResultList = CachedQueryResultList;
					cachedQueryResultList.Clear();
					for (int i = 0; i < resultsCount; i++)
					{
						if (results[i].Length != 0)
						{
							cachedQueryResultList.Add(new QueryResult(results[i]));
						}
					}
					int count = cachedQueryResultList.Count;
					while (true)
					{
						double num = double.MinValue;
						int num2 = -1;
						for (int j = 0; j < count; j++)
						{
							QueryResult queryResult = cachedQueryResultList[j];
							if (queryResult.CurrentIndex < queryResult.Result.Length && queryResult.CurrentPriority > num)
							{
								num = queryResult.CurrentPriority;
								num2 = j;
							}
						}
						if (num2 == -1)
						{
							break;
						}
						QueryResult value2 = cachedQueryResultList[num2];
						cachedFastMergeList.Add(value2.Result[value2.CurrentIndex]);
						value2.CurrentIndex++;
						if (value2.CurrentIndex < value2.Result.Length)
						{
							value2.CurrentPriority = value2.Result[value2.CurrentIndex].MatchedInfo.Priority;
						}
						cachedQueryResultList[num2] = value2;
					}
					key.Results = new List<TypeSearchResult[]>(key.Results);
					TypeSearchResult[] array = cachedFastMergeList.ToArray();
					KnownMergeSignatures.Add(key, array);
					return array;
				}
			}
		}

		public static TypeSearchResult[] GetCachedMergedQueryResults(List<TypeSearchResult[]> results)
		{
			if (results.Count == 0)
			{
				return EmptyResultArray;
			}
			if (results.Count == 1)
			{
				return results[0];
			}
			lock (STATIC_LOCK)
			{
				MergeSignature key = new MergeSignature(results, results.Count);
				if (KnownMergeSignatures.TryGetValue(key, out var value))
				{
					return value;
				}
				List<TypeSearchResult> cachedFastMergeList = CachedFastMergeList;
				cachedFastMergeList.Clear();
				List<QueryResult> cachedQueryResultList = CachedQueryResultList;
				cachedQueryResultList.Clear();
				for (int i = 0; i < results.Count; i++)
				{
					if (results[i].Length != 0)
					{
						cachedQueryResultList.Add(new QueryResult(results[i]));
					}
				}
				int count = cachedQueryResultList.Count;
				while (true)
				{
					double num = double.MinValue;
					int num2 = -1;
					for (int j = 0; j < count; j++)
					{
						QueryResult queryResult = cachedQueryResultList[j];
						if (queryResult.CurrentIndex < queryResult.Result.Length && queryResult.CurrentPriority > num)
						{
							num = queryResult.CurrentPriority;
							num2 = j;
						}
					}
					if (num2 == -1)
					{
						break;
					}
					QueryResult value2 = cachedQueryResultList[num2];
					cachedFastMergeList.Add(value2.Result[value2.CurrentIndex]);
					value2.CurrentIndex++;
					if (value2.CurrentIndex < value2.Result.Length)
					{
						value2.CurrentPriority = value2.Result[value2.CurrentIndex].MatchedInfo.Priority;
					}
					cachedQueryResultList[num2] = value2;
				}
				key.Results = new List<TypeSearchResult[]>(key.Results);
				TypeSearchResult[] array = cachedFastMergeList.ToArray();
				KnownMergeSignatures.Add(key, array);
				return array;
			}
		}

		public static void MergeQueryResultsIntoList(List<TypeSearchResult[]> results, List<TypeSearchResult> mergeIntoList)
		{
			mergeIntoList.Clear();
			if (results.Count == 0)
			{
				return;
			}
			if (results.Count == 1)
			{
				TypeSearchResult[] array = results[0];
				for (int i = 0; i < array.Length; i++)
				{
					mergeIntoList.Add(array[i]);
				}
				return;
			}
			lock (STATIC_LOCK)
			{
				MergeSignature key = new MergeSignature(results, results.Count);
				if (KnownMergeSignatures.TryGetValue(key, out var value))
				{
					for (int j = 0; j < value.Length; j++)
					{
						mergeIntoList.Add(value[j]);
					}
					return;
				}
				List<QueryResult> cachedQueryResultList = CachedQueryResultList;
				cachedQueryResultList.Clear();
				for (int k = 0; k < results.Count; k++)
				{
					if (results[k].Length != 0)
					{
						cachedQueryResultList.Add(new QueryResult(results[k]));
					}
				}
				int count = cachedQueryResultList.Count;
				while (true)
				{
					double num = double.MinValue;
					int num2 = -1;
					for (int l = 0; l < count; l++)
					{
						QueryResult queryResult = cachedQueryResultList[l];
						if (queryResult.CurrentIndex < queryResult.Result.Length && queryResult.CurrentPriority > num)
						{
							num = queryResult.CurrentPriority;
							num2 = l;
						}
					}
					if (num2 == -1)
					{
						break;
					}
					QueryResult value2 = cachedQueryResultList[num2];
					mergeIntoList.Add(value2.Result[value2.CurrentIndex]);
					value2.CurrentIndex++;
					if (value2.CurrentIndex < value2.Result.Length)
					{
						value2.CurrentPriority = value2.Result[value2.CurrentIndex].MatchedInfo.Priority;
					}
					cachedQueryResultList[num2] = value2;
				}
				key.Results = new List<TypeSearchResult[]>(key.Results);
				KnownMergeSignatures.Add(key, mergeIntoList.ToArray());
			}
		}

		public List<Type[]> GetAllCachedTargets()
		{
			List<Type[]> list = new List<Type[]>();
			lock (LOCK)
			{
				foreach (Type[] key in resultCache.Keys)
				{
					list.Add(key);
				}
				return list;
			}
		}

		public void AddIndexedType(TypeSearchInfo typeToIndex)
		{
			lock (LOCK)
			{
				ProcessedTypeSearchInfo processedTypeSearchInfo = ProcessInfo(typeToIndex);
				if (processedTypeSearchInfo != null)
				{
					InsertIndexedTypeSorted(indexedTypes, processedTypeSearchInfo);
				}
				if (resultCache.Count > 0)
				{
					resultCache.Clear();
				}
			}
		}

		public void AddIndexedTypeUnsorted(TypeSearchInfo typeToIndex)
		{
			lock (LOCK)
			{
				ProcessedTypeSearchInfo processedTypeSearchInfo = ProcessInfo(typeToIndex);
				if (processedTypeSearchInfo != null)
				{
					indexedTypes.Add(processedTypeSearchInfo);
				}
				if (resultCache.Count > 0)
				{
					resultCache.Clear();
				}
			}
		}

		public void AddIndexedTypes(List<TypeSearchInfo> typesToIndex)
		{
			lock (LOCK)
			{
				for (int i = 0; i < typesToIndex.Count; i++)
				{
					ProcessedTypeSearchInfo processedTypeSearchInfo = ProcessInfo(typesToIndex[i]);
					if (processedTypeSearchInfo != null)
					{
						InsertIndexedTypeSorted(indexedTypes, processedTypeSearchInfo);
					}
				}
				if (resultCache.Count > 0)
				{
					resultCache.Clear();
				}
			}
		}

		public void ClearResultCache()
		{
			lock (LOCK)
			{
				resultCache.Clear();
			}
		}

		public TypeSearchResult[] GetMatches(Type target)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			lock (LOCK)
			{
				Type[] cachedTargetArray = CachedTargetArray1;
				cachedTargetArray[0] = target;
				if (!resultCache.TryGetValue(cachedTargetArray, out var value))
				{
					Type[] array = new Type[1] { target };
					value = FindAllMatches(array);
					resultCache[array] = value;
					return value;
				}
				return value;
			}
		}

		public TypeSearchResult[] GetMatches(Type target1, Type target2)
		{
			if (target1 == null)
			{
				throw new ArgumentNullException("target1");
			}
			if (target2 == null)
			{
				throw new ArgumentNullException("target2");
			}
			lock (LOCK)
			{
				Type[] cachedTargetArray = CachedTargetArray2;
				cachedTargetArray[0] = target1;
				cachedTargetArray[1] = target2;
				if (!resultCache.TryGetValue(cachedTargetArray, out var value))
				{
					Type[] array = new Type[2] { target1, target2 };
					value = FindAllMatches(array);
					resultCache[array] = value;
					return value;
				}
				return value;
			}
		}

		public TypeSearchResult[] GetMatches(params Type[] targets)
		{
			if (targets == null)
			{
				throw new ArgumentNullException("targets");
			}
			lock (LOCK)
			{
				if (!resultCache.TryGetValue(targets, out var value))
				{
					value = FindAllMatches(targets);
					resultCache[targets] = value;
					return value;
				}
				return value;
			}
		}

		private TypeSearchResult[] FindAllMatches(Type[] targets)
		{
			List<TypeSearchResult> list = new List<TypeSearchResult>();
			for (int i = 0; i < indexedTypes.Count; i++)
			{
				ProcessedTypeSearchInfo processedTypeSearchInfo = indexedTypes[i];
				if (targets.Length != processedTypeSearchInfo.Info.Targets.Length)
				{
					continue;
				}
				TypeSearchResult item;
				for (int j = 0; j < processedTypeSearchInfo.Matchers.Count; j++)
				{
					bool stopMatching = false;
					TypeMatcher typeMatcher = processedTypeSearchInfo.Matchers[j];
					Type type = typeMatcher.Match(targets, ref stopMatching);
					if (type != null)
					{
						item = new TypeSearchResult
						{
							MatchedInfo = processedTypeSearchInfo.Info,
							MatchedMatcher = typeMatcher,
							MatchedType = type,
							MatchedTargets = targets,
							MatchedIndex = this
						};
						list.Add(item);
						break;
					}
					if (stopMatching)
					{
						break;
					}
				}
				for (int k = 0; k < MatchRules.Count; k++)
				{
					TypeMatchRule typeMatchRule = MatchRules[k];
					bool stopMatchingForInfo = false;
					Type type2 = typeMatchRule.Match(processedTypeSearchInfo.Info, targets, ref stopMatchingForInfo);
					if (type2 != null)
					{
						item = new TypeSearchResult
						{
							MatchedInfo = processedTypeSearchInfo.Info,
							MatchedRule = typeMatchRule,
							MatchedType = type2,
							MatchedTargets = targets,
							MatchedIndex = this
						};
						list.Add(item);
						break;
					}
					if (stopMatchingForInfo)
					{
						break;
					}
				}
			}
			return list.ToArray();
		}

		private static void InsertIndexedTypeSorted(List<ProcessedTypeSearchInfo> indexedTypes, ProcessedTypeSearchInfo typeInfo)
		{
			double priority = typeInfo.Info.Priority;
			int num = 0;
			int num2 = indexedTypes.Count - 1;
			int i = 0;
			int num3 = 0;
			while (num <= num2)
			{
				i = (num + num2) / 2;
				ProcessedTypeSearchInfo processedTypeSearchInfo = indexedTypes[i];
				num3 = ((priority < processedTypeSearchInfo.Info.Priority) ? (-1) : ((priority > processedTypeSearchInfo.Info.Priority) ? 1 : 0));
				if (num3 < 0)
				{
					num = i + 1;
					continue;
				}
				if (num3 <= 0)
				{
					break;
				}
				num2 = i - 1;
			}
			if (num3 == 0)
			{
				for (int count = indexedTypes.Count; i + 1 < count; i++)
				{
					ProcessedTypeSearchInfo processedTypeSearchInfo2 = indexedTypes[i + 1];
					if (priority > processedTypeSearchInfo2.Info.Priority)
					{
						i++;
						break;
					}
				}
			}
			else if (num3 < 0)
			{
				i++;
			}
			indexedTypes.Insert(i, typeInfo);
		}

		private ProcessedTypeSearchInfo ProcessInfo(TypeSearchInfo info)
		{
			TypeSearchInfo arg = info;
			if (info.Targets == null)
			{
				info.Targets = Type.EmptyTypes;
			}
			for (int i = 0; i < info.Targets.Length; i++)
			{
				if (info.Targets[i] == null)
				{
					throw new ArgumentNullException("Target at index " + i + " in info for match type " + info.MatchType.GetNiceFullName() + " is null.");
				}
			}
			for (int j = 0; j < IndexingRules.Count; j++)
			{
				TypeMatchIndexingRule typeMatchIndexingRule = IndexingRules[j];
				string errorMessage = null;
				if (typeMatchIndexingRule.Process(ref info, ref errorMessage))
				{
					continue;
				}
				if (LogInvalidTypeInfo != null)
				{
					if (errorMessage == null)
					{
						LogInvalidTypeInfo("Invalid " + MatchedTypeLogName + " declaration '" + arg.MatchType.GetNiceFullName() + "'! Rule '" + typeMatchIndexingRule.Name.Replace("{name}", MatchedTypeLogName) + "' failed.", arg);
					}
					else
					{
						errorMessage = errorMessage.Replace("{name}", MatchedTypeLogName);
						LogInvalidTypeInfo("Invalid " + MatchedTypeLogName + " declaration '" + arg.MatchType.GetNiceFullName() + "'! Rule '" + typeMatchIndexingRule.Name.Replace("{name}", MatchedTypeLogName) + "' failed with message: " + errorMessage, arg);
					}
				}
				return null;
			}
			ProcessedTypeSearchInfo processedTypeSearchInfo = new ProcessedTypeSearchInfo();
			processedTypeSearchInfo.Info = info;
			processedTypeSearchInfo.Matchers = new List<TypeMatcher>(TypeMatcherCreators.Count);
			for (int k = 0; k < TypeMatcherCreators.Count; k++)
			{
				if (TypeMatcherCreators[k].TryCreateMatcher(info, out var matcher))
				{
					processedTypeSearchInfo.Matchers.Add(matcher);
				}
			}
			return processedTypeSearchInfo;
		}

		public void AddDefaultMatchRules()
		{
		}

		public void AddDefaultMatchCreators()
		{
			lock (LOCK)
			{
				TypeMatcherCreators.Add(new ExactTypeMatcher.Creator());
				TypeMatcherCreators.Add(new GenericSingleTargetTypeMatcher.Creator());
				TypeMatcherCreators.Add(new TargetsSatisfyGenericParameterConstraintsTypeMatcher.Creator());
				TypeMatcherCreators.Add(new GenericParameterInferenceTypeMatcher.Creator());
				TypeMatcherCreators.Add(new NestedInSameGenericTypeTypeMatcher.Creator());
			}
		}

		public void AddDefaultIndexingRules()
		{
			lock (LOCK)
			{
				IndexingRules.Add(DefaultIndexingRules.MustBeAbleToInstantiateType);
				IndexingRules.Add(DefaultIndexingRules.NoAbstractOrInterfaceTargets);
				IndexingRules.Add(DefaultIndexingRules.GenericMatchTypeValidation);
				IndexingRules.Add(DefaultIndexingRules.GenericDefinitionSanityCheck);
			}
		}
	}
}
