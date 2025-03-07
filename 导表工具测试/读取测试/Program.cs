using System;

namespace 读取测试
{
	class Program
	{
		static void Main(string[] args)
		{
			ExcelLoaderConfig.bytesDirectory = @"D:\Project\Github\SevenObsidian\Project\导表工具测试\读取测试\bytes\";
			Console.WriteLine(ITEM_DIC.Para[10001].NAME);
		}
	}
}
