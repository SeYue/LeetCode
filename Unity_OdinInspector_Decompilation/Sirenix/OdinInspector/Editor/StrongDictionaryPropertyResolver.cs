using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;

namespace Sirenix.OdinInspector.Editor
{
	[ResolverPriority(-1.0)]
	public class StrongDictionaryPropertyResolver<TDictionary, TKey, TValue> : BaseKeyValueMapResolver<TDictionary>, IHasSpecialPropertyPaths, IPathRedirector, IMaySupportPrefabModifications where TDictionary : IDictionary<TKey, TValue>
	{
		private struct TempKeyInfo
		{
			public TKey Key;

			public bool IsInvalid;
		}

		private int lastUpdateID;

		private Dictionary<int, InspectorPropertyInfo> childInfos = new Dictionary<int, InspectorPropertyInfo>();

		private Dictionary<int, TempKeyInfo> tempKeys = new Dictionary<int, TempKeyInfo>();

		private Dictionary<TDictionary, int> dictIndexMap = new Dictionary<TDictionary, int>();

		private List<TKey>[] keys;

		private List<TKey>[] oldKeys;

		private List<Attribute> childAttrs;

		private static readonly bool KeyTypeSupportsPersistentPaths = DictionaryKeyUtility.KeyTypeSupportsPersistentPaths(typeof(TKey));

		public bool ValueApplyIsTemporary;

		public bool MaySupportPrefabModifications => KeyTypeSupportsPersistentPaths;

		public override Type ElementType => typeof(EditableKeyValuePair<TKey, TValue>);

		protected override void Initialize()
		{
			base.Initialize();
			keys = new List<TKey>[base.Property.Tree.WeakTargets.Count];
			oldKeys = new List<TKey>[base.Property.Tree.WeakTargets.Count];
			for (int i = 0; i < keys.Length; i++)
			{
				keys[i] = new List<TKey>();
				oldKeys[i] = new List<TKey>();
			}
			ImmutableList<Attribute> attributes = base.Property.Attributes;
			List<Attribute> list = new List<Attribute>(attributes.Count);
			for (int j = 0; j < attributes.Count; j++)
			{
				Attribute attribute = attributes[j];
				if (!attribute.GetType().IsDefined(typeof(DontApplyToListElementsAttribute), inherit: true))
				{
					list.Add(attribute);
				}
			}
			childAttrs = list;
		}

		public bool TryGetRedirectedProperty(string childName, out InspectorProperty property)
		{
			EnsureUpdated();
			property = null;
			if (childName.Length == 0 || childName[0] != '{')
			{
				return false;
			}
			try
			{
				bool flag = FastEndsWith(childName, "#key");
				if (flag)
				{
					childName = childName.Substring(0, childName.Length - 4);
				}
				TKey arg = (TKey)DictionaryKeyUtility.GetDictionaryKeyValue(childName, typeof(TKey));
				List<TKey> list = keys[0];
				for (int i = 0; i < list.Count; i++)
				{
					if (PropertyValueEntry<TKey>.EqualityComparer(arg, list[i]))
					{
						property = (flag ? base.Property.Children[i].Children["Key"] : base.Property.Children[i].Children["Value"]);
						return true;
					}
				}
			}
			catch (Exception)
			{
				return false;
			}
			return false;
		}

		public string GetSpecialChildPath(int childIndex)
		{
			EnsureUpdated();
			List<TKey> list = keys[0];
			if (childIndex >= list.Count)
			{
				Update();
				list = keys[0];
			}
			TKey val = keys[0][childIndex];
			return base.Property.Path + "." + DictionaryKeyUtility.GetDictionaryKeyString(val) + "#entry";
		}

		public override object GetKey(int selectionIndex, int childIndex)
		{
			EnsureUpdated();
			return keys[selectionIndex][childIndex];
		}

		public override InspectorPropertyInfo GetChildInfo(int childIndex)
		{
			EnsureUpdated();
			if (!childInfos.TryGetValue(childIndex, out var value))
			{
				value = InspectorPropertyInfo.CreateValue(CollectionResolverUtilities.DefaultIndexToChildName(childIndex), childIndex, base.Property.BaseValueEntry.SerializationBackend, new GetterSetter<TDictionary, EditableKeyValuePair<TKey, TValue>>(CreateGetter(childIndex), CreateSetter(childIndex)), childAttrs);
				childInfos[childIndex] = value;
			}
			return value;
		}

		private ValueGetter<TDictionary, EditableKeyValuePair<TKey, TValue>> CreateGetter(int childIndex)
		{
			return delegate(ref TDictionary dict)
			{
				EnsureUpdated();
				List<TKey> list = keys[dictIndexMap[dict]];
				if (childIndex >= list.Count)
				{
					Update();
					list = keys[dictIndexMap[dict]];
				}
				TKey val = list[childIndex];
				dict.TryGetValue(val, out var value);
				TempKeyInfo value2;
				bool flag = tempKeys.TryGetValue(childIndex, out value2);
				return new EditableKeyValuePair<TKey, TValue>(flag ? value2.Key : val, value, flag && value2.IsInvalid, flag);
			};
		}

		private ValueSetter<TDictionary, EditableKeyValuePair<TKey, TValue>> CreateSetter(int childIndex)
		{
			return delegate(ref TDictionary dict, EditableKeyValuePair<TKey, TValue> value)
			{
				EnsureUpdated();
				int num = dictIndexMap[dict];
				List<TKey> list = keys[num];
				if (childIndex >= list.Count)
				{
					Update();
					num = dictIndexMap[dict];
					list = keys[num];
				}
				TKey val = list[childIndex];
				dict.TryGetValue(val, out var value2);
				TKey key = value.Key;
				TValue value3 = value.Value;
				bool flag = PropertyValueEntry<TKey>.EqualityComparer(val, key);
				CollectionChangeInfo collectionChangeInfo;
				if (!flag)
				{
					TempKeyInfo value4;
					if (dict.ContainsKey(key))
					{
						Dictionary<int, TempKeyInfo> dictionary = tempKeys;
						int key2 = childIndex;
						value4 = new TempKeyInfo
						{
							Key = key,
							IsInvalid = true
						};
						dictionary[key2] = value4;
					}
					else if (!ValueApplyIsTemporary)
					{
						bool flag2 = base.Property.SupportsPrefabModifications && base.ValueEntry.SerializationBackend == SerializationBackend.Odin;
						tempKeys.Remove(childIndex);
						collectionChangeInfo = default(CollectionChangeInfo);
						collectionChangeInfo.ChangeType = CollectionChangeType.RemoveKey;
						collectionChangeInfo.Key = val;
						collectionChangeInfo.SelectionIndex = num;
						CollectionChangeInfo info = collectionChangeInfo;
						InvokeOnBeforeChange(info);
						dict.Remove(val);
						InvokeOnAfterChange(info);
						collectionChangeInfo = default(CollectionChangeInfo);
						collectionChangeInfo.ChangeType = CollectionChangeType.SetKey;
						collectionChangeInfo.Key = key;
						collectionChangeInfo.Value = value3;
						collectionChangeInfo.SelectionIndex = dictIndexMap[dict];
						CollectionChangeInfo info2 = collectionChangeInfo;
						InvokeOnBeforeChange(info2);
						dict.Add(key, value3);
						InvokeOnAfterChange(info2);
						if (flag2)
						{
							for (int i = 0; i < base.Property.Tree.WeakTargets.Count; i++)
							{
								base.Property.Tree.PrefabModificationHandler.RegisterPrefabDictionaryRemoveKeyModification(base.Property, i, val);
								base.Property.Tree.PrefabModificationHandler.RegisterPrefabDictionaryAddKeyModification(base.Property, i, key);
							}
						}
						childInfos.Clear();
						base.Property.Children.ClearAndDisposeChildren();
						if (flag2)
						{
							Update();
							string dictionaryKeyString = DictionaryKeyUtility.GetDictionaryKeyString(key);
							InspectorProperty inspectorProperty = base.Property.Children[dictionaryKeyString];
							inspectorProperty.Update(forceUpdate: true);
							foreach (InspectorProperty item in inspectorProperty.Children.Recurse())
							{
								item.Update(forceUpdate: true);
							}
						}
					}
					else
					{
						Dictionary<int, TempKeyInfo> dictionary2 = tempKeys;
						int key3 = childIndex;
						value4 = new TempKeyInfo
						{
							Key = key,
							IsInvalid = false
						};
						dictionary2[key3] = value4;
					}
				}
				else if (!PropertyValueEntry<TValue>.EqualityComparer(value2, value3))
				{
					collectionChangeInfo = default(CollectionChangeInfo);
					collectionChangeInfo.ChangeType = CollectionChangeType.SetKey;
					collectionChangeInfo.Key = key;
					collectionChangeInfo.Value = value3;
					collectionChangeInfo.SelectionIndex = dictIndexMap[dict];
					CollectionChangeInfo info3 = collectionChangeInfo;
					InvokeOnBeforeChange(info3);
					dict[key] = value3;
					InvokeOnAfterChange(info3);
				}
				if (value.IsTempKey && flag)
				{
					tempKeys.Remove(childIndex);
				}
			};
		}

		protected override int GetChildCount(TDictionary value)
		{
			return value.Count;
		}

		protected override void OnCollectionChangesApplied()
		{
			base.OnCollectionChangesApplied();
			if (base.Property.SupportsPrefabModifications && base.Property.ValueEntry.SerializationBackend == SerializationBackend.Odin)
			{
				int count = base.Property.Tree.WeakTargets.Count;
				for (int i = 0; i < count; i++)
				{
					base.Property.Tree.PrefabModificationHandler.RegisterPrefabDictionaryDeltaModification(base.Property, i);
				}
			}
		}

		protected override void Add(TDictionary collection, object value)
		{
			KeyValuePair<TKey, TValue> item = (KeyValuePair<TKey, TValue>)value;
			collection.Add(item);
			HandleAddSetPrefabValueModification(item.Key);
		}

		protected override void Remove(TDictionary collection, object value)
		{
			collection.Remove((KeyValuePair<TKey, TValue>)value);
		}

		protected override void RemoveKey(TDictionary map, object key)
		{
			map.Remove((TKey)key);
		}

		protected override void Set(TDictionary map, object key, object value)
		{
			map[(TKey)key] = (TValue)value;
			HandleAddSetPrefabValueModification(key);
		}

		protected override void Clear(TDictionary collection)
		{
			collection.Clear();
		}

		protected override bool CollectionIsReadOnly(TDictionary collection)
		{
			return collection.IsReadOnly;
		}

		private void HandleAddSetPrefabValueModification(object key)
		{
			if (!base.Property.SupportsPrefabModifications || base.Property.ValueEntry.SerializationBackend != SerializationBackend.Odin)
			{
				return;
			}
			Update();
			int count = base.Property.Tree.WeakTargets.Count;
			for (int i = 0; i < count; i++)
			{
				base.Property.Tree.PrefabModificationHandler.RegisterPrefabDictionaryAddKeyModification(base.Property, i, key);
				InspectorProperty inspectorProperty = base.Property.Children[DictionaryKeyUtility.GetDictionaryKeyString(key)];
				if (inspectorProperty != null)
				{
					base.Property.Tree.PrefabModificationHandler.RegisterPrefabValueModification(inspectorProperty, i, forceImmediate: true);
				}
			}
		}

		private void EnsureUpdated()
		{
			if (base.Property.Tree.UpdateID != lastUpdateID)
			{
				Update();
			}
		}

		private void Update()
		{
			dictIndexMap.Clear();
			lastUpdateID = base.Property.Tree.UpdateID;
			for (int i = 0; i < keys.Length; i++)
			{
				List<TKey> list = keys[i];
				List<TKey> list2 = oldKeys[i];
				oldKeys[i] = list;
				keys[i] = list2;
				list2.Clear();
				TDictionary val = (TDictionary)base.Property.ValueEntry.WeakValues[i];
				if (val == null)
				{
					continue;
				}
				dictIndexMap[val] = i;
				Dictionary<TKey, TValue> dictionary = val as Dictionary<TKey, TValue>;
				if (dictionary != null)
				{
					foreach (KeyValuePair<TKey, TValue> item in dictionary.GFIterator())
					{
						list2.Add(item.Key);
					}
				}
				else
				{
					foreach (TKey key in val.Keys)
					{
						list2.Add(key);
					}
				}
				if (list2.Count > 1)
				{
					DictionaryKeyUtility.KeyComparer<TKey> @default = DictionaryKeyUtility.KeyComparer<TKey>.Default;
					TKey x = list2[0];
					for (int j = 1; j < list2.Count; j++)
					{
						TKey val2 = list2[j];
						if (@default.Compare(x, val2) > 0)
						{
							list2.Sort(@default);
							break;
						}
						x = val2;
					}
				}
				if (list2.Count != list.Count)
				{
					childInfos.Clear();
					base.Property.Children.ClearAndDisposeChildren();
					continue;
				}
				for (int k = 0; k < list2.Count; k++)
				{
					if (!PropertyValueEntry<TKey>.EqualityComparer(list2[k], list[k]))
					{
						childInfos.Clear();
						base.Property.Children.ClearAndDisposeChildren();
						break;
					}
				}
			}
		}

		public override int ChildNameToIndex(string name)
		{
			return CollectionResolverUtilities.DefaultChildNameToIndex(name);
		}

		public override int ChildNameToIndex(ref StringSlice name)
		{
			return CollectionResolverUtilities.DefaultChildNameToIndex(ref name);
		}

		private static bool FastEndsWith(string str, string endsWith)
		{
			if (str.Length < endsWith.Length)
			{
				return false;
			}
			int num = str.Length - endsWith.Length;
			for (int i = 0; i < endsWith.Length; i++)
			{
				if (str[num + i] != endsWith[i])
				{
					return false;
				}
			}
			return true;
		}
	}
}
