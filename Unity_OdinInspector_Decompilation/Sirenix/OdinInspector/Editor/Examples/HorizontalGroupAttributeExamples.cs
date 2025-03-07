using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(HorizontalGroupAttribute))]
	internal class HorizontalGroupAttributeExamples
	{
		[HorizontalGroup(0f, 0, 0, 0f)]
		public int A;

		[HideLabel]
		[LabelWidth(150f)]
		[HorizontalGroup(150f, 0, 0, 0f)]
		public LayerMask B;

		[HorizontalGroup("Group 1", 0f, 0, 0, 0f, LabelWidth = 20f)]
		public int C;

		[HorizontalGroup("Group 1", 0f, 0, 0, 0f)]
		public int D;

		[HorizontalGroup("Group 1", 0f, 0, 0, 0f)]
		public int E;

		[HorizontalGroup("Split", 0.5f, 0, 0, 0f, LabelWidth = 20f)]
		[BoxGroup("Split/Left", true, false, 0f)]
		public int L;

		[BoxGroup("Split/Right", true, false, 0f)]
		public int M;

		[BoxGroup("Split/Left", true, false, 0f)]
		public int N;

		[BoxGroup("Split/Right", true, false, 0f)]
		public int O;

		[HorizontalGroup("MyButton", 0f, 0, 0, 0f, MarginLeft = 0.25f, MarginRight = 0.25f)]
		public void SomeButton()
		{
		}
	}
}
