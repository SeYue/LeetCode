using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(InlineEditorAttribute), Name = "Modes", Description = "The InlineEditor has various modes that can be used for different use cases.")]
	[ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
	internal class InlineEditorModesExample
	{
		[Title("Boxed / Default", null, TitleAlignments.Left, true, true)]
		[InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		public ExampleTransform Boxed;

		[Title("Foldout", null, TitleAlignments.Left, true, true)]
		[InlineEditor(InlineEditorObjectFieldModes.Foldout)]
		public ExampleTransform Foldout;

		[Title("Hide ObjectField", null, TitleAlignments.Left, true, true)]
		[InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
		public ExampleTransform CompletelyHidden;

		[Title("Show ObjectField if null", null, TitleAlignments.Left, true, true)]
		[InlineEditor(InlineEditorObjectFieldModes.Hidden)]
		public ExampleTransform OnlyHiddenWhenNotNull;

		[OnInspectorInit]
		private void CreateData()
		{
			Boxed = ExampleHelper.GetScriptableObject<ExampleTransform>("Boxed");
			Foldout = ExampleHelper.GetScriptableObject<ExampleTransform>("Foldout");
			CompletelyHidden = ExampleHelper.GetScriptableObject<ExampleTransform>("Completely Hidden");
			OnlyHiddenWhenNotNull = ExampleHelper.GetScriptableObject<ExampleTransform>("Only Hidden When Not Null");
		}

		[OnInspectorDispose]
		private void CleanupData()
		{
			if ((Object)(object)Boxed != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)Boxed);
			}
			if ((Object)(object)Foldout != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)Foldout);
			}
			if ((Object)(object)CompletelyHidden != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)CompletelyHidden);
			}
			if ((Object)(object)OnlyHiddenWhenNotNull != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)OnlyHiddenWhenNotNull);
			}
		}
	}
}
