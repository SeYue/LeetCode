namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(IndentAttribute))]
	internal class IndentExamples
	{
		[Title("Nicely organize your properties.", null, TitleAlignments.Left, true, true)]
		[Indent(1)]
		public int A;

		[Indent(2)]
		public int B;

		[Indent(3)]
		public int C;

		[Indent(4)]
		public int D;

		[Title("Using the Indent attribute", null, TitleAlignments.Left, true, true)]
		[Indent(1)]
		public int E;

		[Indent(0)]
		public int F;

		[Indent(-1)]
		public int G;
	}
}
