using System;

public static class SortTool
{
	public static void Swap(int[] arr, int i, int j)
	{
		int temp = arr[i];
		arr[i] = arr[j];
		arr[j] = temp;
	}

	public static void ConsoleArray(int[] arr)
	{
		foreach (var i in arr)
			Console.Write($"{i} ");
	}
}
