using System.Collections;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalAnimationCurveArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, AnimationCurve> where TArray : IList
	{
		protected override AnimationCurve DrawElement(Rect rect, AnimationCurve value)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Expected O, but got Unknown
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (value == null)
			{
				if (GUI.Button(rect.Padding(2f), "Null - Create Animation Curve", EditorStyles.get_objectField()))
				{
					value = new AnimationCurve();
				}
				return value;
			}
			return EditorGUI.CurveField(rect.Padding(2f), value);
		}
	}
}
