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

	// 使用额外数组
	public void Rotate_2(int[] nums, int k)
	{
		if (nums.Length <= 1)
		{
			return;
		}

		int length = nums.Length;
		k %= length;

		int[] tempNums = new int[length];
		for (int i = 0; i < length; i++)
		{
			tempNums[(i + k) % length] = nums[i];
		}
		tempNums.CopyTo(nums, 0);
	}

	// 反转数组
	public void Rotate_3(int[] nums, int k)
	{
		if (nums.Length <= 1)
		{
			return;
		}

		int length = nums.Length;
		k %= length;

		Reverse(nums, 0, length - 1);
		Reverse(nums, 0, k - 1);
		Reverse(nums, k, length - 1);
	}

	void Reverse(int[] nums, int startIndex, int endIndex)
	{
		int tempNum = 0;
		for (int i = startIndex, j = endIndex; i < j; i++, j--)
		{
			tempNum = nums[i];
			nums[i] = nums[j];
			nums[j] = tempNum;
		}
	}

	// 环状替换
	public void Rotate(int[] nums, int k)
	{
		if (nums.Length <= 1)
		{
			return;
		}

		int length = nums.Length;
		k %= length;
		if (k == 0)
		{
			return;
		}

		int count = 0;
		for (int start = 0; count < length; start++)
		{
			int current = start;    // 当前循环链的索引
			int prev = nums[current];

			do
			{
				int nextIndex = (current + k) % length;

				int tempValue = nums[nextIndex];
				nums[nextIndex] = prev;

				current = nextIndex;
				prev = tempValue;

				count++;
			}
			while (current != start);
		}
	}
}