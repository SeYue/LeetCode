using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;

namespace Sirenix.OdinInspector.Editor
{
	[ResolverPriority(-1.0)]
	public class StrongListPropertyResolver<TList, TElement> : BaseOrderedCollectionResolver<TList>, IMaySupportPrefabModifications where TList : IList<TElement>
	{
		private static bool IsArray = typeof(TList).IsArray;

		private Dictionary<int, InspectorPropertyInfo> childInfos = new Dictionary<int, InspectorPropertyInfo>();

		private List<Attribute> childAttrs;

		public bool MaySupportPrefabModifications => true;

		public override Type ElementType => typeof(TElement);

		protected override void Initialize()
		{
			base.Initialize();
			ImmutableList<Attribute> attributes = base.Property.Attributes;
			List<Attribute> list = new List<Attribute>(attributes.Count);
			for (int i = 0; i < attributes.Count; i++)
			{
				Attribute attribute = attributes[i];
				if (!attribute.GetType().IsDefined(typeof(DontApplyToListElementsAttribute), inherit: true))
				{
					list.Add(attribute);
				}
			}
			childAttrs = list;
		}

		public override InspectorPropertyInfo GetChildInfo(int childIndex)
		{
			if (childIndex < 0 || childIndex >= base.ChildCount)
			{
				throw new IndexOutOfRangeException();
			}
			if (!childInfos.TryGetValue(childIndex, out var value))
			{
				value = InspectorPropertyInfo.CreateValue(CollectionResolverUtilities.DefaultIndexToChildName(childIndex), childIndex, base.Property.BaseValueEntry.SerializationBackend, new GetterSetter<TList, TElement>(delegate(ref TList list)
				{
					return list[childIndex];
				}, delegate(ref TList list, TElement element)
				{
					list[childIndex] = element;
				}), childAttrs);
				childInfos[childIndex] = value;
			}
			return value;
		}

		public override bool ChildPropertyRequiresRefresh(int index, InspectorPropertyInfo info)
		{
			return false;
		}

		public override int ChildNameToIndex(string name)
		{
			return CollectionResolverUtilities.DefaultChildNameToIndex(name);
		}

		public override int ChildNameToIndex(ref StringSlice name)
		{
			return CollectionResolverUtilities.DefaultChildNameToIndex(ref name);
		}

		protected override int GetChildCount(TList value)
		{
			return value.Count;
		}

		protected override void Add(TList collection, object value)
		{
			if (IsArray)
			{
				TList newArray = (TList)(object)ArrayUtilities.CreateNewArrayWithAddedElement((TElement[])(object)collection, (TElement)value);
				ReplaceArray(collection, newArray);
			}
			else
			{
				collection.Add((TElement)value);
			}
		}

		protected override void InsertAt(TList collection, int index, object value)
		{
			if (IsArray)
			{
				TList newArray = (TList)(object)ArrayUtilities.CreateNewArrayWithInsertedElement((TElement[])(object)collection, index, (TElement)value);
				ReplaceArray(collection, newArray);
			}
			else
			{
				collection.Insert(index, (TElement)value);
			}
		}

		protected override void Remove(TList collection, object value)
		{
			if (IsArray)
			{
				int num = collection.IndexOf((TElement)value);
				if (num >= 0)
				{
					TList newArray = (TList)(object)ArrayUtilities.CreateNewArrayWithRemovedElement((TElement[])(object)collection, num);
					ReplaceArray(collection, newArray);
				}
			}
			else
			{
				collection.Remove((TElement)value);
			}
		}

		protected override void RemoveAt(TList collection, int index)
		{
			if (IsArray)
			{
				TList newArray = (TList)(object)ArrayUtilities.CreateNewArrayWithRemovedElement((TElement[])(object)collection, index);
				ReplaceArray(collection, newArray);
			}
			else
			{
				collection.RemoveAt(index);
			}
		}

		protected override void Clear(TList collection)
		{
			if (IsArray)
			{
				ReplaceArray(collection, (TList)(object)new TElement[0]);
			}
			else
			{
				collection.Clear();
			}
		}

		protected override bool CollectionIsReadOnly(TList collection)
		{
			if (IsArray)
			{
				return false;
			}
			return collection.IsReadOnly;
		}

		private void ReplaceArray(TList oldArray, TList newArray)
		{
			if (!base.Property.ValueEntry.SerializationBackend.SupportsCyclicReferences)
			{
				for (int i = 0; i < base.ValueEntry.ValueCount; i++)
				{
					if ((object)base.ValueEntry.Values[i] == (object)oldArray)
					{
						base.ValueEntry.Values[i] = newArray;
						(base.ValueEntry as IValueEntryActualValueSetter).SetActualValue(i, newArray);
					}
				}
				return;
			}
			foreach (InspectorProperty item in base.Property.Tree.EnumerateTree())
			{
				if (item.Info.PropertyType != 0 || item.Info.TypeOfValue.IsValueType)
				{
					continue;
				}
				IPropertyValueEntry valueEntry = item.ValueEntry;
				for (int j = 0; j < valueEntry.ValueCount; j++)
				{
					object obj = valueEntry.WeakValues[j];
					if ((object)oldArray == obj)
					{
						valueEntry.WeakValues[j] = newArray;
						(valueEntry as IValueEntryActualValueSetter).SetActualValue(j, newArray);
					}
				}
			}
		}
	}
}
