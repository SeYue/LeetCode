public class 合并两个有序数组
{
	public static void Test()
	{
		int[] nums1 = [1, 2, 3, 0, 0, 0];
		int[] nums2 = [2, 5, 6];
		Run(nums1, nums1.Length - nums2.Length, nums2, nums2.Length);

		foreach (var i in nums1)
			Console.WriteLine(i);
		Console.WriteLine();

		// 2
		nums1 = [2, 0];
		nums2 = [1];
		Run(nums1, nums1.Length - nums2.Length, nums2, nums2.Length);

		foreach (var i in nums1)
			Console.WriteLine(i);
	}

	static void Run(int[] nums1, int m, int[] nums2, int n)
	{
		// 数组1没有元素
		if (m == 0)
		{
			for (int i = 0; i < n; i++)
				nums1[i] = nums2[i];
			return;
		}
		else if (n == 0)    // 数组2没有元素
			return;

		for (int i = m - 1, j = n - 1; i >= 0 || j >= 0;)
		{
			if (i < 0)
				nums1[j] = nums2[j--];
			else if (j < 0)
				nums1[i] = nums1[i--];
			else
			{
				if (nums1[i] <= nums2[j])
					nums1[i + j + 1] = nums2[j--];
				else if (nums1[i] > nums2[j])
					nums1[i + j + 1] = nums1[i--];
			}
		}
	}
}