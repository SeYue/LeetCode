using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Serialization;
using Sirenix.Utilities.Editor;

namespace Sirenix.OdinInspector.Editor
{
	[ResolverPriority(-2.0)]
	public class StrongCollectionResolver<TCollection, TElement> : BaseOrderedCollectionResolver<TCollection> where TCollection : ICollection<TElement>
	{
		private Dictionary<TCollection, TElement[]> elementsArrays = new Dictionary<TCollection, TElement[]>();

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
				}), base.Property.Attributes.Where((Attribute attr) => !attr.GetType().IsDefined(typeof(DontApplyToListElementsAttribute), inherit: true)).ToArray());
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
			using Buffer<TElement> buffer = Buffer<TElement>.Claim(count);
			TElement[] array = buffer.Array;
			collection.CopyTo(array, 0);
			collection.Clear();
			for (int i = 0; i < count; i++)
			{
				if (i == index)
				{
					collection.Add(element);
				}
				else
				{
					collection.Add(array[i]);
				}
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
					if (!elementsArrays.TryGetValue(val, out var value) || value.Length != val.Count)
					{
						value = new TElement[val.Count];
						elementsArrays[val] = value;
					}
					val.CopyTo(value, 0);
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
			return collection.IsReadOnly;
		}

		protected override int GetChildCount(TCollection value)
		{
			return value.Count;
		}

		protected override void Remove(TCollection collection, object value)
		{
			collection.Remove((TElement)value);
		}

		protected override void InsertAt(TCollection collection, int index, object value)
		{
			int count = collection.Count;
			TElement item = (TElement)value;
			using Buffer<TElement> buffer = Buffer<TElement>.Claim(count);
			TElement[] array = buffer.Array;
			collection.CopyTo(array, 0);
			collection.Clear();
			for (int i = 0; i < count + 1; i++)
			{
				if (i == index)
				{
					collection.Add(item);
					continue;
				}
				int num = ((i >= index) ? (i - 1) : i);
				collection.Add(array[num]);
			}
		}

		protected override void RemoveAt(TCollection collection, int index)
		{
			int count = collection.Count;
			using Buffer<TElement> buffer = Buffer<TElement>.Claim(count);
			TElement[] array = buffer.Array;
			collection.CopyTo(array, 0);
			collection.Clear();
			for (int i = 0; i < count; i++)
			{
				if (i != index)
				{
					collection.Add(array[i]);
				}
			}
		}
	}
}
