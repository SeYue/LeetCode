public class 移除元素
{
	public 移除元素()
	{
		int[] nums = [3, 2, 2, 3];
		int val = 3;
		int numsLength = RemoveElement(nums, val);

		Console.WriteLine($"数组长度:{numsLength}");
		for (int i = 0; i < numsLength; i++)
		{
			Console.WriteLine(nums[i]);
		}
	}

	int RemoveElement(int[] nums, int val)
	{
		if (nums.Length == 0)
			return 0;
		int left = 0, right = nums.Length - 1;
		while (left <= right)
		{
			if (nums[left] == val)
				nums[left] = nums[right--];
			else
				left++;
		}
		return left;
	}
}
