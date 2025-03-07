using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	[AtomHandler]
	public sealed class AnimationCurveAtomHandler : BaseAtomHandler<AnimationCurve>
	{
		public override AnimationCurve CreateInstance()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Expected O, but got Unknown
			return new AnimationCurve();
		}

		protected override bool CompareImplementation(AnimationCurve a, AnimationCurve b)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			if (a.get_postWrapMode() != b.get_postWrapMode() || a.get_preWrapMode() != b.get_preWrapMode() || a.get_keys().Length != b.get_keys().Length)
			{
				return false;
			}
			for (int i = 0; i < a.get_keys().Length; i++)
			{
				Keyframe x = a.get_keys()[i];
				Keyframe y = b.get_keys()[i];
				if (!EqualityComparer<Keyframe>.Default.Equals(x, y))
				{
					return false;
				}
			}
			return true;
		}

		protected override void CopyImplementation(ref AnimationCurve from, ref AnimationCurve to)
		{
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			to.set_postWrapMode(from.get_postWrapMode());
			to.set_preWrapMode(from.get_preWrapMode());
			while (to.get_keys().Length > from.get_keys().Length)
			{
				to.RemoveKey(to.get_keys().Length - 1);
			}
			while (to.get_keys().Length < from.get_keys().Length)
			{
				to.AddKey(Random.Range(0f, 1f), 0f);
			}
			for (int i = 0; i < to.get_keys().Length; i++)
			{
				to.MoveKey(i, from.get_keys()[i]);
			}
		}
	}
}
