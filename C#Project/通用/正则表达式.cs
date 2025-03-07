using System;
using System.IO;
using System.Text.RegularExpressions;

public class 正则表达式
{
	public static void Check()
	{
		string folder = @"D:\Project\Eyeah\TT2\project\trunk\TT2Plugins";
		string[] scriptFiles = Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories);
		Console.WriteLine("开始查找...");
		Console.WriteLine(string.Format("代码文件数量:{0}", scriptFiles.Length));

		foreach (var filePath in scriptFiles)
		{
			Console.WriteLine(string.Format("文件:{0}", Path.GetFileName(filePath)));
			Console.WriteLine("查找结果:");

			string text = File.ReadAllText(filePath);
			MatchCollection mc = Regex.Matches(text, @"CreateModule\s\(.*\)");
			foreach (var i in mc)
			{
				Console.WriteLine(i.ToString());
			}
		}
	}
}
