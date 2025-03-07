namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(ButtonGroupAttribute))]
	internal class ButtonGroupExamples
	{
		[ButtonGroup("_DefaultGroup", 0f)]
		private void A()
		{
		}

		[ButtonGroup("_DefaultGroup", 0f)]
		private void B()
		{
		}

		[ButtonGroup("_DefaultGroup", 0f)]
		private void C()
		{
		}

		[ButtonGroup("_DefaultGroup", 0f)]
		private void D()
		{
		}

		[Button(ButtonSizes.Large)]
		[ButtonGroup("My Button Group", 0f)]
		private void E()
		{
		}

		[GUIColor(0f, 1f, 0f, 1f)]
		[ButtonGroup("My Button Group", 0f)]
		private void F()
		{
		}
	}
}
