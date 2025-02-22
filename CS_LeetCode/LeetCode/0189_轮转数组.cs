public class 轮转数组
{
	public 轮转数组()
	{
		int[] nums = { 1, 2, 3, 4, 5, 6, 7 };
		int k = 3;
		Rotate(nums, k);
		foreach (var x in nums)
		{
			Console.WriteLine(x);
		}
	}

	// 暴力解法,会超时
	public void Rotate_1(int[] nums, int k)
	{
		if (nums.Length <= 1)
		{
			return;
		}

		int length = nums.Length;
		int tempNum = 0;
		for (int i = 0; i < k; i++)
		{
			tempNum = nums[length - 1];
			for (int j = length - 1; j >= 1; j--)
			{
				nums[j] = nums[j - 1];
			}
			nums[0] = tempNum;
		}
	}

	// 一次就移动结束
	public void Rotate(int[] nums, int k)
	{
		if (nums.Length <= 1)
		{
			return;
		}

		int length = nums.Length;
		k %= length;

	}
}