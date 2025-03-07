namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(BoxGroupAttribute), Order = 10f)]
	[AttributeExample(typeof(ButtonGroupAttribute), Order = 10f)]
	[AttributeExample(typeof(TitleGroupAttribute), Order = 10f)]
	internal class BigTitleGroupExample
	{
		[BoxGroup("Titles", true, false, 0f, ShowLabel = false)]
		[TitleGroup("Titles/First Title", null, TitleAlignments.Left, true, true, false, 0f)]
		public int A;

		[BoxGroup("Titles/Boxed", true, false, 0f)]
		[TitleGroup("Titles/Boxed/Second Title", null, TitleAlignments.Left, true, true, false, 0f)]
		public int B;

		[TitleGroup("Titles/Boxed/Second Title", null, TitleAlignments.Left, true, true, false, 0f)]
		public int C;

		[TitleGroup("Titles/Horizontal Buttons", null, TitleAlignments.Left, true, true, false, 0f)]
		[ButtonGroup("Titles/Horizontal Buttons/Buttons", 0f)]
		public void FirstButton()
		{
		}

		[ButtonGroup("Titles/Horizontal Buttons/Buttons", 0f)]
		public void SecondButton()
		{
		}
	}
}
