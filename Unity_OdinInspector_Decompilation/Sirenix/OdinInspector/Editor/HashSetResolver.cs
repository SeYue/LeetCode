using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public class HashSetResolver<TCollection, TElement> : BaseCollectionResolver<TCollection> where TCollection : HashSet<TElement>
	{
		private Dictionary<TCollection, List<TElement>> elementsArrays = new Dictionary<TCollection, List<TElement>>();

		private int lastUpdateId = -1;

		private Dictionary<int, InspectorPropertyInfo> childInfos = new Dictionary<int, InspectorPropertyInfo>();

		private HashSet<TCollection> seenHashset = new HashSet<TCollection>();

		private List<TCollection> toRemoveList = new List<TCollection>();

		public override Type ElementType => typeof(TElement);

		public override int ChildNameToIndex(string name)
		{
			return CollectionResolverUtilities.DefaultChildNameToIndex(name);
		}

		public override int ChildNameToIndex(ref StringSlice name)
		{
			return CollectionResolverUtilities.DefaultChildNameToIndex(ref name);
		}

		public override bool ChildPropertyRequiresRefresh(int index, InspectorPropertyInfo info)
		{
			return false;
		}

		public override InspectorPropertyInfo GetChildInfo(int childIndex)
		{
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Expected O, but got Unknown
			if (childIndex < 0 || childIndex >= base.ChildCount)
			{
				throw new IndexOutOfRangeException();
			}
			if (!childInfos.TryGetValue(childIndex, out var value))
			{
				value = InspectorPropertyInfo.CreateValue(CollectionResolverUtilities.DefaultIndexToChildName(childIndex), childIndex, base.Property.BaseValueEntry.SerializationBackend, new GetterSetter<TCollection, TElement>(delegate(ref TCollection collection)
				{
					return GetElement(collection, childIndex);
				}, delegate(ref TCollection collection, TElement element)
				{
					SetElement(collection, element, childIndex);
				}), base.Property.Attributes.Where((Attribute attr) => !attr.GetType().IsDefined(typeof(DontApplyToListElementsAttribute), inherit: true)).AppendWith((Attribute)new DelayedAttribute()).AppendWith(new SuppressInvalidAttributeErrorAttribute())
					.ToArray());
				childInfos[childIndex] = value;
			}
			return value;
		}

		protected override void Initialize()
		{
			base.Initialize();
		}

		private TElement GetElement(TCollection collection, int index)
		{
			EnsureUpdated();
			if (elementsArrays.TryGetValue(collection, out var value))
			{
				return value[index];
			}
			return default(TElement);
		}

		private void SetElement(TCollection collection, TElement element, int index)
		{
			int count = collection.Count;
			if (elementsArrays.TryGetValue(collection, out var value) && !value.Contains(element))
			{
				value[index] = element;
				collection.Clear();
				collection.AddRange(value);
				EnsureUpdated(force: true);
			}
		}

		private void EnsureUpdated(bool force = false)
		{
			int updateID = base.Property.Tree.UpdateID;
			if (!force && lastUpdateId == updateID)
			{
				return;
			}
			seenHashset.Clear();
			toRemoveList.Clear();
			lastUpdateId = updateID;
			int valueCount = base.ValueEntry.ValueCount;
			for (int i = 0; i < valueCount; i++)
			{
				TCollection val = base.ValueEntry.Values[i];
				if (val != null)
				{
					seenHashset.Add(val);
					if (!elementsArrays.TryGetValue(val, out var value))
					{
						value = new List<TElement>(val.Count);
						elementsArrays[val] = value;
					}
					value.Clear();
					value.AddRange(val);
					DictionaryKeyUtility.KeyComparer<TElement> @default = DictionaryKeyUtility.KeyComparer<TElement>.Default;
					value.Sort(@default);
				}
			}
			foreach (TCollection key in elementsArrays.Keys)
			{
				if (!seenHashset.Contains(key))
				{
					toRemoveList.Add(key);
				}
			}
			for (int j = 0; j < toRemoveList.Count; j++)
			{
				elementsArrays.Remove(toRemoveList[j]);
			}
		}

		protected override void Add(TCollection collection, object value)
		{
			collection.Add((TElement)value);
		}

		protected override void Clear(TCollection collection)
		{
			collection.Clear();
		}

		protected override bool CollectionIsReadOnly(TCollection collection)
		{
			return false;
		}

		protected override int GetChildCount(TCollection value)
		{
			return value.Count;
		}

		protected override void Remove(TCollection collection, object value)
		{
			collection.Remove((TElement)value);
		}
	}
}
