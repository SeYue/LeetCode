using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(ShowInInlineEditorsAttribute))]
	[AttributeExample(typeof(HideInInlineEditorsAttribute))]
	[ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
	internal class ShowAndHideInInlineEditorExample
	{
		[InfoBox("Click the pen icon to open a new inspector window for the InlineObject too see the differences these attributes make.", InfoMessageType.Info, null)]
		[InlineEditor(InlineEditorModes.GUIOnly, InlineEditorObjectFieldModes.Boxed, Expanded = true)]
		public MyInlineScriptableObject InlineObject;

		[OnInspectorInit]
		private void CreateData()
		{
			InlineObject = ExampleHelper.GetScriptableObject<MyInlineScriptableObject>("Inline Object");
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
