using System;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// This class fixes a bug where Unity's Undo.RecordObject does not mark ScriptableObjects dirty when
	/// a change is recorded for them. It does this by subscribing to the Undo.postprocessModifications
	/// event, and marking all modified ScriptableObjects dirty manually.
	/// </summary>
	[InitializeOnLoad]
	internal static class FixUnityScriptableObjectDirtying
	{
		static FixUnityScriptableObjectDirtying()
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected O, but got Unknown
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Expected O, but got Unknown
			Undo.postprocessModifications = (PostprocessModifications)Delegate.Combine((Delegate)(object)Undo.postprocessModifications, (Delegate)(PostprocessModifications)delegate(UndoPropertyModification[] mods)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Unknown result type (might be due to invalid IL or missing references)
				try
				{
					foreach (UndoPropertyModification val in mods)
					{
						if (val.currentValue.target is ScriptableObject)
						{
							EditorUtility.SetDirty(val.currentValue.target);
						}
					}
					return mods;
				}
				catch
				{
					return mods;
				}
			});
		}
	}
}
