using System;

public class C语言类型转换
{
	public static void Test()
	{
		sbyte B1 = 1;   // 1001 1110 1111
		unchecked
		{
			ushort B2 = (ushort)B1; // 1111 1111
			Console.WriteLine($"{B2}, {0b1111_1111_1111_1111}");
		}
	}
}
