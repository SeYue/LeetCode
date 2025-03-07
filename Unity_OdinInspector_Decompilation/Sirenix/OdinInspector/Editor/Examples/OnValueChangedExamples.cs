using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(OnValueChangedAttribute), "OnValueChanged is used here to create a material for a shader, when the shader is changed.")]
	internal class OnValueChangedExamples
	{
		[OnValueChanged("CreateMaterial", false)]
		public Shader Shader;

		[ReadOnly]
		[InlineEditor(InlineEditorModes.LargePreview, InlineEditorObjectFieldModes.Boxed)]
		public Material Material;

		private void CreateMaterial()
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Expected O, but got Unknown
			if ((Object)(object)Material != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)Material);
			}
			if ((Object)(object)Shader != (Object)null)
			{
				Material = new Material(Shader);
			}
		}
	}
}
