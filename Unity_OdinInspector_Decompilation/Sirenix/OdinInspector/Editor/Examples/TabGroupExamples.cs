using System;
using Sirenix.OdinInspector.Editor.Examples.Internal;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(TabGroupAttribute))]
	[ExampleAsComponentData(Namespaces = new string[] { "System" })]
	internal class TabGroupExamples
	{
		[Serializable]
		public class MyTabObject
		{
			public int A;

			public int B;

			public int C;
		}

		[TabGroup("Tab A", false, 0f)]
		public int One;

		[TabGroup("Tab A", false, 0f)]
		public int Two;

		[TabGroup("Tab A", false, 0f)]
		public int Three;

		[TabGroup("Tab B", false, 0f)]
		public string MyString;

		[TabGroup("Tab B", false, 0f)]
		public float MyFloat;

		[TabGroup("Tab C", false, 0f)]
		[HideLabel]
		public MyTabObject TabC;

		[TabGroup("New Group", "Tab A", false, 0f)]
		public int A;

		[TabGroup("New Group", "Tab A", false, 0f)]
		public int B;

		[TabGroup("New Group", "Tab A", false, 0f)]
		public int C;

		[TabGroup("New Group", "Tab B", false, 0f)]
		public string D;

		[TabGroup("New Group", "Tab B", false, 0f)]
		public float E;

		[TabGroup("New Group", "Tab C", false, 0f)]
		[HideLabel]
		public MyTabObject F;
	}
}
