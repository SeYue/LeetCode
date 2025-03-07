using System;
using Sirenix.Serialization;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Not yet documented.
	/// </summary>
	public struct EditableKeyValuePair<TKey, TValue> : IEquatable<EditableKeyValuePair<TKey, TValue>>
	{
		/// <summary>
		/// Not yet documented.
		/// </summary>
		[Space(2f)]
		[OdinSerialize]
		[Delayed]
		[DisableContextMenu(true, false)]
		[ShowInInspector]
		[SuppressInvalidAttributeError]
		[DoesNotSupportPrefabModifications]
		public TKey Key;

		/// <summary>
		/// Not yet documented.
		/// </summary>
		[OdinSerialize]
		[ShowInInspector]
		[OmitFromPrefabModificationPaths]
		public TValue Value;

		[NonSerialized]
		public bool IsTempKey;

		[NonSerialized]
		public bool IsInvalidKey;

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public EditableKeyValuePair(TKey key, TValue value, bool isInvalidKey, bool isTempKey)
		{
			Key = key;
			Value = value;
			IsInvalidKey = isInvalidKey;
			IsTempKey = isTempKey;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public bool Equals(EditableKeyValuePair<TKey, TValue> other)
		{
			return PropertyValueEntry<TKey>.EqualityComparer(Key, other.Key);
		}
	}
}
