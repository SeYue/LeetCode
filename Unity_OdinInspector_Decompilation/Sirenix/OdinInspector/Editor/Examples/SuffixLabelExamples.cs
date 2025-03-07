using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(SuffixLabelAttribute), "The SuffixLabel attribute draws a label at the end of a property. It's useful for conveying intend about a property.")]
	internal class SuffixLabelExamples
	{
		[SuffixLabel("Prefab", false)]
		public GameObject GameObject;

		[Space(15f)]
		[InfoBox("Using the Overlay property, the suffix label will be drawn on top of the property instead of behind it.\nUse this for a neat inline look.", InfoMessageType.Info, null)]
		[SuffixLabel("ms", false, Overlay = true)]
		public float Speed;

		[SuffixLabel("radians", false, Overlay = true)]
		public float Angle;

		[Space(15f)]
		[InfoBox("The Suffix attribute also supports referencing a member string field, property, or method by using $.", InfoMessageType.Info, null)]
		[SuffixLabel("$Suffix", false, Overlay = true)]
		public string Suffix = "Dynamic suffix label";

		[InfoBox("The Suffix attribute also supports expressions by using @.", InfoMessageType.Info, null)]
		[SuffixLabel("@DateTime.Now.ToString(\"HH:mm:ss\")", true)]
		public string Expression;
	}
}
