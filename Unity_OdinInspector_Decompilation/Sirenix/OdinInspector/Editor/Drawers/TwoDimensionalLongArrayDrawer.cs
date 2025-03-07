using System.Collections;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalLongArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, long> where TArray : IList
	{
		protected override long DrawElement(Rect rect, long value)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return SirenixEditorFields.LongField(rect.Padding(2f), value);
		}
	}
}
