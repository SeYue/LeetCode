using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Represents the values of an <see cref="T:Sirenix.OdinInspector.Editor.InspectorProperty" />, and contains utilities for querying the values' type and getting and setting them.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.IPropertyValueEntry" />
	public abstract class PropertyValueEntry : IPropertyValueEntry, IDisposable
	{
		private struct TypePairKey
		{
			public Type ParentType;

			public Type ValueType;
		}

		private class TypePairKeyComparer : IEqualityComparer<TypePairKey>
		{
			public bool Equals(TypePairKey x, TypePairKey y)
			{
				if (x.ParentType == y.ParentType || x.ParentType == y.ParentType)
				{
					if (x.ValueType != y.ValueType)
					{
						return x.ValueType == y.ValueType;
					}
					return true;
				}
				return false;
			}

			public int GetHashCode(TypePairKey obj)
			{
				return obj.ParentType.GetHashCode() ^ obj.ValueType.GetHashCode();
			}
		}

		/// <summary>
		/// Delegate type used for the events <see cref="E:Sirenix.OdinInspector.Editor.PropertyValueEntry.OnValueChanged" /> and <see cref="E:Sirenix.OdinInspector.Editor.PropertyValueEntry.OnChildValueChanged" />.
		/// </summary>
		public delegate void ValueChangedDelegate(int targetIndex);

		private static readonly Dictionary<TypePairKey, Type> GenericValueEntryVariants_Cache = new Dictionary<TypePairKey, Type>(new TypePairKeyComparer());

		private static readonly Dictionary<Type, Func<PropertyValueEntry>> GenericValueEntryVariants_EmittedCreator_Cache = new Dictionary<Type, Func<PropertyValueEntry>>(FastTypeComparer.Instance);

		private static readonly Dictionary<TypePairKey, Type> GenericAliasVariants_Cache = new Dictionary<TypePairKey, Type>(new TypePairKeyComparer());

		private static readonly Dictionary<Type, Func<PropertyValueEntry, IPropertyValueEntry>> GenericAlasVariants_EmittedCreator_Cache = new Dictionary<Type, Func<PropertyValueEntry, IPropertyValueEntry>>(FastTypeComparer.Instance);

		private static readonly Type[] TypeArrayWithOneElement_Cached = new Type[1];

		private InspectorProperty parentValueProperty;

		private InspectorProperty property;

		private bool isBaseEditable;

		private Type actualTypeOfValue;

		private bool baseValueIsValueType;

		/// <summary>
		/// <para>The nearest parent property that has a value.
		/// That is, the property from which this value
		/// entry will fetch its parentvalues from in order
		/// to extract its own values.</para>
		///
		/// <para>If <see cref="P:Sirenix.OdinInspector.Editor.PropertyValueEntry.ParentValueProperty" /> is null, this is a root property.</para>
		/// </summary>
		protected InspectorProperty ParentValueProperty => parentValueProperty;

		/// <summary>
		/// Whether this value entry represents a boxed value type.
		/// </summary>
		protected bool IsBoxedValueType { get; private set; }

		/// <summary>
		/// The number of parallel values this entry represents. This will always be exactly equal to the count of <see cref="P:Sirenix.OdinInspector.Editor.PropertyTree.WeakTargets" />.
		/// </summary>
		public int ValueCount { get; private set; }

		/// <summary>
		/// Whether this value entry is editable or not.
		/// </summary>
		public bool IsEditable
		{
			get
			{
				if (isBaseEditable)
				{
					if (parentValueProperty != null)
					{
						IPropertyValueEntry valueEntry = parentValueProperty.ValueEntry;
						if (!valueEntry.IsEditable)
						{
							return false;
						}
						ICollectionResolver collectionResolver = parentValueProperty.ChildResolver as ICollectionResolver;
						if (collectionResolver != null)
						{
							if (collectionResolver.IsReadOnly)
							{
								return false;
							}
							return true;
						}
					}
					return true;
				}
				return false;
			}
		}

		/// <summary>
		/// If this value entry has the override type <see cref="F:Sirenix.OdinInspector.Editor.PropertyValueState.Reference" />, this is the path of the property it references.
		/// </summary>
		public string TargetReferencePath { get; private set; }

		/// <summary>
		/// <para>The actual serialization backend for this value entry, possibly inherited from the serialization backend of the root property this entry is a child of.</para>
		/// <para>Note that this is *not* always equal to <see cref="P:Sirenix.OdinInspector.Editor.InspectorPropertyInfo.SerializationBackend" />.</para>
		/// </summary>
		public SerializationBackend SerializationBackend { get; private set; }

		/// <summary>
		/// The property whose values this value entry represents.
		/// </summary>
		public InspectorProperty Property => property;

		/// <summary>
		/// Provides access to the weakly typed values of this value entry.
		/// </summary>
		public abstract IPropertyValueCollection WeakValues { get; }

		/// <summary>
		/// Whether this value entry has been changed from its prefab counterpart.
		/// </summary>
		public bool ValueChangedFromPrefab { get; internal set; }

		/// <summary>
		/// Whether this value entry has had its list length changed from its prefab counterpart.
		/// </summary>
		public bool ListLengthChangedFromPrefab { get; internal set; }

		/// <summary>
		/// Whether this value entry has had its dictionary values changes from its prefab counterpart.
		/// </summary>
		public bool DictionaryChangedFromPrefab { get; internal set; }

		/// <summary>
		/// <para>A weakly typed smart value that represents the first element of the value entry's value collection, but has "smart logic" for setting the value that detects relevant changes and applies them in parallel.</para>
		/// <para>This lets you often just use the smart value instead of having to deal with the tedium of multiple parallel values.</para>
		/// </summary>
		public abstract object WeakSmartValue { get; set; }

		/// <summary>
		/// The type from which this value entry comes. If this value entry represents a member value, this is the declaring type of the member. If the value entry represents a collection element, this is the type of the collection.
		/// </summary>
		public abstract Type ParentType { get; }

		/// <summary>
		/// The most precise known contained type of the value entry. If polymorphism is in effect, this will be some type derived from <see cref="P:Sirenix.OdinInspector.Editor.PropertyValueEntry.BaseValueType" />.
		/// </summary>
		public Type TypeOfValue
		{
			get
			{
				if (actualTypeOfValue == null)
				{
					actualTypeOfValue = BaseValueType;
				}
				return actualTypeOfValue;
			}
		}

		/// <summary>
		/// The base type of the value entry. If this is value entry represents a member value, this is the type of the member. If the value entry represents a collection element, this is the element type of the collection.
		/// </summary>
		public Type BaseValueType { get; private set; }

		/// <summary>
		/// The special state of the value entry.
		/// </summary>
		public PropertyValueState ValueState { get; private set; }

		/// <summary>
		/// Whether this value entry is an alias, or not. Value entry aliases are used to provide strongly typed value entries in the case of polymorphism.
		/// </summary>
		public bool IsAlias => false;

		/// <summary>
		/// The context container of this property.
		/// </summary>
		public PropertyContextContainer Context => Property.Context;

		/// <summary>
		/// Whether this type is marked as an atomic type using a <see cref="T:Sirenix.OdinInspector.Editor.IAtomHandler" />.
		/// </summary>
		public abstract bool IsMarkedAtomic { get; }

		/// <summary>
		/// An event that is invoked during <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueEntry.ApplyChanges" />, when any values have changed.
		/// </summary>
		public event Action<int> OnValueChanged;

		/// <summary>
		/// An event that is invoked during <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueEntry.ApplyChanges" />, when any child values have changed.
		/// </summary>
		public event Action<int> OnChildValueChanged;

		/// <summary>
		/// Updates the values contained in this value entry to the actual values in the target objects, and updates its state (override, type of value, etc.) accordingly.
		/// </summary>
		public void Update()
		{
			UpdateValues();
			if (!baseValueIsValueType && (SerializationBackend.SupportsPolymorphism || typeof(Object).IsAssignableFrom(BaseValueType)))
			{
				Type mostPreciseContainedType = GetMostPreciseContainedType();
				if (actualTypeOfValue != mostPreciseContainedType)
				{
					actualTypeOfValue = mostPreciseContainedType;
					IsBoxedValueType = BaseValueType == typeof(object) && mostPreciseContainedType.IsValueType;
				}
			}
			ValueState = GetValueState();
			if (ValueState == PropertyValueState.Reference)
			{
				property.Tree.ObjectIsReferenced(WeakValues[0], out var referencePath);
				TargetReferencePath = referencePath;
			}
			else
			{
				TargetReferencePath = null;
			}
		}

		/// <summary>
		/// <para>Checks whether the values in this value entry are equal to the values in another value entry.</para>
		/// <para>Note, both value entries must have the same value type, and must represent values that are .NET value types.</para>
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public abstract bool ValueTypeValuesAreEqual(IPropertyValueEntry other);

		/// <summary>
		/// Applies the changes made to this value entry to the target objects, and registers prefab modifications as necessary.
		/// </summary>
		/// <returns>
		/// True if any changes were made, otherwise, false.
		/// </returns>
		public abstract bool ApplyChanges();

		/// <summary>
		/// Determines the value state of this value entry.
		/// </summary>
		protected abstract PropertyValueState GetValueState();

		/// <summary>
		/// Determines what the most precise contained type is on this value entry.
		/// </summary>
		protected abstract Type GetMostPreciseContainedType();

		/// <summary>
		/// Updates all values in this value entry from the target tree values.
		/// </summary>
		protected abstract void UpdateValues();

		/// <summary>
		/// Initializes this value entry.
		/// </summary>
		protected abstract void Initialize();

		internal void TriggerOnValueChanged(int index)
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Invalid comparison between Unknown and I4
			Action action = delegate
			{
				//IL_0026: Expected O, but got Unknown
				if (this.OnValueChanged != null)
				{
					try
					{
						this.OnValueChanged(index);
					}
					catch (ExitGUIException val)
					{
						ExitGUIException val2 = val;
						throw val2;
					}
					catch (Exception ex)
					{
						if (ex.IsExitGUIException())
						{
							throw ex.AsExitGUIException();
						}
						Debug.LogException(ex);
					}
				}
				Property.Tree.InvokeOnPropertyValueChanged(Property, index);
			};
			if (Event.get_current() != null && (int)Event.get_current().get_type() == 7)
			{
				action();
			}
			else
			{
				Property.Tree.DelayActionUntilRepaint(action);
			}
			if (ParentValueProperty != null)
			{
				ParentValueProperty.BaseValueEntry.TriggerOnChildValueChanged(index);
			}
		}

		internal void TriggerOnChildValueChanged(int index)
		{
			Property.Tree.DelayActionUntilRepaint(delegate
			{
				//IL_0026: Expected O, but got Unknown
				if (this.OnChildValueChanged != null)
				{
					try
					{
						this.OnChildValueChanged(index);
					}
					catch (ExitGUIException val)
					{
						ExitGUIException val2 = val;
						throw val2;
					}
					catch (Exception ex)
					{
						if (ex.IsExitGUIException())
						{
							throw ex.AsExitGUIException();
						}
						Debug.LogException(ex);
					}
				}
			});
			if (ParentValueProperty != null)
			{
				ParentValueProperty.BaseValueEntry.TriggerOnChildValueChanged(index);
			}
		}

		/// <summary>
		/// Creates an alias value entry of a given type, for a given value entry. This is used to implement polymorphism in Odin.
		/// </summary>
		public static IPropertyValueEntry CreateAlias(PropertyValueEntry entry, Type valueType)
		{
			if (entry == null)
			{
				throw new ArgumentNullException("entry");
			}
			if (valueType == null)
			{
				throw new ArgumentNullException("valueType");
			}
			TypePairKey key = default(TypePairKey);
			key.ParentType = entry.BaseValueType;
			key.ValueType = valueType;
			if (!GenericAliasVariants_Cache.TryGetValue(key, out var value))
			{
				value = typeof(PropertyValueEntryAlias<, >).MakeGenericType(entry.BaseValueType, valueType);
				GenericAliasVariants_Cache.Add(key, value);
			}
			if (!GenericAlasVariants_EmittedCreator_Cache.TryGetValue(value, out var value2))
			{
				TypeArrayWithOneElement_Cached[0] = typeof(PropertyValueEntry);
				DynamicMethod dynamicMethod = new DynamicMethod("AliasCreator_" + Guid.NewGuid().ToString(), typeof(IPropertyValueEntry), TypeArrayWithOneElement_Cached);
				ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
				iLGenerator.Emit(OpCodes.Ldarg_0);
				iLGenerator.Emit(OpCodes.Newobj, value.GetConstructor(TypeArrayWithOneElement_Cached));
				iLGenerator.Emit(OpCodes.Ret);
				value2 = (Func<PropertyValueEntry, IPropertyValueEntry>)dynamicMethod.CreateDelegate(typeof(Func<PropertyValueEntry, IPropertyValueEntry>));
				GenericAlasVariants_EmittedCreator_Cache.Add(value, value2);
			}
			return value2(entry);
		}

		/// <summary>
		/// Creates a value entry for a given property, of a given value type. Note that the created value entry is returned un-updated, and needs to have <see cref="M:Sirenix.OdinInspector.Editor.PropertyValueEntry.Update" /> called on it before it can be used.
		/// </summary>
		internal static PropertyValueEntry Create(InspectorProperty property, Type valueType, bool isSecretRoot)
		{
			if (property == null)
			{
				throw new ArgumentNullException("property");
			}
			if (valueType == null)
			{
				throw new ArgumentNullException("valueType");
			}
			if (property.Info.PropertyType != 0)
			{
				throw new ArgumentException("Cannot create a " + typeof(PropertyValueEntry).Name + " for a property which is not a value property.");
			}
			InspectorProperty inspectorProperty = property.ParentValueProperty;
			Type type = (isSecretRoot ? typeof(int) : ((inspectorProperty == null) ? property.Tree.TargetType : inspectorProperty.ValueEntry.TypeOfValue));
			TypePairKey key = default(TypePairKey);
			key.ParentType = type;
			key.ValueType = valueType;
			if (!GenericValueEntryVariants_Cache.TryGetValue(key, out var value))
			{
				value = typeof(PropertyValueEntry<, >).MakeGenericType(type, valueType);
				GenericValueEntryVariants_Cache.Add(key, value);
			}
			if (!GenericValueEntryVariants_EmittedCreator_Cache.TryGetValue(value, out var value2))
			{
				DynamicMethod dynamicMethod = new DynamicMethod("PropertyValueEntry_InstanceCreator_" + Guid.NewGuid(), typeof(PropertyValueEntry), Type.EmptyTypes);
				ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
				iLGenerator.Emit(OpCodes.Newobj, value.GetConstructor(Type.EmptyTypes));
				iLGenerator.Emit(OpCodes.Ret);
				value2 = (Func<PropertyValueEntry>)dynamicMethod.CreateDelegate(typeof(Func<PropertyValueEntry>));
				GenericValueEntryVariants_EmittedCreator_Cache.Add(value, value2);
			}
			PropertyValueEntry propertyValueEntry = value2();
			propertyValueEntry.BaseValueType = valueType;
			propertyValueEntry.property = property;
			propertyValueEntry.ValueCount = property.Tree.WeakTargets.Count;
			propertyValueEntry.parentValueProperty = inspectorProperty;
			propertyValueEntry.baseValueIsValueType = valueType.IsValueType;
			propertyValueEntry.IsBoxedValueType = propertyValueEntry.BaseValueType == typeof(object) && propertyValueEntry.TypeOfValue.IsValueType;
			if (inspectorProperty != null)
			{
				propertyValueEntry.SerializationBackend = property.Info.SerializationBackend;
				propertyValueEntry.isBaseEditable = inspectorProperty.BaseValueEntry.isBaseEditable && property.Info.IsEditable;
			}
			else
			{
				propertyValueEntry.SerializationBackend = property.Info.SerializationBackend;
				propertyValueEntry.isBaseEditable = property.Info.IsEditable;
			}
			propertyValueEntry.Initialize();
			return propertyValueEntry;
		}

		/// <summary>
		/// <para>Determines whether the value at the given selection index is different from the given prefab value, as is relevant for prefab modification checks.</para>
		/// <para>If the value is a reference type, null and type difference is checked. If value is a value type, a comparer from <see cref="M:Sirenix.Utilities.TypeExtensions.GetEqualityComparerDelegate``1" /> is used.</para>
		/// <para>This method is best ignored unless you know what you are doing.</para>
		/// </summary>
		/// <param name="value">The value to check differences against.</param>
		/// <param name="index">The selection index to compare against.</param>
		public abstract bool ValueIsPrefabDifferent(object value, int index);

		public void Dispose()
		{
			this.OnValueChanged = null;
			this.OnChildValueChanged = null;
		}
	}
	/// <summary>
	/// Represents the values of an <see cref="T:Sirenix.OdinInspector.Editor.InspectorProperty" />, and contains utilities for querying the values' type and getting and setting them.
	/// </summary>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.IPropertyValueEntry" />
	public abstract class PropertyValueEntry<TValue> : PropertyValueEntry, IPropertyValueEntry<TValue>, IPropertyValueEntry, IDisposable, IValueEntryActualValueSetter<TValue>, IValueEntryActualValueSetter
	{
		/// <summary>
		/// An equality comparer for comparing values of type <see cref="!:TValue" />. This is gotten using <see cref="M:Sirenix.Utilities.TypeExtensions.GetEqualityComparerDelegate``1" />.
		/// </summary>
		public static readonly Func<TValue, TValue, bool> EqualityComparer = TypeExtensions.GetEqualityComparerDelegate<TValue>();

		/// <summary>
		/// Whether <see cref="!:TValue" />.is a primitive type; that is, the type is primitive, a string, or an enum.
		/// </summary>
		protected static readonly bool ValueIsPrimitive = typeof(TValue).IsPrimitive || typeof(TValue) == typeof(string) || typeof(TValue).IsEnum;

		/// <summary>
		/// Whether <see cref="!:TValue" /> is a value type.
		/// </summary>
		protected static readonly bool ValueIsValueType = typeof(TValue).IsValueType;

		/// <summary>
		/// Whether the type of the value is marked atomic.
		/// </summary>
		protected static readonly bool ValueIsMarkedAtomic = typeof(TValue).IsMarkedAtomic();

		/// <summary>
		/// If the type of the value is marked atomic, this an instance of an atom handler for the value type.
		/// </summary>
		protected static readonly IAtomHandler<TValue> AtomHandler = (ValueIsMarkedAtomic ? AtomHandlerLocator.GetAtomHandler<TValue>() : null);

		private PropertyValueCollection<TValue> values;

		private bool isWaitingForDelayedValueSet;

		/// <summary>
		/// Whether <see cref="P:Sirenix.OdinInspector.Editor.PropertyValueEntry.TypeOfValue" /> is derived from <see cref="T:UnityEngine.Object" />.
		/// </summary>
		protected bool ValueIsUnityObject => typeof(Object).IsAssignableFrom(base.TypeOfValue);

		/// <summary>
		/// Provides access to the weakly typed values of this value entry.
		/// </summary>
		public sealed override IPropertyValueCollection WeakValues => values;

		/// <summary>
		/// Provides access to the strongly typed values of this value entry.
		/// </summary>
		public IPropertyValueCollection<TValue> Values => values;

		/// <summary>
		/// Whether this type is marked as an atomic type using a <see cref="T:Sirenix.OdinInspector.Editor.IAtomHandler" />.
		/// </summary>
		public override bool IsMarkedAtomic => ValueIsMarkedAtomic;

		/// <summary>
		/// <para>A weakly typed smart value that represents the first element of the value entry's value collection, but has "smart logic" for setting the value that detects relevant changes and applies them in parallel.</para>
		/// <para>This lets you often just use the smart value instead of having to deal with the tedium of multiple parallel values.</para>
		/// </summary>
		public override object WeakSmartValue
		{
			get
			{
				return SmartValue;
			}
			set
			{
				try
				{
					SmartValue = (TValue)value;
				}
				catch (InvalidCastException)
				{
					if (value == null)
					{
						Debug.LogError((object)("Invalid cast on set weak value! Could not cast value 'null' to the type '" + typeof(TValue).GetNiceName() + "' on property " + base.Property.Path + "."));
					}
					else
					{
						Debug.LogError((object)("Invalid cast on set weak value! Could not cast value of type '" + value.GetType().GetNiceName() + "' to '" + typeof(TValue).GetNiceName() + "' on property " + base.Property.Path + "."));
					}
				}
			}
		}

		/// <summary>
		/// <para>A strongly typed smart value that represents the first element of the value entry's value collection, but has "smart logic" for setting the value that detects relevant changes and applies them in parallel.</para>
		/// <para>This lets you often just use the smart value instead of having to deal with the tedium of multiple parallel values.</para>
		/// </summary>
		public TValue SmartValue
		{
			get
			{
				return values[0];
			}
			set
			{
				if (isWaitingForDelayedValueSet)
				{
					return;
				}
				if (ValueIsMarkedAtomic)
				{
					if (AtomHandler.Compare(value, AtomValuesArray[0]))
					{
						return;
					}
					if (!base.IsEditable)
					{
						Debug.LogWarning((object)("Tried to change value of non-editable property '" + base.Property.NiceName + "' of type '" + base.TypeOfValue.GetNiceName() + "' at path '" + base.Property.Path + "'."));
						if (!ValueIsValueType)
						{
							AtomHandler.Copy(ref AtomValuesArray[0], ref value);
						}
					}
					else
					{
						for (int i = 0; i < base.ValueCount; i++)
						{
							values[i] = value;
						}
					}
				}
				else if (ValueIsPrimitive || ValueIsValueType)
				{
					if (EqualityComparer(value, values[0]))
					{
						return;
					}
					if (!base.IsEditable)
					{
						Debug.LogWarning((object)("Tried to change value of non-editable property '" + base.Property.NiceName + "' of type '" + base.TypeOfValue.GetNiceName() + "' at path '" + base.Property.Path + "'."));
					}
					else
					{
						for (int j = 0; j < base.ValueCount; j++)
						{
							values[j] = value;
						}
					}
				}
				else
				{
					if ((object)value == (object)SmartValue)
					{
						return;
					}
					if (!base.IsEditable)
					{
						Debug.LogWarning((object)("Tried to change value of non-editable property '" + base.Property.NiceName + "' of type '" + base.TypeOfValue.GetNiceName() + "' at path '" + base.Property.Path + "'."));
					}
					else
					{
						Type type = ((SmartValue == null) ? typeof(TValue) : SmartValue.GetType());
						if (value != null && value.GetType() != type)
						{
							DelayedSmartValueReferenceSet(value);
						}
						else
						{
							SmartValueReferenceSet(value);
						}
					}
				}
			}
		}

		/// <summary>
		/// An array containing the original values as they were at the beginning of frame.
		/// </summary>
		protected TValue[] OriginalValuesArray { get; private set; }

		/// <summary>
		/// An array containing the current modified set of values.
		/// </summary>
		protected TValue[] InternalValuesArray { get; private set; }

		/// <summary>
		/// An array containing the current modified set of atomic values.
		/// </summary>
		protected TValue[] AtomValuesArray { get; private set; }

		/// <summary>
		/// An array containing the original set of atomic values.
		/// </summary>
		protected TValue[] OriginalAtomValuesArray { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.PropertyValueEntry`1" /> class.
		/// </summary>
		protected PropertyValueEntry()
		{
		}

		private void DelayedSmartValueReferenceSet(TValue value)
		{
			isWaitingForDelayedValueSet = true;
			base.Property.Tree.DelayActionUntilRepaint(delegate
			{
				isWaitingForDelayedValueSet = false;
				SmartValueReferenceSet(value);
			});
		}

		private void SmartValueReferenceSet(TValue value)
		{
			if (base.ValueCount == 1 || value == null)
			{
				for (int i = 0; i < base.ValueCount; i++)
				{
					values[i] = value;
				}
				return;
			}
			bool flag = false;
			if (base.Property.Tree.ObjectIsReferenced(value, out var referencePath))
			{
				InspectorProperty propertyAtPath = base.Property.Tree.GetPropertyAtPath(referencePath);
				if (propertyAtPath != null && propertyAtPath.Info.PropertyType == PropertyType.Value && !propertyAtPath.Info.TypeOfValue.IsValueType)
				{
					for (int j = 0; j < base.ValueCount; j++)
					{
						TValue value2 = (TValue)propertyAtPath.ValueEntry.WeakValues[j];
						values[j] = value2;
					}
					flag = true;
				}
			}
			if (!flag)
			{
				for (int k = 0; k < base.ValueCount; k++)
				{
					values[k] = value;
				}
			}
		}

		/// <summary>
		/// Initializes this value entry.
		/// </summary>
		protected override void Initialize()
		{
			OriginalValuesArray = new TValue[base.Property.Tree.WeakTargets.Count];
			InternalValuesArray = new TValue[base.Property.Tree.WeakTargets.Count];
			if (IsMarkedAtomic)
			{
				AtomValuesArray = new TValue[base.Property.Tree.WeakTargets.Count];
				OriginalAtomValuesArray = new TValue[base.Property.Tree.WeakTargets.Count];
			}
			values = new PropertyValueCollection<TValue>(base.Property, InternalValuesArray, OriginalValuesArray, AtomValuesArray, OriginalAtomValuesArray);
		}

		/// <summary>
		/// Sets the actual target tree value.
		/// </summary>
		protected abstract void SetActualValueImplementation(int index, TValue value);

		/// <summary>
		/// <para>Checks whether the values in this value entry are equal to the values in another value entry.</para>
		/// <para>Note, both value entries must have the same value type, and must represent values that are .NET value types.</para>
		/// </summary>
		public override bool ValueTypeValuesAreEqual(IPropertyValueEntry other)
		{
			if (!ValueIsValueType || !other.TypeOfValue.IsValueType || other.TypeOfValue != base.TypeOfValue)
			{
				return false;
			}
			IPropertyValueEntry<TValue> propertyValueEntry = (IPropertyValueEntry<TValue>)other;
			if (other.ValueCount == 1 || other.ValueState == PropertyValueState.None)
			{
				TValue arg = propertyValueEntry.Values[0];
				for (int i = 0; i < base.ValueCount; i++)
				{
					if (!EqualityComparer(Values[i], arg))
					{
						return false;
					}
				}
				return true;
			}
			if (base.ValueCount == 1 || base.ValueState == PropertyValueState.None)
			{
				TValue arg2 = Values[0];
				for (int j = 0; j < base.ValueCount; j++)
				{
					if (!EqualityComparer(arg2, propertyValueEntry.Values[j]))
					{
						return false;
					}
				}
				return true;
			}
			if (base.ValueCount == other.ValueCount)
			{
				for (int k = 0; k < base.ValueCount; k++)
				{
					if (!EqualityComparer(Values[k], propertyValueEntry.Values[k]))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		void IValueEntryActualValueSetter<TValue>.SetActualValue(int index, TValue value)
		{
			InternalValuesArray[index] = value;
			SetActualValueImplementation(index, value);
		}

		void IValueEntryActualValueSetter.SetActualValue(int index, object value)
		{
			InternalValuesArray[index] = (TValue)value;
			SetActualValueImplementation(index, (TValue)value);
		}

		/// <summary>
		/// <para>Determines whether the value at the given selection index is different from the given prefab value, as is relevant for prefab modification checks.</para>
		/// <para>If the value is a reference type, null and type difference is checked. If value is a value type, a comparer from <see cref="M:Sirenix.Utilities.TypeExtensions.GetEqualityComparerDelegate``1" /> is used.</para>
		/// <para>This method is best ignored unless you know what you are doing.</para>
		/// </summary>
		/// <param name="value">The value to check differences against.</param>
		/// <param name="index">The selection index to compare against.</param>
		public override bool ValueIsPrefabDifferent(object value, int index)
		{
			if (value == null)
			{
				if (ValueIsValueType)
				{
					return true;
				}
			}
			else if (ValueIsValueType)
			{
				if (typeof(TValue) != value.GetType())
				{
					return true;
				}
			}
			else if (!typeof(TValue).IsAssignableFrom(value.GetType()))
			{
				return true;
			}
			return ValueIsPrefabDifferent((TValue)value, index);
		}

		/// <summary>
		/// <para>Determines whether the value at the given selection index is different from the given prefab value, as is relevant for prefab modification checks.</para>
		/// <para>If the value is a reference type, null and type difference is checked. If value is a value type, a comparer from <see cref="M:Sirenix.Utilities.TypeExtensions.GetEqualityComparerDelegate``1" /> is used.</para>
		/// <para>This method is best ignored unless you know what you are doing.</para>
		/// </summary>
		/// <param name="value">The value to check differences against.</param>
		/// <param name="index">The selection index to compare against.</param>
		public bool ValueIsPrefabDifferent(TValue value, int index)
		{
			TValue val = Values[index];
			if (IsMarkedAtomic)
			{
				return !AtomHandler.Compare(value, val);
			}
			if (ValueIsValueType)
			{
				if (ValueIsPrimitive)
				{
					return !EqualityComparer(value, val);
				}
				return false;
			}
			if (typeof(TValue) == typeof(string))
			{
				return !EqualityComparer(value, val);
			}
			if (ValueIsUnityObject)
			{
				return (object)val != (object)value;
			}
			Type type = null;
			Type type2 = null;
			if (value != null)
			{
				type = value.GetType();
			}
			if (val != null)
			{
				type2 = val.GetType();
			}
			return type != type2;
		}
	}
	/// <summary>
	/// Represents the values of an <see cref="T:Sirenix.OdinInspector.Editor.InspectorProperty" />, and contains utilities for querying the values' type and getting and setting them.
	/// </summary>
	/// <typeparam name="TParent">The type of the parent.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.IPropertyValueEntry" />
	public sealed class PropertyValueEntry<TParent, TValue> : PropertyValueEntry<TValue>
	{
		private IValueGetterSetter<TParent, TValue> getterSetter;

		private static readonly bool ParentIsValueType = typeof(TParent).IsValueType;

		/// <summary>
		/// The type from which this value entry comes. If this value entry represents a member value, this is the declaring type of the member. If the value entry represents a collection element, this is the type of the collection.
		/// </summary>
		public sealed override Type ParentType => typeof(TParent);

		/// <summary>
		/// Determines what the most precise contained type is on this value entry.
		/// </summary>
		protected sealed override Type GetMostPreciseContainedType()
		{
			if (PropertyValueEntry<TValue>.ValueIsValueType)
			{
				return typeof(TValue);
			}
			TValue[] internalValuesArray = base.InternalValuesArray;
			Type type = null;
			for (int i = 0; i < internalValuesArray.Length; i++)
			{
				object obj = internalValuesArray[i];
				if (obj == null)
				{
					return base.Property.Info.TypeOfValue;
				}
				if (i == 0)
				{
					type = obj.GetType();
				}
				else if (type != obj.GetType())
				{
					return base.Property.Info.TypeOfValue;
				}
			}
			return type;
		}

		/// <summary>
		/// Initializes this value entry.
		/// </summary>
		protected override void Initialize()
		{
			base.Initialize();
			if (base.Property.Info.IsUnityPropertyOnly)
			{
				getterSetter = new UnityPropertyGetterSetter<TParent, TValue>(base.Property);
			}
			else if (!base.Property.Info.TryGetStrongGetterSetter(out getterSetter))
			{
				base.Property.Info.TryGetStrongGetterSetter(out getterSetter);
				throw new InvalidOperationException("Could not get proper value getter setter for property '" + base.Property.Path + "'.");
			}
		}

		/// <summary>
		/// Updates all values in this value entry from the target tree values.
		/// </summary>
		protected sealed override void UpdateValues()
		{
			for (int i = 0; i < base.ValueCount; i++)
			{
				TParent owner = GetParent(i);
				if (owner == null)
				{
					owner = GetParent(i);
				}
				TValue from = getterSetter.GetValue(ref owner);
				if (PropertyValueEntry<TValue>.ValueIsMarkedAtomic)
				{
					PropertyValueEntry<TValue>.AtomHandler.Copy(ref from, ref base.AtomValuesArray[i]);
					PropertyValueEntry<TValue>.AtomHandler.Copy(ref from, ref base.OriginalAtomValuesArray[i]);
				}
				base.OriginalValuesArray[i] = from;
				base.InternalValuesArray[i] = from;
			}
			base.Values.MarkClean();
		}

		/// <summary>
		/// Determines the value state of this value entry.
		/// </summary>
		protected sealed override PropertyValueState GetValueState()
		{
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Expected O, but got Unknown
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Expected O, but got Unknown
			TValue[] internalValuesArray = base.InternalValuesArray;
			if (!PropertyValueEntry<TValue>.ValueIsValueType && !PropertyValueEntry<TValue>.ValueIsPrimitive && !PropertyValueEntry<TValue>.ValueIsMarkedAtomic)
			{
				TValue val = internalValuesArray[0];
				if (val == null || (base.ValueIsUnityObject && (Object)(object)val == (Object)null))
				{
					for (int i = 1; i < internalValuesArray.Length; i++)
					{
						if (base.ValueIsUnityObject)
						{
							if ((Object)(object)internalValuesArray[i] != (Object)null)
							{
								return PropertyValueState.ReferenceValueConflict;
							}
						}
						else if (internalValuesArray[i] != null)
						{
							return PropertyValueState.ReferenceValueConflict;
						}
					}
					return PropertyValueState.NullReference;
				}
				if (!base.ValueIsUnityObject && base.Property.Tree.ObjectIsReferenced(val, out var referencePath) && referencePath != base.Property.Path)
				{
					bool flag = false;
					for (int j = 1; j < internalValuesArray.Length; j++)
					{
						TValue val2 = internalValuesArray[j];
						string referencePath2;
						if (val2 == null)
						{
							flag = true;
						}
						else if (!base.Property.Tree.ObjectIsReferenced(val2, out referencePath2) || referencePath2 != referencePath)
						{
							return PropertyValueState.ReferencePathConflict;
						}
					}
					if (flag)
					{
						return PropertyValueState.ReferenceValueConflict;
					}
					return PropertyValueState.Reference;
				}
				InspectorProperty inspectorProperty = base.Property;
				PropertyTree tree = inspectorProperty.Tree;
				Type type = val.GetType();
				bool flag2 = false;
				tree.ForceRegisterObjectReference(val, inspectorProperty);
				for (int k = 1; k < internalValuesArray.Length; k++)
				{
					TValue val3 = internalValuesArray[k];
					bool flag3 = val3 == null;
					if (!flag3)
					{
						tree.ForceRegisterObjectReference(val3, inspectorProperty);
					}
					if (flag3 || val3.GetType() != type)
					{
						flag2 = true;
					}
					if (base.ValueIsUnityObject && (object)val != (object)val3)
					{
						flag2 = true;
					}
				}
				if (flag2)
				{
					return PropertyValueState.ReferenceValueConflict;
				}
				ICollectionResolver collectionResolver = base.Property.ChildResolver as ICollectionResolver;
				if (collectionResolver != null && collectionResolver.CheckHasLengthConflict())
				{
					return PropertyValueState.CollectionLengthConflict;
				}
				return PropertyValueState.None;
			}
			if (PropertyValueEntry<TValue>.ValueIsMarkedAtomic)
			{
				TValue val4 = internalValuesArray[0];
				if (!PropertyValueEntry<TValue>.ValueIsValueType && val4 == null)
				{
					for (int l = 1; l < internalValuesArray.Length; l++)
					{
						if (internalValuesArray[l] != null)
						{
							return PropertyValueState.ReferenceValueConflict;
						}
					}
					return PropertyValueState.NullReference;
				}
				for (int m = 1; m < internalValuesArray.Length; m++)
				{
					if (!PropertyValueEntry<TValue>.AtomHandler.Compare(val4, internalValuesArray[m]))
					{
						return PropertyValueState.PrimitiveValueConflict;
					}
				}
				return PropertyValueState.None;
			}
			if (PropertyValueEntry<TValue>.ValueIsPrimitive || PropertyValueEntry<TValue>.ValueIsValueType)
			{
				TValue arg = internalValuesArray[0];
				for (int n = 1; n < internalValuesArray.Length; n++)
				{
					if (!PropertyValueEntry<TValue>.EqualityComparer(arg, internalValuesArray[n]))
					{
						return PropertyValueState.PrimitiveValueConflict;
					}
				}
				return PropertyValueState.None;
			}
			return PropertyValueState.None;
		}

		/// <summary>
		/// Applies the changes made to this value entry to the target objects, and registers prefab modifications as necessary.
		/// </summary>
		/// <returns>
		/// True if any changes were made, otherwise, false.
		/// </returns>
		public sealed override bool ApplyChanges()
		{
			bool result = false;
			PropertyTree tree = base.Property.Tree;
			if (base.Values.AreDirty)
			{
				base.Property.RecordForUndo();
				result = true;
				for (int i = 0; i < base.ValueCount; i++)
				{
					if (GetParent(i) == null && (!base.Property.Tree.IsStatic || (base.Property.ParentValueProperty != null && !base.Property.ParentValueProperty.IsTreeRoot)))
					{
						Debug.LogError((object)"Parent is null!");
						continue;
					}
					TValue value = base.InternalValuesArray[i];
					SetActualValueImplementation(i, value);
				}
				base.Values.MarkClean();
				for (int j = 0; j < base.ValueCount; j++)
				{
					TriggerOnValueChanged(j);
				}
				base.Property.Update(forceUpdate: true);
				for (int k = 0; k < base.ValueCount; k++)
				{
					if (base.SerializationBackend == SerializationBackend.Odin && tree.PrefabModificationHandler.HasPrefabs && tree.PrefabModificationHandler.TargetPrefabs[k] != (Object)null)
					{
						tree.PrefabModificationHandler.RegisterPrefabValueModification(base.Property, k);
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Gets the parent value at the given index.
		/// </summary>
		private TParent GetParent(int index)
		{
			if (base.Property == base.Property.Tree.RootProperty)
			{
				return (TParent)(object)index;
			}
			if (base.ParentValueProperty != null)
			{
				IPropertyValueEntry<TParent> propertyValueEntry = (IPropertyValueEntry<TParent>)base.ParentValueProperty.ValueEntry;
				return propertyValueEntry.Values[index];
			}
			return (TParent)base.Property.Tree.WeakTargets[index];
		}

		protected override void SetActualValueImplementation(int index, TValue value)
		{
			TParent owner = GetParent(index);
			if (ParentIsValueType)
			{
				getterSetter.SetValue(ref owner, value);
				if (base.ParentValueProperty != null)
				{
					((IValueEntryActualValueSetter<TParent>)base.ParentValueProperty.ValueEntry).SetActualValue(index, owner);
				}
			}
			else
			{
				getterSetter.SetValue(ref owner, value);
			}
		}
	}
}
