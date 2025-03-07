using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(OnInspectorGUIAttribute))]
	internal class OnInspectorGUIExamples
	{
		[OnInspectorInit("@Texture = EditorIcons.OdinInspectorLogo")]
		[OnInspectorGUI("DrawPreview", true)]
		public Texture2D Texture;

		private void DrawPreview()
		{
			if (!((Object)(object)Texture == (Object)null))
			{
				GUILayout.BeginVertical(GUI.get_skin().get_box(), (GUILayoutOption[])(object)new GUILayoutOption[0]);
				GUILayout.Label((Texture)(object)Texture, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				GUILayout.EndVertical();
			}
		}

		[OnInspectorGUI]
		private void OnInspectorGUI()
		{
			EditorGUILayout.HelpBox("OnInspectorGUI can also be used on both methods and properties", (MessageType)1);
		}
	}
}
