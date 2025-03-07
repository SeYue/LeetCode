using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.OdinInspector.Editor.TypeSearch;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public static class DrawerUtilities
	{
		private static class Null
		{
		}

		private struct DrawerAndPriority
		{
			public Type Drawer;

			public DrawerPriority Priority;

			public string Name;
		}

		public static class InvalidAttributeTargetUtility
		{
			private static readonly Dictionary<Type, List<Type>> ConcreteAttributeTargets = new Dictionary<Type, List<Type>>(FastTypeComparer.Instance);

			private static readonly Dictionary<Type, List<Type>> GenericParameterAttributeTargets = new Dictionary<Type, List<Type>>(FastTypeComparer.Instance);

			private static readonly DoubleLookupDictionary<Type, Type, bool> ShowErrorCache = new DoubleLookupDictionary<Type, Type, bool>(FastTypeComparer.Instance, FastTypeComparer.Instance);

			private static readonly List<Type> EmptyList = new List<Type>();

			public static void RegisterValidAttributeTarget(Type attribute, Type target)
			{
				List<Type> value;
				if (attribute.IsGenericParameter)
				{
					if (!GenericParameterAttributeTargets.TryGetValue(attribute, out value))
					{
						value = new List<Type>();
						GenericParameterAttributeTargets[attribute] = value;
					}
				}
				else if (!ConcreteAttributeTargets.TryGetValue(attribute, out value))
				{
					value = new List<Type>();
					ConcreteAttributeTargets[attribute] = value;
				}
				value.Add(target);
			}

			public static List<Type> GetValidTargets(Type attribute)
			{
				if (!ConcreteAttributeTargets.TryGetValue(attribute, out var value) && !GenericParameterAttributeTargets.TryGetValue(attribute, out value))
				{
					bool flag = false;
					foreach (KeyValuePair<Type, List<Type>> genericParameterAttributeTarget in GenericParameterAttributeTargets)
					{
						Type key = genericParameterAttributeTarget.Key;
						List<Type> value2 = genericParameterAttributeTarget.Value;
						if (key.GenericParameterIsFulfilledBy(attribute))
						{
							value = value2.ToList();
							ConcreteAttributeTargets[attribute] = value;
							flag = true;
						}
					}
					if (!flag)
					{
						ConcreteAttributeTargets[attribute] = null;
					}
				}
				return value ?? EmptyList;
			}

			public static bool ShowInvalidAttributeErrorFor(InspectorProperty property, Type attribute)
			{
				if (property.ValueEntry == null)
				{
					return false;
				}
				if (property.ValueEntry.BaseValueType == typeof(object))
				{
					return false;
				}
				if (property.Parent != null && property.Parent.ChildResolver is ICollectionResolver)
				{
					return false;
				}
				if (property.GetAttribute<SuppressInvalidAttributeErrorAttribute>() != null)
				{
					return false;
				}
				if (property.Info.TypeOfValue.IsInterface)
				{
					return false;
				}
				ICollectionResolver collectionResolver = property.ChildResolver as ICollectionResolver;
				if (collectionResolver != null)
				{
					if (collectionResolver.ElementType == typeof(object))
					{
						return false;
					}
					if (collectionResolver.ElementType.IsInterface)
					{
						return false;
					}
					if (ShowInvalidAttributeErrorFor(attribute, property.ValueEntry.BaseValueType))
					{
						return ShowInvalidAttributeErrorFor(attribute, collectionResolver.ElementType);
					}
					return false;
				}
				return ShowInvalidAttributeErrorFor(attribute, property.ValueEntry.BaseValueType);
			}

			public static bool ShowInvalidAttributeErrorFor(Type attribute, Type value)
			{
				if (!ShowErrorCache.TryGetInnerValue(attribute, value, out var value2))
				{
					value2 = CalculateShowInvalidAttributeErrorFor(attribute, value);
					ShowErrorCache[attribute][value] = value2;
				}
				return value2;
			}

			private static bool CalculateShowInvalidAttributeErrorFor(Type attribute, Type value)
			{
				if (attribute == typeof(DelayedAttribute) || attribute == typeof(DelayedPropertyAttribute))
				{
					return false;
				}
				List<Type> validTargets = GetValidTargets(attribute);
				if (validTargets.Count == 0)
				{
					return false;
				}
				if (value == typeof(object))
				{
					return false;
				}
				for (int i = 0; i < validTargets.Count; i++)
				{
					Type type = validTargets[i];
					if (type == value)
					{
						return false;
					}
					if (type.IsGenericParameter && type.GenericParameterIsFulfilledBy(value))
					{
						return false;
					}
				}
				return true;
			}
		}

		public static readonly TypeSearchIndex SearchIndex;

		private static List<DrawerAndPriority> AllDrawerTypes;

		private static readonly FieldInfo CustomPropertyDrawerTypeField;

		private static readonly FieldInfo CustomPropertyDrawerUseForChildrenField;

		private static readonly bool SupportsUnityDrawers;

		private static readonly Dictionary<Type, DrawerPriority> DrawerTypePriorityLookup;

		private static readonly Dictionary<Type, OdinDrawer> UninitializedDrawers;

		private static TypeSearchResult[][] CachedQueryResultArray;

		private static readonly Type AbstractTypeUnityPropertyDrawer_TArg2;

		private static readonly Type UnityPropertyAttributeDrawer_TArg1;

		private static readonly Type UnityDecoratorAttributeDrawer_TArg1;

		private static readonly TypeMatchRule InvalidAttributeRule;

		/// <summary>
		/// Odin has its own implementations for these attribute drawers; never use Unity's.
		/// </summary>
		private static readonly HashSet<string> ExcludeUnityDrawers;

		private static readonly string MatchCachePath;

		private static readonly Dictionary<Type, TypeSearchResult[]> InvalidAttributeTypeSearchResults;

		static DrawerUtilities()
		{
			SearchIndex = new TypeSearchIndex
			{
				MatchedTypeLogName = "drawer"
			};
			CustomPropertyDrawerTypeField = typeof(CustomPropertyDrawer).GetField("m_Type", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			CustomPropertyDrawerUseForChildrenField = typeof(CustomPropertyDrawer).GetField("m_UseForChildren", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			SupportsUnityDrawers = CustomPropertyDrawerTypeField != null && CustomPropertyDrawerUseForChildrenField != null;
			DrawerTypePriorityLookup = new Dictionary<Type, DrawerPriority>(FastTypeComparer.Instance);
			UninitializedDrawers = new Dictionary<Type, OdinDrawer>(FastTypeComparer.Instance);
			CachedQueryResultArray = new TypeSearchResult[16][];
			AbstractTypeUnityPropertyDrawer_TArg2 = typeof(AbstractTypeUnityPropertyDrawer<, , >).GetGenericArguments()[2];
			UnityPropertyAttributeDrawer_TArg1 = typeof(UnityPropertyAttributeDrawer<, , >).GetGenericArguments()[1];
			UnityDecoratorAttributeDrawer_TArg1 = typeof(UnityDecoratorAttributeDrawer<, , >).GetGenericArguments()[1];
			InvalidAttributeRule = new TypeMatchRule("Invalid Attribute Notification Dummy Rule (This is never matched against, but only serves to be a result rule for invalid attribute type search results)", (TypeSearchInfo info, Type[] target) => null);
			ExcludeUnityDrawers = new HashSet<string> { "HeaderDrawer", "DelayedDrawer", "MultilineDrawer", "RangeDrawer", "SpaceDrawer", "TextAreaDrawer", "ColorUsageDrawer" };
			MatchCachePath = "Temp/Odin_TypeSearchIndex_MatchCache_Drawers.txt";
			InvalidAttributeTypeSearchResults = new Dictionary<Type, TypeSearchResult[]>(FastTypeComparer.Instance);
			using (SimpleProfiler.Section("DrawerUtilities"))
			{
				if (!SupportsUnityDrawers)
				{
					Debug.LogWarning((object)"Could not find internal fields 'm_Type' and/or 'm_UseForChildren' in type CustomPropertyDrawer in this version of Unity; support for legacy Unity PropertyDrawers and DecoratorDrawers have been disabled in Odin's inspector. Please report this on Odin's issue tracker.");
				}
				if (UnityTypeCacheUtility.IsAvailable)
				{
					IList<Type> typesDerivedFrom = UnityTypeCacheUtility.GetTypesDerivedFrom(typeof(OdinDrawer));
					IList<Type> list = (SupportsUnityDrawers ? UnityTypeCacheUtility.GetTypesDerivedFrom(typeof(GUIDrawer)) : null);
					int num = typesDerivedFrom.Count;
					if (list != null)
					{
						num += list.Count;
						num += 50;
					}
					AllDrawerTypes = new List<DrawerAndPriority>(num);
					foreach (Type item in typesDerivedFrom)
					{
						if (!item.IsAbstract)
						{
							ProcessDrawerType(item, isOdin: true, isUnity: false);
						}
					}
					if (SupportsUnityDrawers)
					{
						foreach (Type item2 in list)
						{
							if (!item2.IsAbstract)
							{
								ProcessDrawerType(item2, isOdin: false, isUnity: true);
							}
						}
					}
				}
				else
				{
					AllDrawerTypes = new List<DrawerAndPriority>(200);
					List<Type> list2 = AssemblyUtilities.GetTypes(AssemblyTypeFlags.CustomTypes | AssemblyTypeFlags.UnityEditorTypes).ToList();
					for (int i = 0; i < list2.Count; i++)
					{
						Type type = list2[i];
						if (!type.IsAbstract && type.IsClass)
						{
							bool flag = typeof(OdinDrawer).IsAssignableFrom(type);
							bool isUnity = !flag && typeof(GUIDrawer).IsAssignableFrom(type);
							ProcessDrawerType(type, flag, isUnity);
						}
					}
				}
				SearchIndex.MatchRules.Add(new TypeMatchRule("Unity Drawer Generic Target Matcher", delegate(TypeSearchInfo info, Type[] targets)
				{
					if (targets.Length != 1)
					{
						return null;
					}
					if (!info.Targets[0].IsGenericTypeDefinition)
					{
						return null;
					}
					Type genericTypeDefinition = info.MatchType.GetGenericTypeDefinition();
					bool flag2 = genericTypeDefinition == typeof(AbstractTypeUnityPropertyDrawer<, , >);
					bool flag3 = genericTypeDefinition == typeof(UnityPropertyDrawer<, >);
					if (!(flag2 || flag3))
					{
						return null;
					}
					if (flag2)
					{
						if (targets[0].ImplementsOpenGenericType(info.Targets[0]))
						{
							Type[] genericArguments = info.MatchType.GetGenericArguments();
							return info.MatchType.GetGenericTypeDefinition().MakeGenericType(genericArguments[0], targets[0], targets[0]);
						}
					}
					else
					{
						if (!targets[0].IsGenericType)
						{
							return null;
						}
						if (targets[0].GetGenericTypeDefinition() == info.Targets[0])
						{
							Type[] genericArguments2 = info.MatchType.GetGenericArguments();
							return info.MatchType.GetGenericTypeDefinition().MakeGenericType(genericArguments2[0], targets[0]);
						}
					}
					return null;
				}));
				for (int j = 0; j < AllDrawerTypes.Count; j++)
				{
					Type drawer = AllDrawerTypes[j].Drawer;
					TypeSearchInfo typeToIndex = new TypeSearchInfo
					{
						MatchType = drawer,
						Priority = AllDrawerTypes.Count - j,
						Targets = null
					};
					Type[] argumentsOfInheritedOpenGenericClass;
					if ((argumentsOfInheritedOpenGenericClass = drawer.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinValueDrawer<>))) != null)
					{
						typeToIndex.Targets = argumentsOfInheritedOpenGenericClass;
					}
					else if (drawer.ImplementsOpenGenericClass(typeof(OdinAttributeDrawer<>)))
					{
						if ((argumentsOfInheritedOpenGenericClass = drawer.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinAttributeDrawer<, >))) != null)
						{
							typeToIndex.Targets = argumentsOfInheritedOpenGenericClass;
							InvalidAttributeTargetUtility.RegisterValidAttributeTarget(typeToIndex.Targets[0], typeToIndex.Targets[1]);
						}
						else
						{
							typeToIndex.Targets = drawer.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinAttributeDrawer<>));
						}
					}
					else if ((argumentsOfInheritedOpenGenericClass = drawer.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinGroupDrawer<>))) != null)
					{
						typeToIndex.Targets = argumentsOfInheritedOpenGenericClass;
					}
					else if (!drawer.IsFullyConstructedGenericType())
					{
						typeToIndex.Targets = drawer.GetGenericArguments();
					}
					typeToIndex.Targets = typeToIndex.Targets ?? Type.EmptyTypes;
					SearchIndex.AddIndexedTypeUnsorted(typeToIndex);
				}
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (Assembly assembly in assemblies)
				{
					object[] array = assembly.SafeGetCustomAttributes(typeof(StaticInitializeBeforeDrawingAttribute), inherit: false);
					foreach (object obj in array)
					{
						StaticInitializeBeforeDrawingAttribute staticInitializeBeforeDrawingAttribute = obj as StaticInitializeBeforeDrawingAttribute;
						if (staticInitializeBeforeDrawingAttribute.Types == null)
						{
							continue;
						}
						Type[] types = staticInitializeBeforeDrawingAttribute.Types;
						foreach (Type type2 in types)
						{
							if (type2 != null)
							{
								RuntimeHelpers.RunClassConstructor(type2.TypeHandle);
							}
						}
					}
				}
				if (GlobalConfig<GeneralDrawerConfig>.Instance.PrecomputeTypeMatching)
				{
					new Thread(WarmUpTypeSearchIndexIfCacheExists).Start();
				}
				AppDomain.CurrentDomain.DomainUnload += SaveSearchIndexCache;
			}
		}

		private static void SaveSearchIndexCache(object sender, EventArgs e)
		{
			if (!GlobalConfig<GeneralDrawerConfig>.HasInstanceLoaded || !GlobalConfig<GeneralDrawerConfig>.Instance.PrecomputeTypeMatching)
			{
				return;
			}
			try
			{
				TwoWaySerializationBinder @default = TwoWaySerializationBinder.Default;
				List<Type[]> allCachedTargets = SearchIndex.GetAllCachedTargets();
				using FileStream stream = new FileStream(MatchCachePath, FileMode.Create, FileAccess.Write, FileShare.None);
				using StreamWriter streamWriter = new StreamWriter(stream);
				foreach (Type[] item in allCachedTargets)
				{
					for (int i = 0; i < item.Length; i++)
					{
						if (i != 0)
						{
							streamWriter.Write("|");
						}
						streamWriter.Write(@default.BindToName(item[i]));
					}
					streamWriter.WriteLine();
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		private static void WarmUpTypeSearchIndexIfCacheExists()
		{
			try
			{
				TwoWaySerializationBinder @default = TwoWaySerializationBinder.Default;
				if (!File.Exists(MatchCachePath))
				{
					return;
				}
				string[] array = File.ReadAllLines(MatchCachePath);
				List<Type> list = new List<Type>();
				string[] array2 = array;
				foreach (string text in array2)
				{
					string[] array3 = text.Split('|');
					list.Clear();
					bool flag = true;
					string[] array4 = array3;
					foreach (string typeName in array4)
					{
						Type type = @default.BindToType(typeName);
						if (type == null)
						{
							flag = false;
							break;
						}
						list.Add(type);
					}
					if (flag && list.Count != 0)
					{
						if (list.Count == 1)
						{
							SearchIndex.GetMatches(list[0]);
						}
						else if (list.Count == 2)
						{
							SearchIndex.GetMatches(list[0], list[1]);
						}
						else
						{
							SearchIndex.GetMatches(list.ToArray());
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		private static bool FastStartsWith(string str, string startsWith)
		{
			if (startsWith.Length > str.Length)
			{
				return false;
			}
			for (int i = 0; i < startsWith.Length; i++)
			{
				if (str[i] != startsWith[i])
				{
					return false;
				}
			}
			return true;
		}

		private static void InsertSortedIntoAllDrawerTypes(Type drawer)
		{
			string name = drawer.Name;
			DrawerPriority drawerPriority = GetDrawerPriority(drawer);
			int num = 0;
			int num2 = AllDrawerTypes.Count - 1;
			int i = 0;
			int num3 = 0;
			while (num <= num2)
			{
				i = (num + num2) / 2;
				DrawerAndPriority drawerAndPriority = AllDrawerTypes[i];
				num3 = drawerPriority.CompareTo(drawerAndPriority.Priority);
				if (num3 == 0)
				{
					num3 = -name.CompareTo(drawerAndPriority.Name);
				}
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
				for (int count = AllDrawerTypes.Count; i + 1 < count; i++)
				{
					DrawerAndPriority drawerAndPriority2 = AllDrawerTypes[i + 1];
					if (drawerPriority > drawerAndPriority2.Priority)
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
			AllDrawerTypes.Insert(i, new DrawerAndPriority
			{
				Drawer = drawer,
				Priority = drawerPriority,
				Name = name
			});
		}

		private static void ProcessDrawerType(Type type, bool isOdin, bool isUnity)
		{
			if ((!isOdin && !isUnity) || (isUnity && !SupportsUnityDrawers) || type.IsDefined(typeof(OdinDontRegisterAttribute), inherit: false))
			{
				return;
			}
			string @namespace = type.Namespace;
			if (@namespace != null && FastStartsWith(@namespace, "Unity") && ExcludeUnityDrawers.Contains(type.Name))
			{
				return;
			}
			if (isOdin)
			{
				InsertSortedIntoAllDrawerTypes(type);
			}
			else
			{
				if (type.IsGenericTypeDefinition || type.GetConstructor(Type.EmptyTypes) == null)
				{
					return;
				}
				bool flag = typeof(PropertyDrawer).IsAssignableFrom(type);
				bool flag2 = !flag && typeof(DecoratorDrawer).IsAssignableFrom(type);
				if (!flag && !flag2)
				{
					return;
				}
				object[] customAttributes = type.GetCustomAttributes(typeof(CustomPropertyDrawer), inherit: false);
				foreach (object obj in customAttributes)
				{
					Type type2 = CustomPropertyDrawerTypeField.GetValue(obj) as Type;
					if (type2 != null)
					{
						bool flag3 = typeof(PropertyAttribute).IsAssignableFrom(type2);
						if (!flag2 || flag3)
						{
							bool flag4 = (bool)CustomPropertyDrawerUseForChildrenField.GetValue(obj);
							Type drawer = (flag ? (flag3 ? ((!flag4 && !type2.IsAbstract) ? typeof(UnityPropertyAttributeDrawer<, , >).MakeGenericType(type, type2, typeof(PropertyAttribute)) : typeof(UnityPropertyAttributeDrawer<, , >).MakeGenericType(type, UnityPropertyAttributeDrawer_TArg1, type2)) : ((!flag4 && !type2.IsAbstract) ? typeof(UnityPropertyDrawer<, >).MakeGenericType(type, type2) : typeof(AbstractTypeUnityPropertyDrawer<, , >).MakeGenericType(type, type2, AbstractTypeUnityPropertyDrawer_TArg2))) : ((!flag4 && !type2.IsAbstract) ? typeof(UnityDecoratorAttributeDrawer<, , >).MakeGenericType(type, type2, typeof(PropertyAttribute)) : typeof(UnityDecoratorAttributeDrawer<, , >).MakeGenericType(type, UnityDecoratorAttributeDrawer_TArg1, type2)));
							InsertSortedIntoAllDrawerTypes(drawer);
						}
					}
				}
			}
		}

		public static void GetDefaultPropertyDrawers(InspectorProperty property, ref TypeSearchResult[] resultArray, ref int resultCount)
		{
			resultCount = 0;
			int resultsCount = 0;
			CachedQueryResultArray[resultsCount++] = SearchIndex.GetMatches(Type.EmptyTypes);
			if (property.ValueEntry != null)
			{
				CachedQueryResultArray[resultsCount++] = SearchIndex.GetMatches(property.ValueEntry.TypeOfValue);
			}
			int num = 2 + property.Attributes.Count * 3;
			while (CachedQueryResultArray.Length <= num)
			{
				ExpandArray(ref CachedQueryResultArray);
			}
			for (int i = 0; i < property.Attributes.Count; i++)
			{
				Type type = property.Attributes[i].GetType();
				CachedQueryResultArray[resultsCount++] = SearchIndex.GetMatches(type);
				if (property.ValueEntry != null)
				{
					CachedQueryResultArray[resultsCount++] = SearchIndex.GetMatches(type, property.ValueEntry.TypeOfValue);
					if (InvalidAttributeTargetUtility.ShowInvalidAttributeErrorFor(property, type))
					{
						CachedQueryResultArray[resultsCount++] = GetInvalidAttributeTypeSearchResult(type);
					}
				}
			}
			TypeSearchResult[] cachedMergedQueryResults = TypeSearchIndex.GetCachedMergedQueryResults(CachedQueryResultArray, resultsCount);
			for (int j = 0; j < cachedMergedQueryResults.Length; j++)
			{
				TypeSearchResult typeSearchResult = cachedMergedQueryResults[j];
				if (DrawerTypeCanDrawProperty(typeSearchResult.MatchedType, property))
				{
					if (resultCount == resultArray.Length)
					{
						ExpandArray(ref resultArray);
					}
					resultArray[resultCount++] = cachedMergedQueryResults[j];
				}
			}
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

		private static TypeSearchResult[] GetInvalidAttributeTypeSearchResult(Type attr)
		{
			if (!InvalidAttributeTypeSearchResults.TryGetValue(attr, out var value))
			{
				value = new TypeSearchResult[1]
				{
					new TypeSearchResult
					{
						MatchedInfo = new TypeSearchInfo
						{
							MatchType = typeof(InvalidAttributeNotificationDrawer<>),
							Priority = double.MaxValue,
							Targets = Type.EmptyTypes
						},
						MatchedRule = InvalidAttributeRule,
						MatchedTargets = Type.EmptyTypes,
						MatchedType = typeof(InvalidAttributeNotificationDrawer<>).MakeGenericType(attr)
					}
				};
				InvalidAttributeTypeSearchResults.Add(attr, value);
			}
			return value;
		}

		/// <summary>
		/// Gets the priority of a given drawer type.
		/// </summary>
		public static DrawerPriority GetDrawerPriority(Type drawerType)
		{
			if (!DrawerTypePriorityLookup.TryGetValue(drawerType, out var value))
			{
				value = CalculateDrawerPriority(drawerType);
				DrawerTypePriorityLookup[drawerType] = value;
			}
			return value;
		}

		private static DrawerPriority CalculateDrawerPriority(Type drawerType)
		{
			DrawerPriority drawerPriority = DrawerPriority.AutoPriority;
			DrawerPriority drawerPriority2 = default(DrawerPriority);
			DrawerPriorityAttribute drawerPriorityAttribute = null;
			if (DrawerIsUnityAlias(drawerType))
			{
				Type type = drawerType.GetGenericArguments()[0];
				if (type.IsDefined(typeof(DrawerPriorityAttribute), inherit: false))
				{
					drawerPriorityAttribute = type.GetCustomAttribute<DrawerPriorityAttribute>(inherit: false);
				}
				if (drawerPriorityAttribute == null)
				{
					AssemblyTypeFlags assemblyTypeFlag = type.Assembly.GetAssemblyTypeFlag();
					if ((assemblyTypeFlag & (AssemblyTypeFlags.UnityTypes | AssemblyTypeFlags.UnityEditorTypes)) != 0)
					{
						drawerPriority2.Value -= 0.1;
					}
				}
			}
			if (drawerPriorityAttribute == null && drawerType.IsDefined(typeof(DrawerPriorityAttribute), inherit: false))
			{
				drawerPriorityAttribute = drawerType.GetCustomAttribute<DrawerPriorityAttribute>(inherit: false);
			}
			if (drawerPriorityAttribute != null)
			{
				drawerPriority = drawerPriorityAttribute.Priority;
			}
			if (drawerPriority == DrawerPriority.AutoPriority)
			{
				drawerPriority = ((!drawerType.ImplementsOpenGenericClass(typeof(OdinAttributeDrawer<>))) ? DrawerPriority.ValuePriority : DrawerPriority.AttributePriority);
				if (drawerType.Assembly == typeof(OdinEditor).Assembly)
				{
					drawerPriority.Value -= 0.001;
				}
			}
			drawerPriority += drawerPriority2;
			return drawerPriority;
		}

		private static bool DrawerIsUnityAlias(Type drawerType)
		{
			if (!drawerType.IsGenericType || drawerType.IsGenericTypeDefinition)
			{
				return false;
			}
			Type genericTypeDefinition = drawerType.GetGenericTypeDefinition();
			if (genericTypeDefinition != typeof(UnityPropertyDrawer<, >) && genericTypeDefinition != typeof(UnityPropertyAttributeDrawer<, , >) && genericTypeDefinition != typeof(UnityDecoratorAttributeDrawer<, , >))
			{
				return genericTypeDefinition == typeof(AbstractTypeUnityPropertyDrawer<, , >);
			}
			return true;
		}

		public static bool DrawerTypeCanDrawProperty(Type drawerType, InspectorProperty property)
		{
			OdinDrawer cachedUninitializedDrawer = GetCachedUninitializedDrawer(drawerType);
			return cachedUninitializedDrawer.CanDrawProperty(property);
		}

		public static OdinDrawer GetCachedUninitializedDrawer(Type drawerType)
		{
			if (!UninitializedDrawers.TryGetValue(drawerType, out var value))
			{
				value = (OdinDrawer)FormatterServices.GetUninitializedObject(drawerType);
				UninitializedDrawers[drawerType] = value;
			}
			return value;
		}

		public static bool HasAttributeDrawer(Type attributeType)
		{
			return (from d in AllDrawerTypes
				select d.Drawer.GetBaseClasses().FirstOrDefault((Type x) => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(OdinAttributeDrawer<>)) into d
				where d != null
				select d).Any((Type d) => d.GetGenericArguments()[0] == attributeType);
		}
	}
}
