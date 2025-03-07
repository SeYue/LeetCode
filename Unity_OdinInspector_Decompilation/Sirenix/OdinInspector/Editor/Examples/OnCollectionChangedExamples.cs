using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(OnCollectionChangedAttribute), "The following example shows how OnCollectionChanged can be used to get callbacks when a collection is being changed. Note that CollectionChangeInfo is an editor-only struct located in the Sirenix.OdinInspector.Editor namespace and must be accessed inside an #if UNITY_EDITOR scope.")]
	[ShowOdinSerializedPropertiesInInspector]
	[ExampleAsComponentData(Namespaces = new string[] { "System.Collections.Generic", "Sirenix.OdinInspector.Editor" })]
	internal class OnCollectionChangedExamples
	{
		[InfoBox("Change the collection to get callbacks detailing the changes that are being made.", InfoMessageType.Info, null)]
		[OnCollectionChanged("Before", "After")]
		public List<string> list = new List<string> { "str1", "str2", "str3" };

		[OnCollectionChanged("Before", "After")]
		public HashSet<string> hashset = new HashSet<string> { "str1", "str2", "str3" };

		[OnCollectionChanged("Before", "After")]
		public Dictionary<string, string> dictionary = new Dictionary<string, string>
		{
			{ "key1", "str1" },
			{ "key2", "str2" },
			{ "key3", "str3" }
		};

		public void Before(CollectionChangeInfo info, object value)
		{
			Debug.Log((object)string.Concat("Received callback BEFORE CHANGE with the following info: ", info, ", and the following collection instance: ", value));
		}

		public void After(CollectionChangeInfo info, object value)
		{
			Debug.Log((object)string.Concat("Received callback AFTER CHANGE with the following info: ", info, ", and the following collection instance: ", value));
		}
	}
}
