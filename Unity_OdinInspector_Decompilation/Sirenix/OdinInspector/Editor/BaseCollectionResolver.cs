using System;
using System.Collections.Generic;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	public abstract class BaseCollectionResolver<TCollection> : OdinPropertyResolver<TCollection>, ICollectionResolver, IApplyableResolver, IRefreshableResolver
	{
		private struct EnqueuedChange
		{
			public Action Action;

			public CollectionChangeInfo Info;
		}

		private Queue<EnqueuedChange> changeQueue = new Queue<EnqueuedChange>();

		private bool isReadOnly;

		private int isReadOnlyLastUpdateID;

		private static bool? IsDerivedFromGenericIList_backingfield;

		private static bool IsDerivedFromGenericList
		{
			get
			{
				if (!IsDerivedFromGenericIList_backingfield.HasValue)
				{
					IsDerivedFromGenericIList_backingfield = !typeof(TCollection).IsGenericType && typeof(TCollection).ImplementsOpenGenericClass(typeof(List<>));
				}
				return IsDerivedFromGenericIList_backingfield.Value;
			}
		}

		protected virtual bool ApplyToRootSelectionTarget => false;

		public abstract Type ElementType { get; }

		public override bool IsCollection => true;

		public bool IsReadOnly
		{
			get
			{
				if (isReadOnlyLastUpdateID != base.Property.Tree.UpdateID)
				{
					isReadOnlyLastUpdateID = base.Property.Tree.UpdateID;
					isReadOnly = true;
					IPropertyValueEntry valueEntry = base.Property.ValueEntry;
					for (int i = 0; i < valueEntry.ValueCount; i++)
					{
						try
						{
							TCollection val = (TCollection)valueEntry.WeakValues[i];
							if (val == null)
							{
								isReadOnly = false;
								break;
							}
							if (!CollectionIsReadOnly(val))
							{
								isReadOnly = false;
								break;
							}
							continue;
						}
						catch (NotImplementedException)
						{
							isReadOnly = false;
							continue;
						}
					}
				}
				return isReadOnly;
			}
		}

		public int MaxCollectionLength => base.MaxChildCountSeen;

		public event Action<CollectionChangeInfo> OnBeforeChange;

		public event Action<CollectionChangeInfo> OnAfterChange;

		public void InvokeOnBeforeChange(CollectionChangeInfo info)
		{
			if (this.OnBeforeChange != null)
			{
				this.OnBeforeChange(info);
			}
		}

		public void InvokeOnAfterChange(CollectionChangeInfo info)
		{
			if (this.OnAfterChange != null)
			{
				this.OnAfterChange(info);
			}
		}

		public override bool CanResolveForPropertyFilter(InspectorProperty property)
		{
			if (!ApplyToRootSelectionTarget && property == property.Tree.RootProperty)
			{
				return false;
			}
			if (property.ValueEntry.SerializationBackend.IsUnity && IsDerivedFromGenericList)
			{
				return false;
			}
			return true;
		}

		public bool ApplyChanges()
		{
			bool flag = changeQueue.Count > 0;
			if (flag)
			{
				base.Property.RecordForUndo("Collection modification");
			}
			while (changeQueue.Count > 0)
			{
				EnqueuedChange enqueuedChange = changeQueue.Dequeue();
				if (this.OnBeforeChange != null)
				{
					this.OnBeforeChange(enqueuedChange.Info);
				}
				enqueuedChange.Action();
				if (this.OnAfterChange != null)
				{
					this.OnAfterChange(enqueuedChange.Info);
				}
			}
			if (flag)
			{
				OnCollectionChangesApplied();
				if (base.Property.SupportsPrefabModifications)
				{
					base.Property.Update(forceUpdate: true);
					foreach (InspectorProperty item in base.Property.Children.Recurse())
					{
						item.Update(forceUpdate: true);
					}
				}
				base.Property.Children.ClearAndDisposeChildren();
			}
			return flag;
		}

		public bool CheckHasLengthConflict()
		{
			IPropertyValueEntry valueEntry = base.Property.ValueEntry;
			if (valueEntry.ValueCount == 0)
			{
				return false;
			}
			TCollection val = base.ValueEntry.Values[0];
			int num = ((val != null) ? GetChildCount(val) : 0);
			for (int i = 1; i < base.ValueEntry.ValueCount; i++)
			{
				val = base.ValueEntry.Values[i];
				if (num != ((val != null) ? GetChildCount(val) : 0))
				{
					return true;
				}
			}
			return false;
		}

		public abstract bool ChildPropertyRequiresRefresh(int index, InspectorPropertyInfo info);

		[Obsolete("Use the overload that takes a CollectionChangeInfo instead.", false)]
		public void EnqueueChange(Action action)
		{
			EnqueueChange(action, new CollectionChangeInfo
			{
				ChangeType = CollectionChangeType.Unspecified
			});
		}

		public void EnqueueChange(Action action, CollectionChangeInfo info)
		{
			changeQueue.Enqueue(new EnqueuedChange
			{
				Action = action,
				Info = info
			});
			base.Property.Tree.DelayActionUntilRepaint(delegate
			{
				base.Property.Tree.RegisterPropertyDirty(base.Property);
			});
		}

		public void QueueAdd(object[] values)
		{
			for (int i = 0; i < values.Length; i++)
			{
				QueueAdd(values[i], i);
			}
		}

		public void QueueAdd(object value, int selectionIndex)
		{
			EnqueueChange(delegate
			{
				Add((TCollection)base.Property.BaseValueEntry.WeakValues[selectionIndex], value);
			}, new CollectionChangeInfo
			{
				ChangeType = CollectionChangeType.Add,
				Value = value,
				SelectionIndex = selectionIndex
			});
		}

		public void QueueClear()
		{
			int count = base.Property.BaseValueEntry.WeakValues.Count;
			for (int i = 0; i < count; i++)
			{
				int capture = i;
				EnqueueChange(delegate
				{
					Clear((TCollection)base.Property.BaseValueEntry.WeakValues[capture]);
				}, new CollectionChangeInfo
				{
					ChangeType = CollectionChangeType.Clear,
					SelectionIndex = capture
				});
			}
		}

		public void QueueRemove(object[] values)
		{
			for (int i = 0; i < values.Length; i++)
			{
				QueueRemove(values[i], i);
			}
		}

		public void QueueRemove(object value, int selectionIndex)
		{
			EnqueueChange(delegate
			{
				Remove((TCollection)base.Property.BaseValueEntry.WeakValues[selectionIndex], value);
			}, new CollectionChangeInfo
			{
				Value = value,
				ChangeType = CollectionChangeType.RemoveValue,
				SelectionIndex = selectionIndex
			});
		}

		protected abstract void Add(TCollection collection, object value);

		protected abstract void Clear(TCollection collection);

		protected abstract bool CollectionIsReadOnly(TCollection collection);

		protected virtual void OnCollectionChangesApplied()
		{
		}

		protected abstract void Remove(TCollection collection, object value);
	}
}
