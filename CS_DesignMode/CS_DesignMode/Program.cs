using System;
using System.Collections.Generic;

namespace CS_DesignMode
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			Console.WriteLine(1 << 0);
			Console.WriteLine(1 << 1);
			Console.WriteLine(1 << 2);
			Console.WriteLine(1 << 3);
			Console.WriteLine(1 << 4);
			Console.WriteLine(1 << 5);

			Console.WriteLine();
			Console.WriteLine(Convert.ToInt32(8));
			Console.WriteLine(8 >> 1);

			List<int> intList = new List<int>() { 4, 1, 5, 8, 2 };
			intList.Sort();
			Console.Read();
		}
	}
}
