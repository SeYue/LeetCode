using System;

// 斐波那契数列
public class Fibonacci
{
	public static void Test()
	{
		Console.WriteLine(Fbi2(5));
	}

	// 用递归来实现
	public static int Fbi(int count)
	{
		if (count == 0)
			return 0;
		if (count == 1)
			return 1;
		return Fbi(count - 2) + Fbi(count - 1);
	}

	// 用数组来实现
	public static int Fbi2(int count)
	{
		int[] arr = new int[count + 1];
		arr[0] = 0;
		arr[1] = 1;
		for (int i = 2; i <= count; i++)
		{
			arr[i] = arr[i - 2] + arr[i - 1];
		}
		return arr[arr.Length - 1];
	}
}
