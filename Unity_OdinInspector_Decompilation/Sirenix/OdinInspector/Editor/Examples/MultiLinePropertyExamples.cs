using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(MultilineAttribute))]
	[AttributeExample(typeof(MultiLinePropertyAttribute))]
	internal class MultiLinePropertyExamples
	{
		[Multiline(10)]
		public string UnityMultilineField = "";

		[Title("Wide Multiline Text Field", null, TitleAlignments.Left, true, false)]
		[HideLabel]
		[MultiLineProperty(10)]
		public string WideMultilineTextField = "";

		[InfoBox("Odin supports properties, but Unity's own Multiline attribute only works on fields.", InfoMessageType.Info, null)]
		[ShowInInspector]
		[MultiLineProperty(10)]
		public string OdinMultilineProperty { get; set; }
	}
}
