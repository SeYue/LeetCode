// 希尔排序
public class ShellSort
{
	public static void ShellSort1(int[] arr)
	{
		int increment = arr.Length - 1;
		do
		{
			increment = increment / 3 + 1;  // 增量序列,9/3+1=4,4/3+1=2,2/3+1=1
			for (int i = increment + 1; i <= arr.Length; i++)   // i = 4+1 = 5
			{
				if (arr[i] < arr[i - increment])    // 需将arr[i]插入有序增量子表
				{
					arr[0] = arr[i];    // 暂存
					int j = i - increment;
					for (; j > 0 && arr[0] < arr[j]; j -= increment)    // 将两个数字位置调换
					{
						arr[j + increment] = arr[j];    // 记录后移，查找插入位置
					}
					arr[j + increment] = arr[0];    // 插入
				}
			}
		}
		while (increment > 1);  // 增量为1时就停止循环
	}
}

public class ShellSortTest
{
	public static void Test()
	{
		int[] arr = new int[] { 0, 9, 1, 5, 8, 3, 7, 4, 6, 2 };
		ShellSort.ShellSort1(arr);
	}
}
