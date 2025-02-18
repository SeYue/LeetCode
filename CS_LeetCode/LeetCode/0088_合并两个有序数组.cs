// https://leetcode.cn/problems/merge-sorted-array/description/?envType=study-plan-v2&envId=top-interview-150

public class 合并两个有序数组
{
	public 合并两个有序数组()
	{
		int[] nums1 = [2, 2, 3, 0, 0, 0];
		int[] nums2 = [1, 5, 6];
		Run(nums1, nums1.Length - nums2.Length, nums2, nums2.Length);

		foreach (var i in nums1)
		{
			Console.WriteLine(i);
		}

		Console.WriteLine();
	}

	void Run(int[] nums1, int m, int[] nums2, int n)
	{
		if (nums2 == null || n == 0)
		{
			return;
		}

		int k = m + n - 1;
		m--;
		n--;
		while (m >= 0 || n >= 0)
		{
			if (m >= 0 && n >= 0)
			{
				if (nums1[m] <= nums2[n])
				{
					nums1[k--] = nums2[n--];
				}
				else
				{
					nums1[k--] = nums1[m--];
				}
			}
			else if (m >= 0)
			{
				nums1[k--] = nums1[m--];
			}
			else if (n >= 0)
			{
				nums1[k--] = nums2[n--];
			}
		}
	}
}