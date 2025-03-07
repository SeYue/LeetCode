using System.Collections;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalFloatArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, float> where TArray : IList
	{
		protected override float DrawElement(Rect rect, float value)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return SirenixEditorFields.FloatField(rect.Padding(2f), value);
		}
	}
}
