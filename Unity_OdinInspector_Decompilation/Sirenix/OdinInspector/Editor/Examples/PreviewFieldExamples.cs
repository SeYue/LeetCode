using Sirenix.OdinInspector.Editor.Examples.Internal;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(PreviewFieldAttribute))]
	[ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
	internal class PreviewFieldExamples
	{
		[PreviewField]
		public Object RegularPreviewField;

		[VerticalGroup("row1/left", 0f)]
		public string A;

		[VerticalGroup("row1/left", 0f)]
		public string B;

		[VerticalGroup("row1/left", 0f)]
		public string C;

		[HideLabel]
		[PreviewField(50f, ObjectFieldAlignment.Right)]
		[HorizontalGroup("row1", 50f, 0, 0, 0f)]
		[VerticalGroup("row1/right", 0f)]
		public Object D;

		[HideLabel]
		[PreviewField(50f, ObjectFieldAlignment.Left)]
		[HorizontalGroup("row2", 50f, 0, 0, 0f)]
		[VerticalGroup("row2/left", 0f)]
		public Object E;

		[VerticalGroup("row2/right", 0f)]
		[LabelWidth(-54f)]
		public string F;

		[VerticalGroup("row2/right", 0f)]
		[LabelWidth(-54f)]
		public string G;

		[VerticalGroup("row2/right", 0f)]
		[LabelWidth(-54f)]
		public string H;

		[OnInspectorInit]
		private void CreateData()
		{
			RegularPreviewField = (Object)(object)ExampleHelper.GetTexture();
			D = (Object)(object)ExampleHelper.GetTexture();
			E = (Object)(object)ExampleHelper.GetTexture();
		}

		[InfoBox("These object fields can also be selectively enabled and customized globally from the Odin preferences window.\n\n - Hold Ctrl + Click = Delete Instance\n - Drag and drop = Move / Swap.\n - Ctrl + Drag = Replace.\n - Ctrl + drag and drop = Move and override.", InfoMessageType.Info, null)]
		[PropertyOrder(-1f)]
		[Button(ButtonSizes.Large)]
		private void ConfigureGlobalPreviewFieldSettings()
		{
			GlobalConfig<GeneralDrawerConfig>.Instance.OpenInEditor();
		}
	}
}
