using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(ShowIfGroupAttribute))]
	internal class ShowIfGroupExample
	{
		public bool Toggle = true;

		[ShowIfGroup("Toggle", true)]
		[BoxGroup("Toggle/Shown Box", true, false, 0f)]
		public int A;

		[ShowIfGroup("Toggle", true)]
		[BoxGroup("Toggle/Shown Box", true, false, 0f)]
		public int B;

		[BoxGroup("Box", true, false, 0f)]
		public InfoMessageType EnumField = InfoMessageType.Info;

		[BoxGroup("Box", true, false, 0f)]
		[ShowIfGroup("Box/Toggle", true)]
		public Vector3 X;

		[BoxGroup("Box", true, false, 0f)]
		[ShowIfGroup("Box/Toggle", true)]
		public Vector3 Y;

		[ShowIfGroup("Box/Toggle/EnumField", true, Value = InfoMessageType.Info)]
		[BoxGroup("Box/Toggle/EnumField/Border", true, false, 0f, ShowLabel = false)]
		public string Name;

		[BoxGroup("Box/Toggle/EnumField/Border", true, false, 0f)]
		public Vector3 Vector;

		[ShowIfGroup("RectGroup", true, Condition = "Toggle")]
		public Rect Rect;
	}
}
