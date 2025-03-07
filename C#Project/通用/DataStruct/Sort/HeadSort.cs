public class HeadSort
{
	public static void HeadSort1(int[] arr)
	{
		int len = arr.Length - 1;
		for (int i = len / 2; i > 0; i--)    // i = 9/2=4,3,2,1
		{
			HeadAdjust(arr, i, len);
		}
		for (int i = len; i > 1; i--)   //9,8,......,2,2
		{
			SortTool.Swap(arr, 1, i);
			HeadAdjust(arr, 1, i - 1);
		}
	}

	// 堆调整是如何进行的
	/*
	 sm:
	 4	9
	 3  9
	 2  9
	 1  9

	 // 第二次for循环的堆排序
	 1	8
	 */
	static void HeadAdjust(int[] arr, int s, int m)
	{
		int temp = arr[s];  // 50
		for (int j = 2 * s; j <= m; j *= 2) // j*=2,表示遍历s结点的左孩子，因为完全二叉树中，左孩子的序号=父节点序号*2
		{
			/* sj:
			 * 3	6-7
			 */
			// j<m说明不是最后一个结点
			// arr[j]<arr[j+1]说明右孩子大于左孩子,我们的目标是找到较大值，所以应该让j++，让j指向右孩子
			if (j < m && arr[j] < arr[j + 1])
				j++;    // 3
			if (temp >= arr[j])
				break;
			arr[s] = arr[j];    // 因为arr[8]的值大于查找的根节点arr[4],所以将结点8往上移动
			s = j;  // 将空缺的8结点记录下来
		}
		arr[s] = temp;  // 将空缺位置使用根节点填补
	}
}

public class HeadSortTest
{
	public static void Test()
	{
		int[] arr = new int[] { 0, 50, 10, 90, 30, 70, 40, 80, 60, 20 };
		HeadSort.HeadSort1(arr);
	}
}