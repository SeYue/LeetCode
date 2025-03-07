using System.Collections;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalDoubleArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, double> where TArray : IList
	{
		protected override double DrawElement(Rect rect, double value)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return SirenixEditorFields.DoubleField(rect.Padding(2f), value);
		}
	}
}
