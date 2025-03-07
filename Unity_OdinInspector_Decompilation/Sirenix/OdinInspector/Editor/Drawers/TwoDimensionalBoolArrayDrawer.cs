using System.Collections;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalBoolArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, bool> where TArray : IList
	{
		protected override bool DrawElement(Rect rect, bool value)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			if ((int)Event.get_current().get_type() == 7)
			{
				return EditorGUI.Toggle(rect.AlignCenter(16f, 16f), value);
			}
			return EditorGUI.Toggle(rect, value);
		}
	}
}
