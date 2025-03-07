using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	[CustomEditor(typeof(SirenixPathLookupScriptableObject))]
	internal class SirenixPathLookupScriptableObjectEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			GUILayout.Label("Don't delete this file!", SirenixGUIStyles.SectionHeader, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			GUILayout.Space(15f);
			GUILayout.Label("Odin will use this file to locate where Odin is installed. That way you can freely move around the Sirenix folder and place it anywhere you like.", SirenixGUIStyles.MultiLineLabel, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			GUILayout.Space(15f);
			GUILayout.Label("<b>Why does Odin need to know where it's located?</b>", SirenixGUIStyles.MultiLineLabel, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			GUILayout.Label("There are a number of features that need to know the location of Odin. Odin will use it to make sure you get a smooth update, without you needing to manually delete files that are no longer needed. It'll also auto-generate various config files in the correct locations, so we don't need to include those in the UnityPackage, making sure we never override any of your settings when you update. Many other systems also use it, like the assembly import settings automation, the AOT support generation, the Editor Only Mode switching logic, and so on.", SirenixGUIStyles.MultiLineLabel, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			SirenixEditorGUI.DrawThickHorizontalSeperator(4f, 10f, 10f);
			GUILayout.Label("Expected Asset Guid: 08379ccefc05200459f90a1c0711a340", (GUILayoutOption[])(object)new GUILayoutOption[0]);
			string assetPath = AssetDatabase.GetAssetPath(((Editor)this).get_target());
			string text = ((assetPath == null) ? "Not an Asset" : AssetDatabase.AssetPathToGUID(assetPath));
			GUILayout.Label("Actual Asset Guid:     " + text, (GUILayoutOption[])(object)new GUILayoutOption[0]);
		}

		public SirenixPathLookupScriptableObjectEditor()
			: this()
		{
		}
	}
}
