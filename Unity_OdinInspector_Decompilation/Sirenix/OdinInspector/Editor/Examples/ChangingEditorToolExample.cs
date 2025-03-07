using UnityEditor;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExampleDescription("Example of using EnumPaging together with OnValueChanged.")]
	[AttributeExample(typeof(EnumPagingAttribute), Order = 10f)]
	[AttributeExample(typeof(OnValueChangedAttribute), Order = 10f)]
	internal class ChangingEditorToolExample
	{
		[EnumPaging]
		[OnValueChanged("SetCurrentTool", false)]
		[InfoBox("Changing this property will change the current selected tool in the Unity editor.", InfoMessageType.Info, null)]
		public Tool sceneTool;

		private void SetCurrentTool()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			Tools.set_current(sceneTool);
		}
	}
}
