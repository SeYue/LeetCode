namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(PropertyOrderAttribute))]
	internal class PropertyOrderExamples
	{
		[PropertyOrder(1f)]
		public int Second;

		[InfoBox("PropertyOrder is used to change the order of properties in the inspector.", InfoMessageType.Info, null)]
		[PropertyOrder(-1f)]
		public int First;
	}
}
