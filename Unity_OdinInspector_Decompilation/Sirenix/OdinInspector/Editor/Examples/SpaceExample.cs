using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(SpaceAttribute))]
	[AttributeExample(typeof(PropertySpaceAttribute))]
	internal class SpaceExample
	{
		[Space]
		public int Space;

		[PropertySpace(SpaceBefore = 0f, SpaceAfter = 60f)]
		[PropertyOrder(2f)]
		public int BeforeAndAfter;

		[ShowInInspector]
		[PropertySpace]
		public string Property { get; set; }
	}
}
