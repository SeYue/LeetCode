using System;
using System.Collections.Generic;
using System.Text;

namespace 导表工具
{
	public static class Log
	{
		public static void Space()
		{
			Console.WriteLine();
		}

		// 信息
		public static void Info(string log)
		{
			Console.WriteLine(log);
		}

		// 警告
		public static void Warrning(string log)
		{
			InfoColor(log, ConsoleColor.Yellow);
		}

		// 错误
		public static void Error(string log)
		{
			InfoColor(log, ConsoleColor.Red);
			//Console.ReadKey();
		}

		public static void InfoColor(string log, ConsoleColor color)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(log);
			Console.ResetColor();
		}
	}
}
