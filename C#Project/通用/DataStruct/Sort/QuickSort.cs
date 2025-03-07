// 快速排序
public class QuickSort
{
	public static void QuickSort1(int[] arr)
	{
		QSort(arr, 0, arr.Length - 1);
	}

	// 对顺序表L中的子序列arr[low,high]作快速排序
	static void QSort(int[] arr, int low, int high)
	{
		if (low < high)
		{
			int pivot = Partition(arr, low, high);  // 将arr[low...high]一分为二，算出枢轴值pivot
			QSort(arr, low, pivot - 1); // 对前半部分进行快排
			QSort(arr, pivot + 1, high);    // 对后半部分进行快排
		}
	}

	// 分治
	static int Partition(int[] arr, int low, int high)
	{
		// 交换顺序表arr中子表的记录，使枢轴记录到位，并返回其所在位置，此时在它之前的均不大于它，在它之后的均不小于它
		int pivotKey = arr[low];    // 用子表第一个记录作枢轴记录
		while (low < high)
		{
			while (low < high && arr[high] >= pivotKey)
				high--;
			SortTool.Swap(arr, low, high);  // 将比枢轴记录小的记录交换到低端
			while (low < high && arr[low] <= pivotKey)
				low++;
			SortTool.Swap(arr, low, high);  // 将比枢轴值大的记录交换到高端
		}
		return low;
	}
}

public class QuickSortTest
{
	public static void Test()
	{
		int[] arr = new int[] { 50, 10, 90, 30, 70, 40, 80, 60, 20 };
		QuickSort.QuickSort1(arr);
		SortTool.ConsoleArray(arr);
	}
}