// 直接插入排序
public class StraightInsertionSort
{
	public static void InsertSort(int[] arr)
	{
		for (int i = 2; i < arr.Length; i++)
		{
			if (arr[i] < arr[i - 1])
			{
				int temp = arr[i];
				int j = i - 1;
				for (; arr[j] > temp; j--)
					arr[j + 1] = arr[j];
				arr[j + 1] = temp;
			}
		}
	}
}

public class StraightInsertionSortTest
{
	public static void Test()
	{
		int[] arr = new int[] { 0, 5, 3, 4, 6, 2 };
		StraightInsertionSort.InsertSort(arr);
		SortTool.ConsoleArray(arr);
	}
}