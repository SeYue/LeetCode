using System;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// A polymorphic alias for getting and setting the values of an <see cref="T:Sirenix.OdinInspector.Editor.IValueGetterSetter`2" />.
	/// </summary>
	/// <typeparam name="TOwner">The type of the owner.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	/// <typeparam name="TPropertyOwner">The type of the property owner.</typeparam>
	/// <typeparam name="TPropertyValue">The type of the property value.</typeparam>
	public class AliasGetterSetter<TOwner, TValue, TPropertyOwner, TPropertyValue> : IValueGetterSetter<TOwner, TValue>, IValueGetterSetter
	{
		private IValueGetterSetter<TPropertyOwner, TPropertyValue> aliasedGetterSetter;

		/// <summary>
		/// Gets the type of the owner.
		/// </summary>
		public Type OwnerType => typeof(TOwner);

		/// <summary>
		/// Gets the type of the value.
		/// </summary>
		public Type ValueType => typeof(TValue);

		/// <summary>
		/// Whether the value is readonly.
		/// </summary>
		public bool IsReadonly => aliasedGetterSetter.IsReadonly;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.AliasGetterSetter`4" /> class.
		/// </summary>
		/// <param name="aliasedGetterSetter">The information.</param>
		/// <exception cref="T:System.ArgumentNullException">info</exception>
		public AliasGetterSetter(IValueGetterSetter<TPropertyOwner, TPropertyValue> aliasedGetterSetter)
		{
			if (aliasedGetterSetter == null)
			{
				throw new ArgumentNullException("info");
			}
			this.aliasedGetterSetter = aliasedGetterSetter;
		}

		/// <summary>
		/// Gets the value from a given weakly typed owner.
		/// </summary>
		/// <param name="owner">The weakly typed owner.</param>
		/// <returns>The found value.</returns>
		public object GetValue(object owner)
		{
			TOwner owner2 = (TOwner)owner;
			return GetValue(ref owner2);
		}

		/// <summary>
		/// Gets the value from a given owner.
		/// </summary>
		/// <param name="owner">The owner.</param>
		/// <exception cref="T:System.ArgumentNullException">owner is null</exception>
		public TValue GetValue(ref TOwner owner)
		{
			TPropertyOwner owner2 = (TPropertyOwner)(object)owner;
			return (TValue)(object)aliasedGetterSetter.GetValue(ref owner2);
		}

		/// <summary>
		/// Sets the weakly typed value on a given weakly typed owner.
		/// </summary>
		/// <param name="owner">The owner.</param>
		/// <param name="value">The value.</param>
		public void SetValue(object owner, object value)
		{
			TOwner owner2 = (TOwner)owner;
			SetValue(ref owner2, (TValue)value);
		}

		/// <summary>
		/// Sets the value on a given owner.
		/// </summary>
		/// <param name="owner">The owner.</param>
		/// <param name="value">The value.</param>
		public void SetValue(ref TOwner owner, TValue value)
		{
			TPropertyOwner owner2 = (TPropertyOwner)(object)owner;
			aliasedGetterSetter.SetValue(ref owner2, (TPropertyValue)(object)value);
			owner = (TOwner)(object)owner2;
		}
	}
}
