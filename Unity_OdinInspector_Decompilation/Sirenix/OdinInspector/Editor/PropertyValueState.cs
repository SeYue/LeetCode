namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Enumeration for designating whether a <see cref="T:Sirenix.OdinInspector.Editor.PropertyValueEntry" /> has a special state,.
	/// </summary>
	public enum PropertyValueState
	{
		/// <summary>
		/// The value entry has no special state.
		/// </summary>
		None,
		/// <summary>
		/// The property is a reference to another property. Get the path of the referenced property from <see cref="P:Sirenix.OdinInspector.Editor.PropertyValueEntry.TargetReferencePath" />.
		/// </summary>
		Reference,
		/// <summary>
		/// The value entry is a null value.
		/// </summary>
		NullReference,
		/// <summary>
		/// <para>The value entry has a primitive value conflict across selected indices.</para>
		/// <para>A primitive value conflict is when primitive values, such a strings or floats, differ.</para>
		/// </summary>
		PrimitiveValueConflict,
		/// <summary>
		/// <para>The value entry has a reference value conflict across selected indices.</para>
		/// <para>A reference value conflict is when the types of reference type values differ, or when some values are null while others are not.</para>
		/// </summary>
		ReferenceValueConflict,
		/// <summary>
		/// <para>The value entry has a reference path conflict across selected indices.</para>
		/// <para>A reference path conflict is when the property consists of references to many conflicting paths. Use <see cref="M:Sirenix.OdinInspector.Editor.PropertyTree.ObjectIsReferenced(System.Object,System.String@)" /> to get paths to all referenced objects.</para>
		/// </summary>
		ReferencePathConflict,
		/// <summary>
		/// <para>The value entry has a collection length conflict across selected indices.</para>
		/// <para>A collection length conflict is when the property represents multiple parallel collections, and their lengths differ.</para>
		/// </summary>
		CollectionLengthConflict
	}
}
