using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(InlineButtonAttribute))]
	internal class InlineButtonExamples
	{
		[InlineButton("A", null)]
		public int InlineButton;

		[InlineButton("A", null)]
		[InlineButton("B", "Custom Button Name")]
		public int ChainedButtons;

		private void A()
		{
			Debug.Log((object)"A");
		}

		private void B()
		{
			Debug.Log((object)"B");
		}
	}
}
