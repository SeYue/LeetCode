namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Note: this interface may be temporary, and may eventually be substituted for a public-facing way of extending the prefab modification system.
	/// <para />
	/// For now, it only exists to denote which internally defined resolvers support prefab modifications being set.
	/// </summary>
	internal interface IMaySupportPrefabModifications
	{
		bool MaySupportPrefabModifications { get; }
	}
}
