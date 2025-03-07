using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Sirenix.Utilities.Editor;

namespace Sirenix.OdinInspector.Editor
{
	public abstract class OdinPropertyResolver
	{
		private bool hasUpdatedChildCountEver;

		private int lastUpdatedTreeID = -1;

		private int childCount;

		private bool hasChildCountConflict;

		private int maxChildCountSeen;

		private static readonly Dictionary<Type, Func<OdinPropertyResolver>> Resolver_EmittedCreator_Cache = new Dictionary<Type, Func<OdinPropertyResolver>>(FastTypeComparer.Instance);

		public bool HasChildCountConflict
		{
			get
			{
				UpdateChildCountIfNeeded();
				return hasChildCountConflict;
			}
			protected set
			{
				hasChildCountConflict = value;
			}
		}

		public int MaxChildCountSeen
		{
			get
			{
				UpdateChildCountIfNeeded();
				return maxChildCountSeen;
			}
			protected set
			{
				maxChildCountSeen = value;
			}
		}

		public virtual Type ResolverForType => null;

		public InspectorProperty Property { get; private set; }

		public virtual bool IsCollection => this is ICollectionResolver;

		public int ChildCount
		{
			get
			{
				UpdateChildCountIfNeeded();
				return childCount;
			}
		}

		public static OdinPropertyResolver Create(Type resolverType, InspectorProperty property)
		{
			if (resolverType == null)
			{
				throw new ArgumentNullException("resolverType");
			}
			if (property == null)
			{
				throw new ArgumentNullException("property");
			}
			if (!typeof(OdinPropertyResolver).IsAssignableFrom(resolverType))
			{
				throw new ArgumentException("Type is not a PropertyResolver");
			}
			if (!Resolver_EmittedCreator_Cache.TryGetValue(resolverType, out var value))
			{
				DynamicMethod dynamicMethod = new DynamicMethod("OdinPropertyResolver_EmittedCreator_" + Guid.NewGuid(), typeof(OdinPropertyResolver), Type.EmptyTypes);
				ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
				iLGenerator.Emit(OpCodes.Newobj, resolverType.GetConstructor(Type.EmptyTypes));
				iLGenerator.Emit(OpCodes.Ret);
				value = (Func<OdinPropertyResolver>)dynamicMethod.CreateDelegate(typeof(Func<OdinPropertyResolver>));
				Resolver_EmittedCreator_Cache.Add(resolverType, value);
			}
			OdinPropertyResolver odinPropertyResolver = value();
			odinPropertyResolver.Property = property;
			odinPropertyResolver.Initialize();
			return odinPropertyResolver;
		}

		public static T Create<T>(InspectorProperty property) where T : OdinPropertyResolver, new()
		{
			if (property == null)
			{
				throw new ArgumentNullException("property");
			}
			T val = new T();
			val.Property = property;
			val.Initialize();
			return val;
		}

		protected virtual void Initialize()
		{
		}

		[MethodImpl((MethodImplOptions)256)]
		private void UpdateChildCountIfNeeded()
		{
			int updateID = Property.Tree.UpdateID;
			if (lastUpdatedTreeID != updateID || !hasUpdatedChildCountEver)
			{
				lastUpdatedTreeID = updateID;
				hasUpdatedChildCountEver = true;
				childCount = CalculateChildCount();
			}
		}

		public abstract InspectorPropertyInfo GetChildInfo(int childIndex);

		public abstract int ChildNameToIndex(string name);

		public virtual int ChildNameToIndex(ref StringSlice name)
		{
			return ChildNameToIndex(name.ToString());
		}

		protected abstract int CalculateChildCount();

		public virtual bool CanResolveForPropertyFilter(InspectorProperty property)
		{
			return true;
		}

		public void ForceUpdateChildCount()
		{
			if (hasUpdatedChildCountEver)
			{
				lastUpdatedTreeID = Property.Tree.UpdateID;
				childCount = CalculateChildCount();
			}
		}
	}
	public abstract class OdinPropertyResolver<TValue> : OdinPropertyResolver
	{
		public sealed override Type ResolverForType => typeof(TValue);

		public IPropertyValueEntry<TValue> ValueEntry => (IPropertyValueEntry<TValue>)base.Property.ValueEntry;

		protected virtual bool AllowNullValues => false;

		protected sealed override int CalculateChildCount()
		{
			IPropertyValueEntry<TValue> propertyValueEntry = (IPropertyValueEntry<TValue>)base.Property.ValueEntry;
			base.HasChildCountConflict = false;
			int num = int.MaxValue;
			base.MaxChildCountSeen = int.MinValue;
			for (int i = 0; i < propertyValueEntry.ValueCount; i++)
			{
				TValue val = propertyValueEntry.Values[i];
				int num2 = ((!AllowNullValues) ? ((val != null) ? GetChildCount(val) : 0) : GetChildCount(val));
				if (num != int.MaxValue && num != num2)
				{
					base.HasChildCountConflict = true;
				}
				if (num2 < num)
				{
					num = num2;
				}
				if (num2 > base.MaxChildCountSeen)
				{
					base.MaxChildCountSeen = num2;
				}
			}
			return num;
		}

		protected abstract int GetChildCount(TValue value);
	}
	public abstract class OdinPropertyResolver<TValue, TAttribute> : OdinPropertyResolver<TValue> where TAttribute : Attribute
	{
	}
}
