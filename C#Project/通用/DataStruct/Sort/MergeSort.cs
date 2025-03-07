// 归并排序
public class MergeSort
{
	// 递归实现
	public static void MergeSort1(int[] arr)
	{
		MSort(arr, arr, 0, arr.Length - 1);
	}

	static void MSort(int[] SR, int[] TR1, int s, int t)
	{
		if (s == t)
		{
			TR1[s] = SR[s];
		}
		else
		{
			int[] TR2 = new int[10000];

			int m = (s + t) / 2;
			MSort(SR, TR2, s, m);
			MSort(SR, TR2, m + 1, t);
			Merge(TR2, TR1, s, m, t);
		}
	}

	// 将有序的SR[i...m]和SR[m+1....n]归并为有序的TR[i...n]
	// SR:Source R，原数组;TR:Target R,目标数组
	static void Merge(int[] SR, int[] TR, int i, int m, int n)
	{
		int j = m + 1, k = i;
		for (; i <= m && j <= n; k++)
		{
			if (SR[i] < SR[j])
				TR[k] = SR[i++];
			else
				TR[k] = SR[j++];
		}

		if (i <= m)
		{
			for (int l = 0; l <= m - i; l++)
				TR[k + l] = SR[i + l];
		}
		if (j <= n)
		{
			for (int l = 0; l <= n - j; l++)
				TR[k + l] = SR[j + l];
		}
	}

	// 非递归实现
	public static void MergeSort2(int[] arr)
	{
		int[] TR = new int[arr.Length];
		int k = 1;
		while (k < arr.Length)
		{
			MergePass(arr, TR, k, arr.Length - 1);
			k *= 2;
			MergePass(TR, arr, k, arr.Length - 1);
			k *= 2;
		}
	}

	// 将SR[]中相邻长度为s的子序列两两归并到TR[]
	static void MergePass(int[] SR, int[] TR, int s, int n)
	{
		int i = 0, j;
		while (i <= n - 2 * s + 1)  // 两两归并
		{
			Merge(SR, TR, i, i + s - 1, i + 2 * s - 1);
			// i:0,2,4,6
			i = i + 2 * s;
		}
		if (i <= n - s + 1)
			Merge(SR, TR, i, i + s - 1, n);
		else
			for (j = i; j < n; j++)
				TR[j] = SR[j];
	}
}

public class MergeSortTest
{
	public static void Test()
	{
		int[] arr = new int[] { 50, 10, 90, 30, 70, 40, 80, 60, 20 };
		MergeSort.MergeSort2(arr);
		SortTool.ConsoleArray(arr);
	}
}
