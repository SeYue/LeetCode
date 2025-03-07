using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalStringArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, string> where TArray : IList
	{
		private static GUIStyle style;

		protected override string DrawElement(Rect rect, string value)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			if (style == null)
			{
				style = new GUIStyle(EditorStyles.get_textField());
				style.set_alignment((TextAnchor)4);
			}
			return EditorGUI.TextField(new Rect(((Rect)(ref rect)).get_x(), ((Rect)(ref rect)).get_y(), ((Rect)(ref rect)).get_width() + 1f, ((Rect)(ref rect)).get_height() + 1f), value, style);
		}
	}
}
