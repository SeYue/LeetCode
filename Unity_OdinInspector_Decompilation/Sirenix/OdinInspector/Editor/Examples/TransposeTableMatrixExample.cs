using Sirenix.OdinInspector.Editor.Examples.Internal;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[ShowOdinSerializedPropertiesInInspector]
	[AttributeExample(typeof(TableMatrixAttribute), Name = "Transpose")]
	[ExampleAsComponentData(Namespaces = new string[] { "Sirenix.Utilities" })]
	internal class TransposeTableMatrixExample
	{
		[TableMatrix(HorizontalTitle = "Custom Cell Drawing", DrawElementMethod = "DrawColoredEnumElement", ResizableColumns = false, RowHeight = 16)]
		public bool[,] CustomCellDrawing;

		[ShowInInspector]
		[DoNotDrawAsReference]
		[TableMatrix(HorizontalTitle = "Transposed Custom Cell Drawing", DrawElementMethod = "DrawColoredEnumElement", ResizableColumns = false, RowHeight = 16, Transpose = true)]
		public bool[,] Transposed
		{
			get
			{
				return CustomCellDrawing;
			}
			set
			{
				CustomCellDrawing = value;
			}
		}

		private static bool DrawColoredEnumElement(Rect rect, bool value)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			if ((int)Event.get_current().get_type() == 0 && ((Rect)(ref rect)).Contains(Event.get_current().get_mousePosition()))
			{
				value = !value;
				GUI.set_changed(true);
				Event.get_current().Use();
			}
			EditorGUI.DrawRect(rect.Padding(1f), value ? new Color(0.1f, 0.8f, 0.2f) : new Color(0f, 0f, 0f, 0.5f));
			return value;
		}

		[OnInspectorInit]
		private void CreateData()
		{
			CustomCellDrawing = new bool[15, 15];
			CustomCellDrawing[6, 5] = true;
			CustomCellDrawing[6, 6] = true;
			CustomCellDrawing[6, 7] = true;
			CustomCellDrawing[8, 5] = true;
			CustomCellDrawing[8, 6] = true;
			CustomCellDrawing[8, 7] = true;
			CustomCellDrawing[5, 9] = true;
			CustomCellDrawing[5, 10] = true;
			CustomCellDrawing[9, 9] = true;
			CustomCellDrawing[9, 10] = true;
			CustomCellDrawing[6, 11] = true;
			CustomCellDrawing[7, 11] = true;
			CustomCellDrawing[8, 11] = true;
		}
	}
}
