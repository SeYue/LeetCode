using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[ShowOdinSerializedPropertiesInInspector]
	[AttributeExample(typeof(ShowPropertyResolverAttribute), Description = "The ShowPropertyResolver attribute allows you to debug how your properties are handled by Odin behind the scenes.")]
	[ExampleAsComponentData(Namespaces = new string[] { "System.Collections.Generic" })]
	internal class ShowPropertyResolverExample
	{
		[ShowPropertyResolver]
		public string MyString;

		[ShowPropertyResolver]
		public List<int> MyList;

		[ShowPropertyResolver]
		public Dictionary<int, Vector3> MyDictionary;
	}
}
