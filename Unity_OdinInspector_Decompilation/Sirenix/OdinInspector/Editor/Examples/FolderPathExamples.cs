namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(FolderPathAttribute), "FolderPath attribute provides a neat interface for assigning paths to strings.\nIt also supports drag and drop from the project folder.")]
	internal sealed class FolderPathExamples
	{
		[FolderPath]
		public string UnityProjectPath;

		[FolderPath(ParentFolder = "Assets/Plugins/Sirenix")]
		public string RelativeToParentPath;

		[FolderPath(ParentFolder = "Assets/Resources")]
		public string ResourcePath;

		[FolderPath(AbsolutePath = true)]
		[BoxGroup("Conditions", true, false, 0f)]
		public string AbsolutePath;

		[FolderPath(RequireExistingPath = true)]
		[BoxGroup("Conditions", true, false, 0f)]
		public string ExistingPath;

		[FolderPath(UseBackslashes = true)]
		[BoxGroup("Conditions", true, false, 0f)]
		public string Backslashes;

		[FolderPath(ParentFolder = "$DynamicParent")]
		[BoxGroup("Member referencing", true, false, 0f)]
		public string DynamicFolderPath;

		[BoxGroup("Member referencing", true, false, 0f)]
		public string DynamicParent = "Assets/Plugins/Sirenix";

		[FolderPath(ParentFolder = "Assets/Plugins/Sirenix")]
		[BoxGroup("Lists", true, false, 0f)]
		public string[] ListOfFolders;
	}
}
