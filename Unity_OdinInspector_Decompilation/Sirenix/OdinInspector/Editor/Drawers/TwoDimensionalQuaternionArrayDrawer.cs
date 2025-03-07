using System.Collections;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalQuaternionArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, Quaternion> where TArray : IList
	{
		protected override Quaternion DrawElement(Rect rect, Quaternion value)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			return SirenixEditorFields.RotationField(rect.Padding(2f), value, QuaternionDrawMode.Eulers);
		}
	}
}
