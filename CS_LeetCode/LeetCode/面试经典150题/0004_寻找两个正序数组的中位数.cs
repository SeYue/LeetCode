public class 寻找两个正序数组的中位数
{
	public 寻找两个正序数组的中位数()
	{
		int[] nums1 = [1, 3];
		int[] nums2 = [2];
		double result = FindMedianSortedArrays(nums1, nums2);
		Console.WriteLine(result);

		nums1 = [1, 2];
		nums2 = [3, 4];
		result = FindMedianSortedArrays(nums1, nums2);
		Console.WriteLine(result);

		nums1 = [];
		nums2 = [3];
		result = FindMedianSortedArrays(nums1, nums2);
		Console.WriteLine(result);
	}

	// 暴力解法:
	// 先合并两个数组，如果数组长度是单数，则中位数是中间位置的数，如果数组长度是双数，则中位数是中间两个数字相加然后除以2。
	public double FindMedianSortedArrays_1(int[] nums1, int[] nums2)
	{
		int nums1Length = nums1.Length;
		int nums2Length = nums2.Length;

		// 合并两个有序数组
		int[] sumArr = new int[nums1Length + nums2Length];
		for (int i = 0, j = 0, k = 0; k < sumArr.Length;)
		{
			if (i < nums1Length && j >= nums2Length)
			{
				sumArr[k++] = nums1[i++];
			}
			else if (i >= nums1Length && j < nums2Length)
			{
				sumArr[k++] = nums2[j++];
			}
			else if (i < nums1Length && j < nums2Length && nums1[i] <= nums2[j])
			{
				sumArr[k++] = nums1[i++];
			}
			else if (i < nums1Length && j < nums2Length && nums1[i] > nums2[j])
			{
				sumArr[k++] = nums2[j++];
			}
		}

		// 求中位数
		if (sumArr.Length % 2 == 0)
		{
			return (sumArr[sumArr.Length / 2 - 1] + sumArr[sumArr.Length / 2]) / 2f;
		}
		else if (sumArr.Length % 2 == 1)
		{
			return sumArr[sumArr.Length / 2];
		}
		return 0;
	}

	// 完美解法:
	// 求第K小的数
	public double FindMedianSortedArrays(int[] nums1, int[] nums2)
	{
		int nums1Length = nums1.Length;
		int nums2Length = nums2.Length;
		int mergeLength = nums1Length + nums2Length;

		int[] sumArr = new int[Math.Max(1, mergeLength / 2 + 1)];
		// 一路遍历，直到中位数的位置，然后停止
		for (int i = 0, j = 0, k = 0; k < sumArr.Length;)
		{
			if (i < nums1Length && j >= nums2Length)
			{
				sumArr[k++] = nums1[i++];
			}
			else if (i >= nums1Length && j < nums2Length)
			{
				sumArr[k++] = nums2[j++];
			}
			else if (i < nums1Length && j < nums2Length && nums1[i] <= nums2[j])
			{
				sumArr[k++] = nums1[i++];
			}
			else if (i < nums1Length && j < nums2Length && nums1[i] > nums2[j])
			{
				sumArr[k++] = nums2[j++];
			}
		}

		if (mergeLength % 2 == 0)
		{
			return (sumArr[sumArr.Length - 1] + sumArr[sumArr.Length - 2]) / 2f;
		}
		else if (mergeLength % 2 == 1)
		{
			return sumArr[sumArr.Length - 1];
		}
		return 0;
	}


}