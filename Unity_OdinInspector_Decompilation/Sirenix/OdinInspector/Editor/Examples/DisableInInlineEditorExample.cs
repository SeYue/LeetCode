using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(DisableInInlineEditorsAttribute))]
	[ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
	internal class DisableInInlineEditorExample
	{
		[InfoBox("Click the pen icon to open a new inspector window for the InlineObject too see the difference this attribute makes.", InfoMessageType.Info, null)]
		[InlineEditor(InlineEditorModes.GUIOnly, InlineEditorObjectFieldModes.Boxed, Expanded = true)]
		public DisabledInInlineEditorScriptableObject InlineObject;

		[OnInspectorInit]
		private void CreateData()
		{
			InlineObject = ExampleHelper.GetScriptableObject<DisabledInInlineEditorScriptableObject>("Inline Object");
		}

		[OnInspectorDispose]
		private void CleanupData()
		{
			if ((Object)(object)InlineObject != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)InlineObject);
			}
		}
	}
}
