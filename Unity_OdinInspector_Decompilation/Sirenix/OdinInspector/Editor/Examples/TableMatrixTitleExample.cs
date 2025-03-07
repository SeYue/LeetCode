namespace Sirenix.OdinInspector.Editor.Examples
{
	[ShowOdinSerializedPropertiesInInspector]
	[AttributeExample(typeof(TableMatrixAttribute), "You can specify custom labels for both the the rows and columns of the table.")]
	internal class TableMatrixTitleExample
	{
		[TableMatrix(HorizontalTitle = "Read Only Matrix", IsReadOnly = true)]
		public int[,] ReadOnlyMatrix = new int[5, 5];

		[TableMatrix(HorizontalTitle = "X axis", VerticalTitle = "Y axis")]
		public InfoMessageType[,] LabledMatrix = new InfoMessageType[6, 6];
	}
}
