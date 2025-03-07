using System;
using System.Reflection;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	[AtomHandler]
	public sealed class GradientAtomHandler : BaseAtomHandler<Gradient>
	{
		private static readonly PropertyInfo ModeProperty = typeof(Gradient).GetProperty("mode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

		public override Gradient CreateInstance()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Expected O, but got Unknown
			return new Gradient();
		}

		protected override bool CompareImplementation(Gradient a, Gradient b)
		{
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			if (ModeProperty != null)
			{
				Enum @enum = (Enum)ModeProperty.GetValue(a, null);
				Enum obj = (Enum)ModeProperty.GetValue(b, null);
				if (!@enum.Equals(obj))
				{
					return false;
				}
			}
			if (a.get_alphaKeys().Length != b.get_alphaKeys().Length || a.get_colorKeys().Length != b.get_colorKeys().Length)
			{
				return false;
			}
			for (int i = 0; i < a.get_alphaKeys().Length; i++)
			{
				GradientAlphaKey val = a.get_alphaKeys()[i];
				GradientAlphaKey val2 = b.get_alphaKeys()[i];
				if (val.alpha != val2.alpha || val.time != val2.time)
				{
					return false;
				}
			}
			for (int j = 0; j < a.get_colorKeys().Length; j++)
			{
				GradientColorKey val3 = a.get_colorKeys()[j];
				GradientColorKey val4 = b.get_colorKeys()[j];
				if (val3.color != val4.color || val3.time != val4.time)
				{
					return false;
				}
			}
			return true;
		}

		protected override void CopyImplementation(ref Gradient from, ref Gradient to)
		{
			if (ModeProperty != null)
			{
				ModeProperty.SetValue(to, ModeProperty.GetValue(from, null), null);
			}
			to.SetKeys(from.get_colorKeys(), from.get_alphaKeys());
		}
	}
}
