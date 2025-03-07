namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(HideInEditorModeAttribute))]
	internal class HideInEditorModeExamples
	{
		[Title("Hidden in editor mode", null, TitleAlignments.Left, true, true)]
		[HideInEditorMode]
		public int C;

		[HideInEditorMode]
		public int D;
	}
}
