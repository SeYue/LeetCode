using System;
using Sirenix.OdinInspector.Editor.Examples.Internal;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(CustomValueDrawerAttribute))]
	[ExampleAsComponentData(Namespaces = new string[] { "System", "UnityEditor", "System.Collections.Generic", "Sirenix.Utilities.Editor" })]
	internal class CustomValueDrawerExamples
	{
		public float From = 2f;

		public float To = 7f;

		[CustomValueDrawer("MyCustomDrawerStatic")]
		public float CustomDrawerStatic;

		[CustomValueDrawer("MyCustomDrawerInstance")]
		public float CustomDrawerInstance;

		[CustomValueDrawer("MyCustomDrawerAppendRange")]
		public float AppendRange;

		[CustomValueDrawer("MyCustomDrawerArrayNoLabel")]
		public float[] CustomDrawerArrayNoLabel = new float[3] { 3f, 5f, 6f };

		private static float MyCustomDrawerStatic(float value, GUIContent label)
		{
			return EditorGUILayout.Slider(label, value, 0f, 10f, (GUILayoutOption[])(object)new GUILayoutOption[0]);
		}

		private float MyCustomDrawerInstance(float value, GUIContent label)
		{
			return EditorGUILayout.Slider(label, value, From, To, (GUILayoutOption[])(object)new GUILayoutOption[0]);
		}

		private float MyCustomDrawerAppendRange(float value, GUIContent label, Func<GUIContent, bool> callNextDrawer)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			SirenixEditorGUI.BeginBox();
			callNextDrawer(label);
			float result = EditorGUILayout.Slider(value, From, To, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			SirenixEditorGUI.EndBox();
			return result;
		}

		private float MyCustomDrawerArrayNoLabel(float value)
		{
			return EditorGUILayout.Slider(value, From, To, (GUILayoutOption[])(object)new GUILayoutOption[0]);
		}
	}
}
