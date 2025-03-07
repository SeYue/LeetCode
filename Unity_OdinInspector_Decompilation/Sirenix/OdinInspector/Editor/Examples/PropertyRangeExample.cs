using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(RangeAttribute))]
	[AttributeExample(typeof(PropertyRangeAttribute))]
	internal class PropertyRangeExample
	{
		[Range(0f, 10f)]
		public int Field = 2;

		[InfoBox("You can also reference member for either or both min and max values.", InfoMessageType.Info, null)]
		[PropertyRange(0.0, "Max")]
		[PropertyOrder(3f)]
		public int Dynamic = 6;

		[PropertyOrder(4f)]
		public int Max = 100;

		[InfoBox("Odin's PropertyRange attribute is similar to Unity's Range attribute, but also works on properties.", InfoMessageType.Info, null)]
		[ShowInInspector]
		[PropertyRange(0.0, 10.0)]
		public int Property { get; set; }
	}
}
