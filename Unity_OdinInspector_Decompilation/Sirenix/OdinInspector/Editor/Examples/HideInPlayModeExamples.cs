namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(HideInPlayModeAttribute))]
	internal class HideInPlayModeExamples
	{
		[Title("Hidden in play mode", null, TitleAlignments.Left, true, true)]
		[HideInPlayMode]
		public int A;

		[HideInPlayMode]
		public int B;
	}
}
