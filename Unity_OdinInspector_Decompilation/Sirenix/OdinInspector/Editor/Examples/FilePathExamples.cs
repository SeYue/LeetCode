namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(FilePathAttribute), "FilePath attribute provides a neat interface for assigning paths to strings.\nIt also supports drag and drop from the project folder.")]
	internal class FilePathExamples
	{
		[FilePath]
		public string UnityProjectPath;

		[FilePath(ParentFolder = "Assets/Plugins/Sirenix")]
		public string RelativeToParentPath;

		[FilePath(ParentFolder = "Assets/Resources")]
		public string ResourcePath;

		[FilePath(Extensions = "cs")]
		[BoxGroup("Conditions", true, false, 0f)]
		public string ScriptFiles;

		[FilePath(AbsolutePath = true)]
		[BoxGroup("Conditions", true, false, 0f)]
		public string AbsolutePath;

		[FilePath(RequireExistingPath = true)]
		[BoxGroup("Conditions", true, false, 0f)]
		public string ExistingPath;

		[FilePath(UseBackslashes = true)]
		[BoxGroup("Conditions", true, false, 0f)]
		public string Backslashes;

		[FilePath(ParentFolder = "$DynamicParent", Extensions = "$DynamicExtensions")]
		[BoxGroup("Member referencing", true, false, 0f)]
		public string DynamicFilePath;

		[BoxGroup("Member referencing", true, false, 0f)]
		public string DynamicParent = "Assets/Plugins/Sirenix";

		[BoxGroup("Member referencing", true, false, 0f)]
		public string DynamicExtensions = "cs, unity, jpg";

		[FilePath(ParentFolder = "Assets/Plugins/Sirenix/Demos/Odin Inspector")]
		[BoxGroup("Lists", true, false, 0f)]
		public string[] ListOfFiles;
	}
}
