using System;

public class BubbleSort
{
	// 交换排序
	public static void BubbleSort1(int[] arr)
	{
		for (int i = 0; i < arr.Length; i++)
		{
			for (int j = i + 1; j < arr.Length; j++)
			{
				Console.WriteLine($"比较:{arr[i]}\t {arr[j]}");
				if (arr[i] > arr[j])
				{
					SortTool.Swap(arr, i, j);
				}
			}
			Console.WriteLine("比较结束,i++");
		}
		Console.WriteLine("比较结束-------");
		foreach (var i in arr)
			Console.Write($"{i} ");
	}

	// 这才是冒泡排序
	public static void BubbleSort2(int[] arr)
	{
		for (int i = 0; i < arr.Length; i++)
		{
			for (int j = arr.Length - 2; j >= i; j--)
			{
				Console.WriteLine($"比较:{arr[j]}\t {arr[j + 1]}");
				if (arr[j] > arr[j + 1])
				{
					SortTool.Swap(arr, j, j + 1);
				}
			}
			Console.WriteLine("比较结束,i++");
		}
		Console.WriteLine("比较结束-------");
		foreach (var i in arr)
			Console.Write($"{i} ");
	}

	// 冒泡排序优化
	public static void BubbleSort3(int[] arr)
	{
		bool flag = true;  // 当前i循环是否已经交换数据
		for (int i = 0; i < arr.Length && flag; i++)
		{
			flag = false;
			for (int j = arr.Length - 2; j >= i; j--)
			{
				Console.WriteLine($"比较:{arr[j]}\t {arr[j + 1]}");
				if (arr[j] > arr[j + 1])
				{
					SortTool.Swap(arr, j, j + 1);
					flag = true;
				}
			}
			Console.WriteLine("比较结束,i++");
		}
		Console.WriteLine("比较结束-------");
		foreach (var i in arr)
			Console.Write($"{i} ");
	}
}

public class BubbleSortTest
{
	public static void Test()
	{
		int[] arr = new int[] { 10, 9, 8, 6, 7, 5, 4, 3, 1, 2 };
		BubbleSort.BubbleSort3(arr);
	}
}
