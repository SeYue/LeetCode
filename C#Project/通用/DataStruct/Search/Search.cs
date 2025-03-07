using System;

public class Search
{
	// 二分查找
	public static int Binary_Search()
	{
		int key = 9;
		int[] a = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
		int low = 0;    // 最低下标
		int high = a.Length - 1;    // 最高下标
		int mid;

		int count = 0;
		while (low <= high)
		{
			count++;
			mid = (low + high) / 2; // 折半
			Console.WriteLine($"mid:{mid}");
			if (key < a[mid])   // 左半区
				high = mid - 1;
			else if (key > a[mid])  // 右半区
				low = mid + 1;
			else
			{
				Console.WriteLine($"次数:{count}");
				return mid;
			}
		}
		Console.WriteLine($"次数:{count}");
		return 0;
	}

	// 插值查找法
	public static int Binary_Search2()
	{
		int key = 9;
		int[] a = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
		int low = 0;    // 最低下标
		int high = a.Length - 1;    // 最高下标
		int mid;

		int count = 0;
		while (low <= high)
		{
			count++;
			mid = low + (high - low) * (key - a[low]) / (a[high] - a[low]);
			Console.WriteLine($"mid:{mid}");
			if (key < a[mid])   // 左半区
				high = mid - 1;
			else if (key > a[mid])  // 右半区
				low = mid + 1;
			else
			{
				Console.WriteLine($"次数:{count}");
				return mid;
			}
		}
		Console.WriteLine($"次数:{count}");
		return 0;
	}

	// 斐波那契查找
	public static int Fibonacci_Search()
	{
		int[] F = new int[] { 0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55 };

		int[] a = new int[] { 0, 1, 16, 24, 35, 47, 59, 62, 73, 88, 99 };
		int n = 10;

		int low = 1;
		int high = n;
		int k = 0;

		while (n > F[k] - 1)    // 计算n位斐波那契数组的位置
			k++;    // k = 7

		for (int i = n; i < F[k] - 1; i++)  // 将不满的数值补全,也就是从n开始,将这个斐波那契数列补全
			a[i] = a[n];

		int mid;
		int key = 59;
		while (low <= high)
		{
			mid = low + F[k - 1] - 1;   // 1+8-1
			if (key < a[mid])
			{
				high = mid - 1;	// 7
				k = k - 1;  // 斐波那契数列下标-1位,6
			}
			else if (key > a[mid])
			{
				low = mid + 1;	// 
				k = k - 2;  // 斐波那契数列下标-2位
			}
			else
			{
				if (mid <= n)
					return mid;
				else
					return n;
			}
		}

		return 0;
	}

	public static void Test()
	{
		int res = Fibonacci_Search();
		Console.WriteLine(res);
	}
}
