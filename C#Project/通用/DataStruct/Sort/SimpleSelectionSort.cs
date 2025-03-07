// 简单选择排序
public class SimpleSelectionSort
{
	public static void SimpleSelectionSort1(int[] arr)
	{
		for (int i = 0; i < arr.Length; i++)
		{
			int min = i;
			for (int j = i + 1; j < arr.Length; j++)
			{
				if (arr[min] > arr[j])
					min = j;
			}
			if (i != min)
				SortTool.Swap(arr, i, min);
		}
	}
}

public class SimpleSelectionSortTest
{
	public static void Test()
	{
		int[] arr = new int[] { 4, 3, 2, 1 };
		SimpleSelectionSort.SimpleSelectionSort1(arr);
		SortTool.ConsoleArray(arr);
	}
}

