using System.Collections;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalIntArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, int> where TArray : IList
	{
		protected override int DrawElement(Rect rect, int value)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return SirenixEditorFields.IntField(rect.Padding(2f), value);
		}
	}
}
