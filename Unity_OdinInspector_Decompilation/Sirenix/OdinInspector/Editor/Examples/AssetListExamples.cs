using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(AssetListAttribute), "The AssetList attribute work on both lists of UnityEngine.Object types and UnityEngine.Object types, but have different behaviour.")]
	[ExampleAsComponentData(Namespaces = new string[] { "System.Collections.Generic" })]
	internal class AssetListExamples
	{
		[AssetList]
		[PreviewField(70f, ObjectFieldAlignment.Center)]
		public Texture2D SingleObject;

		[AssetList(Path = "/Plugins/Sirenix/")]
		public List<ScriptableObject> AssetList;

		[FoldoutGroup("Filtered Odin ScriptableObjects", false, 0f)]
		[AssetList(Path = "Plugins/Sirenix/")]
		public ScriptableObject Object;

		[AssetList(AutoPopulate = true, Path = "Plugins/Sirenix/")]
		[FoldoutGroup("Filtered Odin ScriptableObjects", false, 0f)]
		public List<ScriptableObject> AutoPopulatedWhenInspected;

		[AssetList(LayerNames = "MyLayerName")]
		[FoldoutGroup("Filtered AssetLists examples", 0f)]
		public GameObject[] AllPrefabsWithLayerName;

		[AssetList(AssetNamePrefix = "Rock")]
		[FoldoutGroup("Filtered AssetLists examples", 0f)]
		public List<GameObject> PrefabsStartingWithRock;

		[FoldoutGroup("Filtered AssetLists examples", 0f)]
		[AssetList(Tags = "MyTagA, MyTabB", Path = "/Plugins/Sirenix/")]
		public List<GameObject> GameObjectsWithTag;

		[FoldoutGroup("Filtered AssetLists examples", 0f)]
		[AssetList(CustomFilterMethod = "HasRigidbodyComponent")]
		public List<GameObject> MyRigidbodyPrefabs;

		private bool HasRigidbodyComponent(GameObject obj)
		{
			return (Object)(object)obj.GetComponent<Rigidbody>() != (Object)null;
		}
	}
}
