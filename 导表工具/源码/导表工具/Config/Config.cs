using System.IO;

namespace 导表工具
{
	public class Config
	{
		public static string cmdPath = Directory.GetCurrentDirectory();
		// Excel表文件夹
		public const string excelPath = "Excel";
		public const string outputDirectory = "Output";
		public static string outputDll = Path.Combine(cmdPath, outputDirectory + "\\CS\\Dll");
		public static string outputBytes = Path.Combine(cmdPath, outputDirectory + "\\CS\\bytes");

		// dll
		public const string dllName = "EXCEL_DATA";
		public static string dllPath = $"{outputDll}\\{dllName}.dll";
		public static string pdbPath = $"{outputDll}\\{dllName}.pdb";

		// Copy到游戏目录,配置自己的目录
		public const string dllCopyDirectory = @"..\\导表工具\ExcelDll";
		public const string bytesCopyDirectory = @"D:\Project\Github\SevenObsidian\Project\导表工具\Bytes";
	}
}
