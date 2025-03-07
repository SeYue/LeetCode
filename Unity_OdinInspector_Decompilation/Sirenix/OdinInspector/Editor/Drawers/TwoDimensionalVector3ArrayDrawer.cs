using System.Collections;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalVector3ArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, Vector3> where TArray : IList
	{
		protected override Vector3 DrawElement(Rect rect, Vector3 value)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return SirenixEditorFields.Vector3Field(rect.Padding(2f), value);
		}
	}
}
