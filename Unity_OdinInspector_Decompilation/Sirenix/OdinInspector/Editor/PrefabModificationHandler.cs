using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>Handles all prefab modifications that apply to the targets of a property tree, if any. This class determines which properties have modifications, what the modifications are, auto-applies modifications if the current instance values do not correspond to the prefab values, and also provides an API for modifying those modifications.</para>
	/// <para>NOTE: This class is liable to see a lot of changes, as the prefab modification system is slated to be redesigned for increased extendability in the future. Do not depend overly on the current API.</para>
	/// </summary>
	public sealed class PrefabModificationHandler
	{
		private readonly bool targetSupportsPrefabSerialization;

		private ImmutableList<Object> immutableTargetPrefabs;

		private bool hasPrefabs;

		private bool allTargetsHaveSamePrefab;

		private Dictionary<string, PrefabModification>[] prefabValueModifications;

		private Dictionary<string, PrefabModification>[] prefabListLengthModifications;

		private Dictionary<string, PrefabModification>[] prefabDictionaryModifications;

		private PropertyTree prefabPropertyTree;

		private int[] prefabPropertyTreeIndexMap;

		private bool allowAutoRegisterPrefabModifications = true;

		private PathLookup<PropertyModification>[] unityModLookups;

		public PropertyTree Tree { get; private set; }

		/// <summary>
		/// The prefabs for each prefab instance represented by the property tree, if any.
		/// </summary>
		public ImmutableList<Object> TargetPrefabs => immutableTargetPrefabs;

		/// <summary>
		/// Whether any of the values the property tree represents are prefab instances.
		/// </summary>
		public bool HasPrefabs => hasPrefabs;

		/// <summary>
		/// A prefab tree for the prefabs of this property tree's prefab instances, if any exist.
		/// </summary>
		public PropertyTree PrefabPropertyTree => prefabPropertyTree;

		public bool HasNestedOdinPrefabData { get; private set; }

		public PrefabModificationHandler(PropertyTree tree)
		{
			Tree = tree;
			prefabValueModifications = new Dictionary<string, PrefabModification>[tree.WeakTargets.Count];
			prefabListLengthModifications = new Dictionary<string, PrefabModification>[tree.WeakTargets.Count];
			prefabDictionaryModifications = new Dictionary<string, PrefabModification>[tree.WeakTargets.Count];
			prefabPropertyTreeIndexMap = new int[tree.WeakTargets.Count];
			unityModLookups = new PathLookup<PropertyModification>[tree.WeakTargets.Count];
			Type targetType = tree.TargetType;
			targetSupportsPrefabSerialization = !tree.IsStatic && typeof(Object).IsAssignableFrom(targetType) && typeof(ISupportsPrefabSerialization).IsAssignableFrom(targetType);
		}

		public void Update()
		{
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Expected O, but got Unknown
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Invalid comparison between Unknown and I4
			//IL_0469: Unknown result type (might be due to invalid IL or missing references)
			//IL_0470: Expected O, but got Unknown
			//IL_0472: Unknown result type (might be due to invalid IL or missing references)
			//IL_0477: Unknown result type (might be due to invalid IL or missing references)
			//IL_0479: Unknown result type (might be due to invalid IL or missing references)
			//IL_047c: Invalid comparison between Unknown and I4
			hasPrefabs = false;
			HasNestedOdinPrefabData = false;
			if (Tree.IsStatic)
			{
				return;
			}
			Object[] array = (Object[])(object)new Object[Tree.WeakTargets.Count];
			if (typeof(Object).IsAssignableFrom(Tree.TargetType))
			{
				int num = 0;
				for (int i = 0; i < Tree.WeakTargets.Count; i++)
				{
					Object val = (Object)Tree.WeakTargets[i];
					bool flag = false;
					Object val2 = null;
					if (val != (Object)null)
					{
						PrefabType prefabType = PrefabUtility.GetPrefabType(val);
						if (!HasNestedOdinPrefabData && OdinPrefabSerializationEditorUtility.ObjectHasNestedOdinPrefabData(val))
						{
							HasNestedOdinPrefabData = true;
						}
						if ((int)prefabType == 3 && (val2 = OdinPrefabSerializationEditorUtility.GetCorrespondingObjectFromSource(val)) != (Object)null)
						{
							flag = true;
						}
					}
					if (flag)
					{
						array[i] = val2;
						hasPrefabs = true;
						prefabPropertyTreeIndexMap[i] = num;
						num++;
						if (!targetSupportsPrefabSerialization)
						{
							continue;
						}
						ISupportsPrefabSerialization supportsPrefabSerialization = (ISupportsPrefabSerialization)Tree.WeakTargets[i];
						List<PrefabModification> list = PrefabModificationCache.DeserializePrefabModificationsCached(val, supportsPrefabSerialization.SerializationData.PrefabModifications, supportsPrefabSerialization.SerializationData.PrefabModificationsReferencedUnityObjects);
						Dictionary<string, PrefabModification> dictionary = prefabListLengthModifications[i] ?? new Dictionary<string, PrefabModification>();
						Dictionary<string, PrefabModification> dictionary2 = prefabValueModifications[i] ?? new Dictionary<string, PrefabModification>();
						Dictionary<string, PrefabModification> dictionary3 = prefabDictionaryModifications[i] ?? new Dictionary<string, PrefabModification>();
						dictionary.Clear();
						dictionary2.Clear();
						dictionary3.Clear();
						for (int j = 0; j < list.Count; j++)
						{
							PrefabModification prefabModification = list[j];
							switch (prefabModification.ModificationType)
							{
							case PrefabModificationType.Value:
								if (!dictionary2.ContainsKey(prefabModification.Path))
								{
									dictionary2[prefabModification.Path] = prefabModification;
								}
								break;
							case PrefabModificationType.ListLength:
								if (!dictionary.ContainsKey(prefabModification.Path))
								{
									dictionary[prefabModification.Path] = prefabModification;
								}
								break;
							case PrefabModificationType.Dictionary:
								if (!dictionary3.ContainsKey(prefabModification.Path))
								{
									dictionary3[prefabModification.Path] = prefabModification;
								}
								break;
							default:
								throw new NotImplementedException(prefabModification.ModificationType.ToString());
							}
						}
						List<PrefabModification> registeredPrefabModifications = UnitySerializationUtility.GetRegisteredPrefabModifications(val);
						if (registeredPrefabModifications != null)
						{
							for (int k = 0; k < registeredPrefabModifications.Count; k++)
							{
								PrefabModification prefabModification2 = registeredPrefabModifications[k];
								if (prefabModification2.ModificationType == PrefabModificationType.Value)
								{
									dictionary2[prefabModification2.Path] = prefabModification2;
								}
								else if (prefabModification2.ModificationType == PrefabModificationType.ListLength)
								{
									dictionary[prefabModification2.Path] = prefabModification2;
								}
								else if (prefabModification2.ModificationType == PrefabModificationType.Dictionary)
								{
									dictionary3[prefabModification2.Path] = prefabModification2;
								}
							}
						}
						prefabListLengthModifications[i] = dictionary;
						prefabValueModifications[i] = dictionary2;
						prefabDictionaryModifications[i] = dictionary3;
					}
					else
					{
						prefabPropertyTreeIndexMap[i] = -1;
					}
				}
				if (num > 0)
				{
					Object[] array2 = (Object[])(object)new Object[num];
					for (int l = 0; l < array.Length; l++)
					{
						int num2 = prefabPropertyTreeIndexMap[l];
						if (num2 >= 0)
						{
							array2[num2] = array[l];
						}
					}
					if (prefabPropertyTree != null)
					{
						if (prefabPropertyTree.WeakTargets.Count != array2.Length)
						{
							prefabPropertyTree = null;
						}
						else
						{
							for (int m = 0; m < prefabPropertyTree.WeakTargets.Count; m++)
							{
								if (prefabPropertyTree.WeakTargets[m] != array2[m])
								{
									prefabPropertyTree = null;
									break;
								}
							}
						}
					}
					if (prefabPropertyTree == null)
					{
						prefabPropertyTree = PropertyTree.Create((object[])array2);
					}
					prefabPropertyTree.UpdateTree();
				}
				allTargetsHaveSamePrefab = false;
				if (num == Tree.WeakTargets.Count)
				{
					allTargetsHaveSamePrefab = true;
					Object val3 = array[0];
					for (int n = 1; n < array.Length; n++)
					{
						if (val3 != array[n])
						{
							allTargetsHaveSamePrefab = false;
							break;
						}
					}
				}
			}
			immutableTargetPrefabs = new ImmutableList<Object>(array.Cast<Object>().ToArray());
			if (!HasPrefabs || Tree.UnitySerializedObject == null || Application.get_isPlaying())
			{
				return;
			}
			for (int num3 = 0; num3 < Tree.WeakTargets.Count; num3++)
			{
				Object val4 = TargetPrefabs[num3];
				if (!(val4 != (Object)null))
				{
					continue;
				}
				Object val5 = (Object)Tree.WeakTargets[num3];
				PrefabType prefabType2 = PrefabUtility.GetPrefabType(val5);
				if ((int)prefabType2 != 3)
				{
					continue;
				}
				PropertyModification[] propertyModifications = PrefabUtility.GetPropertyModifications(val5);
				if (propertyModifications == null || propertyModifications.Length == 0)
				{
					continue;
				}
				PathLookup<PropertyModification> pathLookup = unityModLookups[num3];
				if (pathLookup == null)
				{
					pathLookup = new PathLookup<PropertyModification>();
					unityModLookups[num3] = pathLookup;
				}
				pathLookup.BeginRebuild();
				try
				{
					foreach (PropertyModification val6 in propertyModifications)
					{
						if (!(val6.target != val4))
						{
							pathLookup.AddValue(val6.propertyPath, val6);
						}
					}
				}
				finally
				{
					pathLookup.FinishRebuild();
				}
			}
		}

		private static bool FastStringEndsWith(string str, string endsWith)
		{
			int length = str.Length;
			int length2 = endsWith.Length;
			if (length < length2)
			{
				return false;
			}
			if (length == length2)
			{
				return str == endsWith;
			}
			for (int i = 1; i <= length2; i++)
			{
				if (str[length - i] != endsWith[length2 - i])
				{
					return false;
				}
			}
			return true;
		}

		private bool TargetHasRegisteredModificationsWaitingForApply()
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			for (int i = 0; i < Tree.WeakTargets.Count; i++)
			{
				Object val = (Object)Tree.WeakTargets[i];
				if (UnitySerializationUtility.HasModificationsWaitingForDelayedApply(val))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Gets the Unity PropertyModification for the property at this path, if there are any.
		/// </summary>
		/// <param name="path">The property path to get the modification for.</param>
		/// <param name="selectionIndex">The index of the tree target to get the modification for.</param>
		/// <param name="childrenHaveModifications">Whether any children of the path have modifications registered.</param>
		/// <returns></returns>
		public PropertyModification GetUnityPropertyModification(StringSlice path, int selectionIndex, out bool childrenHaveModifications)
		{
			childrenHaveModifications = false;
			PathLookup<PropertyModification> pathLookup = unityModLookups[selectionIndex];
			if (pathLookup == null)
			{
				return null;
			}
			if (pathLookup.TryGetValue(path, out var _, out childrenHaveModifications, out var value))
			{
				return value;
			}
			return null;
		}

		/// <summary>
		/// Gets the Odin prefab modification type of a given property, if any.
		/// </summary>
		/// <param name="property">The property to check.</param>
		/// <param name="forceAutoRegister"></param>
		/// <returns>
		/// The prefab modification type of the property if it has one, otherwise null.
		/// </returns>
		public PrefabModificationType? GetPrefabModificationType(InspectorProperty property, bool forceAutoRegister = false)
		{
			if (!HasPrefabs)
			{
				return null;
			}
			bool registerModification;
			PrefabModificationType? result = PrivateGetPrefabModificationType(property, out registerModification);
			if (result.HasValue && (forceAutoRegister || (allowAutoRegisterPrefabModifications && registerModification && allTargetsHaveSamePrefab && !TargetHasRegisteredModificationsWaitingForApply())))
			{
				switch (result.Value)
				{
				case PrefabModificationType.Value:
				{
					for (int j = 0; j < Tree.WeakTargets.Count; j++)
					{
						RegisterPrefabValueModification(property, j);
					}
					break;
				}
				case PrefabModificationType.ListLength:
				{
					for (int k = 0; k < Tree.WeakTargets.Count; k++)
					{
						property.Children.Update();
						RegisterPrefabListLengthModification(property, k, property.Children.Count);
					}
					break;
				}
				case PrefabModificationType.Dictionary:
				{
					for (int i = 0; i < Tree.WeakTargets.Count; i++)
					{
						property.Children.Update();
						RegisterPrefabDictionaryDeltaModification(property, i);
					}
					break;
				}
				}
			}
			return result;
		}

		private static bool PropertyCanHaveModifications(InspectorProperty property)
		{
			if (!property.SupportsPrefabModifications)
			{
				return false;
			}
			if (property.ValueEntry != null && property.ValueEntry.TypeOfValue.IsGenericType && property.ValueEntry.TypeOfValue.GetGenericTypeDefinition() == typeof(EditableKeyValuePair<, >))
			{
				return false;
			}
			return true;
		}

		private PrefabModificationType? PrivateGetPrefabModificationType(InspectorProperty property, out bool registerModification)
		{
			//IL_037b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0382: Expected O, but got Unknown
			//IL_038d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0394: Expected O, but got Unknown
			if (Application.get_isPlaying() || !HasPrefabs || !allTargetsHaveSamePrefab)
			{
				registerModification = false;
				return null;
			}
			if (!PropertyCanHaveModifications(property))
			{
				registerModification = false;
				if (property.Index == 0 && property.Parent != null && property.Parent.Parent != null && property.Parent.ValueEntry != null && property.Parent.ValueEntry.TypeOfValue.IsGenericType && property.Parent.ValueEntry.TypeOfValue.GetGenericTypeDefinition() == typeof(EditableKeyValuePair<, >))
				{
					InspectorProperty parent = property.Parent.Parent;
					for (int i = 0; i < prefabDictionaryModifications.Length; i++)
					{
						object key = (parent.ChildResolver as IKeyValueMapResolver).GetKey(i, property.Parent.Index);
						Dictionary<string, PrefabModification> dictionary = prefabDictionaryModifications[i];
						if (dictionary != null && dictionary.TryGetValue(parent.PrefabModificationPath, out var value) && value.DictionaryKeysAdded != null && value.DictionaryKeysAdded.Contains(key))
						{
							return PrefabModificationType.Value;
						}
					}
				}
				return null;
			}
			registerModification = true;
			for (int j = 0; j < prefabValueModifications.Length; j++)
			{
				Dictionary<string, PrefabModification> dictionary2 = prefabValueModifications[j];
				if (dictionary2 == null)
				{
					continue;
				}
				InspectorProperty inspectorProperty = property;
				do
				{
					if (dictionary2.ContainsKey(inspectorProperty.PrefabModificationPath))
					{
						IPropertyValueEntry valueEntry = inspectorProperty.ValueEntry;
						if (valueEntry != null)
						{
							PrefabModification prefabModification = dictionary2[inspectorProperty.PrefabModificationPath];
							registerModification = valueEntry.ValueIsPrefabDifferent(prefabModification.ModifiedValue, j);
						}
						else
						{
							registerModification = false;
						}
						return PrefabModificationType.Value;
					}
					inspectorProperty = inspectorProperty.ParentValueProperty;
				}
				while (inspectorProperty != null && inspectorProperty.ValueEntry.TypeOfValue.IsValueType);
			}
			for (int k = 0; k < prefabListLengthModifications.Length; k++)
			{
				Dictionary<string, PrefabModification> dictionary3 = prefabListLengthModifications[k];
				if (dictionary3 != null && dictionary3.TryGetValue(property.PrefabModificationPath, out var value2))
				{
					registerModification = value2.NewLength != property.Children.Count;
					return PrefabModificationType.ListLength;
				}
			}
			for (int l = 0; l < prefabDictionaryModifications.Length; l++)
			{
				Dictionary<string, PrefabModification> dictionary4 = prefabDictionaryModifications[l];
				if (dictionary4 != null && dictionary4.ContainsKey(property.PrefabModificationPath))
				{
					registerModification = false;
					return PrefabModificationType.Dictionary;
				}
			}
			if (prefabPropertyTree == null || property.ValueEntry == null)
			{
				registerModification = false;
				return null;
			}
			InspectorProperty propertyAtPrefabModificationPath = prefabPropertyTree.GetPropertyAtPrefabModificationPath(property.PrefabModificationPath);
			if (propertyAtPrefabModificationPath == null || propertyAtPrefabModificationPath.ValueEntry == null)
			{
				return PrefabModificationType.Value;
			}
			if (!propertyAtPrefabModificationPath.ValueEntry.TypeOfValue.IsValueType && propertyAtPrefabModificationPath.ValueEntry.TypeOfValue != property.ValueEntry.TypeOfValue)
			{
				return PrefabModificationType.Value;
			}
			for (int m = 0; m < propertyAtPrefabModificationPath.ValueEntry.ValueCount; m++)
			{
				if (property.ValueEntry.ValueIsPrefabDifferent(propertyAtPrefabModificationPath.ValueEntry.WeakValues[m], m))
				{
					return PrefabModificationType.Value;
				}
			}
			if (propertyAtPrefabModificationPath.ValueEntry.TypeOfValue.IsValueType && !propertyAtPrefabModificationPath.ValueEntry.ValueTypeValuesAreEqual(property.ValueEntry))
			{
				return PrefabModificationType.Value;
			}
			if (typeof(Object).IsAssignableFrom(property.ValueEntry.TypeOfValue) && property.ValueEntry.WeakSmartValue != propertyAtPrefabModificationPath.ValueEntry.WeakSmartValue)
			{
				Object val = (Object)property.ValueEntry.WeakSmartValue;
				Object val2 = (Object)propertyAtPrefabModificationPath.ValueEntry.WeakSmartValue;
				if (val == (Object)null || val2 == (Object)null)
				{
					return PrefabModificationType.Value;
				}
				Object correspondingObjectFromSource = OdinPrefabSerializationEditorUtility.GetCorrespondingObjectFromSource(val);
				if (correspondingObjectFromSource != val2)
				{
					return PrefabModificationType.Value;
				}
			}
			if (propertyAtPrefabModificationPath.Children != null && property.Children != null && propertyAtPrefabModificationPath.ChildResolver is ICollectionResolver && propertyAtPrefabModificationPath.Children.Count != property.Children.Count)
			{
				if (propertyAtPrefabModificationPath.ChildResolver is IKeyValueMapResolver)
				{
					return PrefabModificationType.Dictionary;
				}
				return PrefabModificationType.ListLength;
			}
			registerModification = false;
			return null;
		}

		/// <summary>
		/// Registers a modification of type <see cref="F:Sirenix.Serialization.PrefabModificationType.ListLength" /> for a given property.
		/// </summary>
		/// <param name="property">The property to register a modification for.</param>
		/// <param name="targetIndex">Selection index of the target to register a modification for.</param>
		/// <param name="newLength">The modified list length.</param>
		/// <exception cref="T:System.ArgumentException">
		/// Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.
		/// or
		/// newLength cannot be negative!
		/// </exception>
		public void RegisterPrefabListLengthModification(InspectorProperty property, int targetIndex, int newLength)
		{
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Expected O, but got Unknown
			if (!targetSupportsPrefabSerialization)
			{
				Debug.LogError((object)string.Concat("Target of type ", Tree.TargetType, " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?"));
				return;
			}
			if (property.ValueEntry == null)
			{
				throw new ArgumentException("Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.");
			}
			if (!PropertyCanHaveModifications(property))
			{
				return;
			}
			Dictionary<string, PrefabModification> dictionary = prefabListLengthModifications[targetIndex];
			Dictionary<string, PrefabModification> valueMods = prefabValueModifications[targetIndex];
			Dictionary<string, PrefabModification> dictionaryMods = prefabDictionaryModifications[targetIndex];
			if (dictionary == null)
			{
				Debug.LogError((object)string.Concat("Target of type ", Tree.TargetType, " at index ", targetIndex, " is not a prefab!"));
				return;
			}
			if (newLength < 0)
			{
				throw new ArgumentException("newLength cannot be negative!");
			}
			try
			{
				allowAutoRegisterPrefabModifications = false;
				PrefabModification value = new PrefabModification
				{
					ModificationType = PrefabModificationType.ListLength,
					Path = property.PrefabModificationPath,
					NewLength = newLength
				};
				Update();
				RemovePrefabModificationsForInvalidIndices(property, dictionary, valueMods, dictionaryMods, newLength);
				dictionary[property.PrefabModificationPath] = value;
				UnitySerializationUtility.RegisterPrefabModificationsChange((Object)Tree.WeakTargets[targetIndex], GetPrefabModifications(targetIndex));
			}
			finally
			{
				allowAutoRegisterPrefabModifications = true;
			}
		}

		/// <summary>
		/// Registers a modification of type <see cref="F:Sirenix.Serialization.PrefabModificationType.Value" /> for a given property.
		/// </summary>
		/// <param name="property">The property to register a modification for.</param>
		/// <param name="targetIndex">Selection index of the target to register a modification for.</param>
		/// <param name="forceImmediate">Whether to force the change to be registered immediately, rather than at the end of frame.</param>
		/// <exception cref="T:System.ArgumentException">Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.</exception>
		public void RegisterPrefabValueModification(InspectorProperty property, int targetIndex, bool forceImmediate = false)
		{
			//IL_043c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0457: Expected O, but got Unknown
			if (!targetSupportsPrefabSerialization)
			{
				Debug.LogError((object)string.Concat("Target of type ", Tree.TargetType, " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?"));
				return;
			}
			if (property.ValueEntry == null)
			{
				throw new ArgumentException("Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.");
			}
			if (!PropertyCanHaveModifications(property))
			{
				return;
			}
			Dictionary<string, PrefabModification> valueMods = prefabValueModifications[targetIndex];
			Dictionary<string, PrefabModification> listLengthMods = prefabListLengthModifications[targetIndex];
			Dictionary<string, PrefabModification> dictionaryMods = prefabDictionaryModifications[targetIndex];
			if (valueMods == null)
			{
				Debug.LogError((object)string.Concat("Target of type ", Tree.TargetType, " at index ", targetIndex, " is not a prefab!"));
				return;
			}
			try
			{
				allowAutoRegisterPrefabModifications = false;
				string propPath = property.PrefabModificationPath;
				PrefabModification mod = new PrefabModification();
				Dictionary<string, PrefabModification> extraModChanges = null;
				property.Update(forceUpdate: true);
				mod.Path = propPath;
				mod.ModifiedValue = property.ValueEntry.WeakValues[targetIndex];
				object obj = property.ValueEntry.WeakValues[targetIndex];
				if (obj != null && !(obj is Object) && (property.ValueEntry.ValueState == PropertyValueState.Reference || (!property.BaseValueEntry.TypeOfValue.IsValueType && property.BaseValueEntry.TypeOfValue != typeof(string))))
				{
					mod.ReferencePaths = new List<string>();
					foreach (InspectorProperty item in Tree.EnumerateTree())
					{
						if (item.ValueEntry == null || item.Info.TypeOfValue.IsValueType || item.Path == property.Path)
						{
							continue;
						}
						item.Update(forceUpdate: true);
						if (obj != item.ValueEntry.WeakValues[targetIndex])
						{
							continue;
						}
						if (mod.ReferencePaths.Count < 5)
						{
							mod.ReferencePaths.Add(item.PrefabModificationPath);
						}
						if (!valueMods.TryGetValue(item.PrefabModificationPath, out var value) || (value.ReferencePaths != null && value.ReferencePaths.Contains(property.PrefabModificationPath)))
						{
							continue;
						}
						if (value.ReferencePaths == null)
						{
							value.ReferencePaths = new List<string>();
						}
						if (value.ReferencePaths.Count < 5)
						{
							value.ReferencePaths.Add(property.PrefabModificationPath);
							if (extraModChanges == null)
							{
								extraModChanges = new Dictionary<string, PrefabModification>();
							}
							extraModChanges[item.PrefabModificationPath] = value;
						}
					}
				}
				else if (obj is Object)
				{
					mod.ReferencePaths = null;
				}
				if (forceImmediate)
				{
					Update();
					RemoveInvalidPrefabModifications("", listLengthMods, valueMods, dictionaryMods);
					valueMods[propPath] = mod;
					listLengthMods.Remove(propPath);
					dictionaryMods.Remove(propPath);
					if (extraModChanges != null)
					{
						foreach (KeyValuePair<string, PrefabModification> item2 in extraModChanges)
						{
							valueMods[item2.Key] = item2.Value;
						}
					}
					UnitySerializationUtility.RegisterPrefabModificationsChange((Object)Tree.WeakTargets[targetIndex], GetPrefabModifications(targetIndex));
					return;
				}
				Tree.DelayAction(delegate
				{
					//IL_011f: Unknown result type (might be due to invalid IL or missing references)
					//IL_0144: Expected O, but got Unknown
					Update();
					Tree.UpdateTree();
					RemoveInvalidPrefabModifications("", listLengthMods, valueMods, dictionaryMods);
					valueMods[propPath] = mod;
					listLengthMods.Remove(propPath);
					dictionaryMods.Remove(propPath);
					if (extraModChanges != null)
					{
						foreach (KeyValuePair<string, PrefabModification> item3 in extraModChanges)
						{
							valueMods[item3.Key] = item3.Value;
						}
					}
					UnitySerializationUtility.RegisterPrefabModificationsChange((Object)Tree.WeakTargets[targetIndex], GetPrefabModifications(targetIndex));
				});
			}
			finally
			{
				allowAutoRegisterPrefabModifications = true;
			}
		}

		/// <summary>
		/// Calculates a delta between the current dictionary property and its prefab counterpart, and registers that delta as a <see cref="F:Sirenix.Serialization.PrefabModificationType.Dictionary" /> modification.
		/// </summary>
		/// <param name="property">The property to register a modification for.</param>
		/// <param name="targetIndex">Selection index of the target.</param>
		/// <exception cref="T:System.ArgumentException">Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.</exception>
		public void RegisterPrefabDictionaryDeltaModification(InspectorProperty property, int targetIndex)
		{
			//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_02be: Expected O, but got Unknown
			if (!targetSupportsPrefabSerialization)
			{
				Debug.LogError((object)string.Concat("Target of type ", Tree.TargetType, " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?"));
				return;
			}
			if (property.ValueEntry == null)
			{
				throw new ArgumentException("Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.");
			}
			if (!PropertyCanHaveModifications(property))
			{
				return;
			}
			Dictionary<string, PrefabModification> dictionary = prefabValueModifications[targetIndex];
			Dictionary<string, PrefabModification> listLengthMods = prefabListLengthModifications[targetIndex];
			Dictionary<string, PrefabModification> dictionary2 = prefabDictionaryModifications[targetIndex];
			if (dictionary == null)
			{
				Debug.LogError((object)string.Concat("Target of type ", Tree.TargetType, " at index ", targetIndex, " is not a prefab!"));
				return;
			}
			try
			{
				allowAutoRegisterPrefabModifications = false;
				string prefabModificationPath = property.PrefabModificationPath;
				if (dictionary.TryGetValue(prefabModificationPath, out var value))
				{
					RegisterPrefabValueModification(property, targetIndex);
					return;
				}
				bool flag = false;
				if (!dictionary2.TryGetValue(prefabModificationPath, out value))
				{
					flag = true;
					value = new PrefabModification();
					value.ModificationType = PrefabModificationType.Dictionary;
					value.Path = prefabModificationPath;
				}
				InspectorProperty propertyAtPath = prefabPropertyTree.GetPropertyAtPath(property.Path);
				if (propertyAtPath == null)
				{
					return;
				}
				int index = prefabPropertyTreeIndexMap[targetIndex];
				IDictionary dictionary3 = propertyAtPath.ValueEntry.WeakValues[index] as IDictionary;
				IDictionary dictionary4 = property.ValueEntry.WeakValues[targetIndex] as IDictionary;
				if (dictionary3 == null || dictionary4 == null)
				{
					return;
				}
				foreach (object key in dictionary3.Keys)
				{
					if (!dictionary4.Contains(key))
					{
						if (value.DictionaryKeysRemoved == null)
						{
							value.DictionaryKeysRemoved = new object[1] { key };
						}
						else
						{
							value.DictionaryKeysRemoved = ArrayUtilities.CreateNewArrayWithAddedElement(value.DictionaryKeysRemoved, key);
						}
					}
				}
				foreach (object key2 in dictionary4.Keys)
				{
					if (!dictionary3.Contains(key2))
					{
						if (value.DictionaryKeysAdded == null)
						{
							value.DictionaryKeysAdded = new object[1] { key2 };
						}
						else
						{
							value.DictionaryKeysAdded = ArrayUtilities.CreateNewArrayWithAddedElement(value.DictionaryKeysAdded, key2);
						}
					}
				}
				if (!flag || (value.DictionaryKeysAdded != null && value.DictionaryKeysAdded.Length != 0) || (value.DictionaryKeysRemoved != null && value.DictionaryKeysRemoved.Length != 0))
				{
					Update();
					RemoveInvalidPrefabModifications("", listLengthMods, dictionary, dictionary2);
					dictionary2[prefabModificationPath] = value;
					UnitySerializationUtility.RegisterPrefabModificationsChange((Object)Tree.WeakTargets[targetIndex], GetPrefabModifications(targetIndex));
				}
			}
			finally
			{
				allowAutoRegisterPrefabModifications = true;
			}
		}

		/// <summary>
		/// Adds a remove key modification to the dictionary modifications of a given property.
		/// </summary>
		/// <param name="property">The property to register a modification for.</param>
		/// <param name="targetIndex">Selection index of the target.</param>
		/// <param name="key">The key to be removed.</param>
		/// <exception cref="T:System.ArgumentException">Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.</exception>
		public void RegisterPrefabDictionaryRemoveKeyModification(InspectorProperty property, int targetIndex, object key)
		{
			//IL_0199: Unknown result type (might be due to invalid IL or missing references)
			//IL_01aa: Expected O, but got Unknown
			if (!targetSupportsPrefabSerialization)
			{
				Debug.LogError((object)string.Concat("Target of type ", Tree.TargetType, " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?"));
				return;
			}
			if (property.ValueEntry == null)
			{
				throw new ArgumentException("Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.");
			}
			if (!PropertyCanHaveModifications(property))
			{
				return;
			}
			Dictionary<string, PrefabModification> dictionary = prefabValueModifications[targetIndex];
			Dictionary<string, PrefabModification> listLengthMods = prefabListLengthModifications[targetIndex];
			Dictionary<string, PrefabModification> dictionary2 = prefabDictionaryModifications[targetIndex];
			if (dictionary == null)
			{
				Debug.LogError((object)string.Concat("Target of type ", Tree.TargetType, " at index ", targetIndex, " is not a prefab!"));
				return;
			}
			try
			{
				allowAutoRegisterPrefabModifications = false;
				string prefabModificationPath = property.PrefabModificationPath;
				if (!dictionary2.TryGetValue(prefabModificationPath, out var value))
				{
					value = new PrefabModification();
					value.ModificationType = PrefabModificationType.Dictionary;
					value.Path = prefabModificationPath;
				}
				bool flag = true;
				if (value.DictionaryKeysAdded != null)
				{
					for (int i = 0; i < value.DictionaryKeysAdded.Length; i++)
					{
						if (key.Equals(value.DictionaryKeysAdded[i]))
						{
							value.DictionaryKeysAdded = ArrayUtilities.CreateNewArrayWithRemovedElement(value.DictionaryKeysAdded, i);
							flag = false;
							i--;
						}
					}
				}
				if (flag)
				{
					if (value.DictionaryKeysRemoved == null)
					{
						value.DictionaryKeysRemoved = new object[1] { key };
					}
					else
					{
						value.DictionaryKeysRemoved = ArrayUtilities.CreateNewArrayWithAddedElement(value.DictionaryKeysRemoved, key);
					}
				}
				Update();
				RemoveInvalidPrefabModifications("", listLengthMods, dictionary, dictionary2);
				dictionary2[prefabModificationPath] = value;
				UnitySerializationUtility.RegisterPrefabModificationsChange((Object)Tree.WeakTargets[targetIndex], GetPrefabModifications(targetIndex));
			}
			finally
			{
				allowAutoRegisterPrefabModifications = true;
			}
		}

		/// <summary>
		/// Adds an add key modification to the dictionary modifications of a given property.
		/// </summary>
		/// <param name="property">The property to register a modification for.</param>
		/// <param name="targetIndex">Selection index of the target.</param>
		/// <param name="key">The key to be added.</param>
		/// <exception cref="T:System.ArgumentException">Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.</exception>
		public void RegisterPrefabDictionaryAddKeyModification(InspectorProperty property, int targetIndex, object key)
		{
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Expected O, but got Unknown
			if (!targetSupportsPrefabSerialization)
			{
				Debug.LogError((object)string.Concat("Target of type ", Tree.TargetType, " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?"));
				return;
			}
			if (property.ValueEntry == null)
			{
				throw new ArgumentException("Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.");
			}
			if (!PropertyCanHaveModifications(property))
			{
				return;
			}
			Dictionary<string, PrefabModification> dictionary = prefabValueModifications[targetIndex];
			Dictionary<string, PrefabModification> listLengthMods = prefabListLengthModifications[targetIndex];
			Dictionary<string, PrefabModification> dictionary2 = prefabDictionaryModifications[targetIndex];
			if (dictionary == null)
			{
				Debug.LogError((object)string.Concat("Target of type ", Tree.TargetType, " at index ", targetIndex, " is not a prefab!"));
				return;
			}
			try
			{
				allowAutoRegisterPrefabModifications = false;
				string prefabModificationPath = property.PrefabModificationPath;
				if (!dictionary2.TryGetValue(prefabModificationPath, out var value))
				{
					value = new PrefabModification();
					value.ModificationType = PrefabModificationType.Dictionary;
					value.Path = prefabModificationPath;
				}
				if (value.DictionaryKeysAdded == null)
				{
					value.DictionaryKeysAdded = new object[1] { key };
				}
				else
				{
					value.DictionaryKeysAdded = ArrayUtilities.CreateNewArrayWithAddedElement(value.DictionaryKeysAdded, key);
				}
				if (value.DictionaryKeysRemoved != null)
				{
					for (int i = 0; i < value.DictionaryKeysRemoved.Length; i++)
					{
						if (key.Equals(value.DictionaryKeysRemoved[i]))
						{
							value.DictionaryKeysRemoved = ArrayUtilities.CreateNewArrayWithRemovedElement(value.DictionaryKeysRemoved, i);
							i--;
						}
					}
				}
				Update();
				RemoveInvalidPrefabModifications("", listLengthMods, dictionary, dictionary2);
				dictionary2[prefabModificationPath] = value;
				UnitySerializationUtility.RegisterPrefabModificationsChange((Object)Tree.WeakTargets[targetIndex], GetPrefabModifications(targetIndex));
			}
			finally
			{
				allowAutoRegisterPrefabModifications = true;
			}
		}

		/// <summary>
		/// Removes all dictionary modifications on a property for a given dictionary key value.
		/// </summary>
		/// <param name="property">The property to remove a key modification for.</param>
		/// <param name="targetIndex">Selection index of the target.</param>
		/// <param name="key">The key to remove modifications for.</param>
		/// <exception cref="T:System.ArgumentNullException">key</exception>
		public void RemovePrefabDictionaryModification(InspectorProperty property, int targetIndex, object key)
		{
			//IL_028c: Unknown result type (might be due to invalid IL or missing references)
			//IL_029d: Expected O, but got Unknown
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (property.ValueEntry == null || !typeof(Object).IsAssignableFrom(Tree.TargetType))
			{
				return;
			}
			if (!targetSupportsPrefabSerialization)
			{
				Debug.LogError((object)string.Concat("Target of type ", Tree.TargetType, " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?"));
				return;
			}
			Dictionary<string, PrefabModification> dictionary = prefabListLengthModifications[targetIndex];
			Dictionary<string, PrefabModification> dictionary2 = prefabValueModifications[targetIndex];
			Dictionary<string, PrefabModification> dictionary3 = prefabDictionaryModifications[targetIndex];
			if (dictionary == null || dictionary2 == null || dictionary3 == null)
			{
				Debug.LogError((object)string.Concat("Target of type ", Tree.TargetType, " at index ", targetIndex, " is not a prefab!"));
				return;
			}
			try
			{
				allowAutoRegisterPrefabModifications = false;
				bool flag = false;
				string prefabModificationPath = property.PrefabModificationPath;
				if (dictionary3.TryGetValue(prefabModificationPath, out var value))
				{
					if (value.DictionaryKeysRemoved != null)
					{
						for (int i = 0; i < value.DictionaryKeysRemoved.Length; i++)
						{
							if (key.Equals(value.DictionaryKeysRemoved[i]))
							{
								flag = true;
								value.DictionaryKeysRemoved = ArrayUtilities.CreateNewArrayWithRemovedElement(value.DictionaryKeysRemoved, i);
								i--;
							}
						}
					}
					if (value.DictionaryKeysAdded != null)
					{
						for (int j = 0; j < value.DictionaryKeysAdded.Length; j++)
						{
							if (key.Equals(value.DictionaryKeysAdded[j]))
							{
								flag = true;
								value.DictionaryKeysAdded = ArrayUtilities.CreateNewArrayWithRemovedElement(value.DictionaryKeysAdded, j);
								j--;
							}
						}
						if (flag)
						{
							string value2 = prefabModificationPath + "." + DictionaryKeyUtility.GetDictionaryKeyString(key);
							HashSet<string> hashSet = new HashSet<string>();
							foreach (string item in dictionary.Keys.AppendWith(dictionary2.Keys).AppendWith(dictionary3.Keys))
							{
								if (item.StartsWith(value2, StringComparison.InvariantCulture))
								{
									hashSet.Add(item);
								}
							}
							foreach (string item2 in hashSet)
							{
								dictionary.Remove(item2);
								dictionary2.Remove(item2);
								dictionary3.Remove(item2);
							}
						}
					}
					if ((value.DictionaryKeysRemoved == null || value.DictionaryKeysRemoved.Length == 0) && (value.DictionaryKeysRemoved == null || value.DictionaryKeysRemoved.Length == 0))
					{
						dictionary3.Remove(prefabModificationPath);
					}
					flag = true;
				}
				if (flag)
				{
					UnitySerializationUtility.RegisterPrefabModificationsChange((Object)Tree.WeakTargets[targetIndex], GetPrefabModifications(targetIndex));
				}
			}
			finally
			{
				allowAutoRegisterPrefabModifications = true;
			}
		}

		/// <summary>
		/// Removes all prefab modifications of a given type on a given property.
		/// </summary>
		/// <param name="property">The property to remove modifications for.</param>
		/// <param name="targetIndex">Selection index of the target.</param>
		/// <param name="modificationType">Type of the modification to remove.</param>
		public void RemovePrefabModification(InspectorProperty property, int targetIndex, PrefabModificationType modificationType)
		{
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Expected O, but got Unknown
			//IL_04c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_04d4: Expected O, but got Unknown
			if (property.ValueEntry == null || !typeof(Object).IsAssignableFrom(Tree.TargetType))
			{
				return;
			}
			try
			{
				allowAutoRegisterPrefabModifications = false;
				if (property.ValueEntry.SerializationBackend.IsUnity)
				{
					Object val = (Object)Tree.WeakTargets[targetIndex];
					Object val2 = TargetPrefabs[targetIndex];
					List<PropertyModification> list = PrefabUtility.GetPropertyModifications(val).ToList();
					switch (modificationType)
					{
					case PrefabModificationType.Value:
					{
						for (int j = 0; j < list.Count; j++)
						{
							PropertyModification val4 = list[j];
							if (val4.target == val2 && val4.propertyPath.StartsWith(property.UnityPropertyPath, StringComparison.InvariantCulture))
							{
								list.RemoveAt(j);
								j--;
							}
						}
						break;
					}
					case PrefabModificationType.ListLength:
					{
						string text = property.UnityPropertyPath + ".Array.size";
						for (int i = 0; i < list.Count; i++)
						{
							PropertyModification val3 = list[i];
							if (val3.target == val2 && val3.propertyPath == text)
							{
								list.RemoveAt(i);
								i--;
							}
						}
						RemovePrefabModificationsForInvalidIndices(property, val2, list);
						break;
					}
					}
					PrefabUtility.SetPropertyModifications(val, list.ToArray());
					string currentGroupName = Undo.GetCurrentGroupName();
					Undo.FlushUndoRecordObjects();
					Tree.RootProperty.RecordForUndo();
					PrefabUtility.SetPropertyModifications(val, list.ToArray());
				}
				else
				{
					if (property.ValueEntry.SerializationBackend != SerializationBackend.Odin)
					{
						return;
					}
					if (!targetSupportsPrefabSerialization)
					{
						Debug.LogError((object)string.Concat("Target of type ", Tree.TargetType, " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?"));
						return;
					}
					Dictionary<string, PrefabModification> dictionary = prefabListLengthModifications[targetIndex];
					Dictionary<string, PrefabModification> dictionary2 = prefabValueModifications[targetIndex];
					Dictionary<string, PrefabModification> dictionary3 = prefabDictionaryModifications[targetIndex];
					if (dictionary == null || dictionary2 == null || dictionary3 == null)
					{
						Debug.LogError((object)string.Concat("Target of type ", Tree.TargetType, " at index ", targetIndex, " is not a prefab!"));
						return;
					}
					string prefabModificationPath = property.PrefabModificationPath;
					bool flag = false;
					PrefabModification value2;
					if (modificationType == PrefabModificationType.Value && dictionary2.ContainsKey(prefabModificationPath))
					{
						Update();
						dictionary2.Remove(prefabModificationPath);
						string value = prefabModificationPath + ".";
						HashSet<string> hashSet = new HashSet<string>();
						foreach (string item in dictionary.Keys.AppendWith(dictionary2.Keys).AppendWith(dictionary3.Keys))
						{
							if (item.StartsWith(value, StringComparison.InvariantCulture))
							{
								hashSet.Add(item);
							}
						}
						foreach (string item2 in hashSet)
						{
							dictionary.Remove(item2);
							dictionary2.Remove(item2);
							dictionary3.Remove(item2);
						}
						flag = true;
					}
					else if (modificationType == PrefabModificationType.ListLength && dictionary.TryGetValue(prefabModificationPath, out value2))
					{
						Update();
						dictionary.Remove(prefabModificationPath);
						InspectorProperty propertyAtPath = prefabPropertyTree.GetPropertyAtPath(property.Path);
						if (propertyAtPath != null)
						{
							int newLength = (propertyAtPath.ChildResolver as ICollectionResolver)?.MaxCollectionLength ?? propertyAtPath.Children.Count;
							RemovePrefabModificationsForInvalidIndices(property, dictionary, dictionary2, dictionary3, newLength);
						}
						flag = true;
					}
					else if (modificationType == PrefabModificationType.Dictionary && dictionary3.TryGetValue(prefabModificationPath, out value2))
					{
						Update();
						dictionary3.Remove(prefabModificationPath);
						if (value2.DictionaryKeysAdded != null)
						{
							HashSet<string> hashSet2 = new HashSet<string>();
							for (int k = 0; k < value2.DictionaryKeysAdded.Length; k++)
							{
								string dictionaryKeyString = DictionaryKeyUtility.GetDictionaryKeyString(value2.DictionaryKeysAdded[k]);
								string value3 = prefabModificationPath + "." + dictionaryKeyString;
								foreach (string item3 in dictionary.Keys.AppendWith(dictionary2.Keys).AppendWith(dictionary3.Keys))
								{
									if (item3.StartsWith(value3, StringComparison.InvariantCulture))
									{
										hashSet2.Add(item3);
									}
								}
							}
							foreach (string item4 in hashSet2)
							{
								dictionary.Remove(item4);
								dictionary2.Remove(item4);
								dictionary3.Remove(item4);
							}
						}
						flag = true;
					}
					if (flag)
					{
						UnitySerializationUtility.RegisterPrefabModificationsChange((Object)Tree.WeakTargets[targetIndex], GetPrefabModifications(targetIndex));
					}
					return;
				}
			}
			finally
			{
				allowAutoRegisterPrefabModifications = true;
			}
		}

		private void RemoveInvalidPrefabModifications(string startPath, Dictionary<string, PrefabModification> listLengthMods, Dictionary<string, PrefabModification> valueMods, Dictionary<string, PrefabModification> dictionaryMods)
		{
			HashSet<string> hashSet = new HashSet<string>();
			foreach (string item in listLengthMods.Keys.AppendWith(valueMods.Keys).AppendWith(dictionaryMods.Keys))
			{
				if (item.StartsWith(startPath))
				{
					InspectorProperty propertyAtPrefabModificationPath = Tree.GetPropertyAtPrefabModificationPath(item);
					if (propertyAtPrefabModificationPath == null || !propertyAtPrefabModificationPath.SupportsPrefabModifications)
					{
						hashSet.Add(item);
					}
				}
			}
			foreach (string item2 in hashSet)
			{
				listLengthMods.Remove(item2);
				valueMods.Remove(item2);
				dictionaryMods.Remove(item2);
			}
		}

		private void RemovePrefabModificationsForInvalidIndices(InspectorProperty property, Dictionary<string, PrefabModification> listLengthMods, Dictionary<string, PrefabModification> valueMods, Dictionary<string, PrefabModification> dictionaryMods, int newLength)
		{
			string prefabModificationPath = property.PrefabModificationPath;
			HashSet<string> hashSet = new HashSet<string>();
			string text = prefabModificationPath + ".[";
			foreach (string item in listLengthMods.Keys.AppendWith(valueMods.Keys).AppendWith(dictionaryMods.Keys))
			{
				if (!item.StartsWith(text, StringComparison.InvariantCulture))
				{
					continue;
				}
				int num = item.IndexOf("]", text.Length, StringComparison.InvariantCulture);
				if (num > text.Length)
				{
					string s = item.Substring(text.Length, num - text.Length);
					if (int.TryParse(s, out var result) && result >= newLength)
					{
						hashSet.Add(item);
					}
				}
			}
			foreach (string item2 in hashSet)
			{
				listLengthMods.Remove(item2);
				valueMods.Remove(item2);
				dictionaryMods.Remove(item2);
			}
		}

		private void RemovePrefabModificationsForInvalidIndices(InspectorProperty property, Object prefab, List<PropertyModification> unityMods)
		{
			string text = property.UnityPropertyPath + ".Array.data[";
			InspectorProperty propertyAtPath = prefabPropertyTree.GetPropertyAtPath(property.Path);
			if (propertyAtPath == null)
			{
				return;
			}
			HashSet<string> hashSet = new HashSet<string>();
			int num = (propertyAtPath.ChildResolver as ICollectionResolver)?.MaxCollectionLength ?? propertyAtPath.Children.Count;
			if (num < property.Children.Count)
			{
				foreach (PropertyModification unityMod in unityMods)
				{
					string propertyPath = unityMod.propertyPath;
					if (!propertyPath.StartsWith(text, StringComparison.InvariantCulture))
					{
						continue;
					}
					int num2 = propertyPath.IndexOf("]", text.Length, StringComparison.InvariantCulture);
					if (num2 > text.Length)
					{
						string s = propertyPath.Substring(text.Length, num2 - text.Length);
						if (int.TryParse(s, out var result) && result >= num)
						{
							hashSet.Add(propertyPath);
						}
					}
				}
			}
			for (int i = 0; i < unityMods.Count; i++)
			{
				PropertyModification val = unityMods[i];
				if (val.target == prefab && hashSet.Contains(val.propertyPath))
				{
					unityMods.RemoveAt(i);
					i--;
				}
			}
		}

		/// <summary>
		/// Gets all prefab modifications in this property tree for a given selection index.
		/// </summary>
		/// <param name="targetIndex"></param>
		/// <returns></returns>
		public List<PrefabModification> GetPrefabModifications(int targetIndex)
		{
			if (!targetSupportsPrefabSerialization)
			{
				return new List<PrefabModification>();
			}
			Dictionary<string, PrefabModification> dictionary = prefabValueModifications[targetIndex];
			Dictionary<string, PrefabModification> dictionary2 = prefabListLengthModifications[targetIndex];
			Dictionary<string, PrefabModification> dictionary3 = prefabDictionaryModifications[targetIndex];
			if (dictionary == null || dictionary2 == null || dictionary3 == null)
			{
				return new List<PrefabModification>();
			}
			return dictionary2.Values.AppendWith(dictionary.Values).AppendWith(dictionary3.Values).ToList();
		}
	}
}
