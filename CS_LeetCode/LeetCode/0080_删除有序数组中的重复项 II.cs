public class 删除有序数组中的重复项_II
{
	public 删除有序数组中的重复项_II()
	{
		int[] nums = [1, 1, 1, 2, 2, 3];
		int res = RemoveDuplicates(nums);
		Console.WriteLine($"重复项,5:{res}");
	}

	public int RemoveDuplicates(int[] nums)
	{
		if (nums.Length <= 2)
		{
			return nums.Length;
		}

		int _num;
		int tempNum = int.MinValue, tempCount = 0;
		int j = 0;
		for (int i = 0; i < nums.Length; i++)
		{
			_num = nums[i];
			if (tempNum != _num)
			{
				tempNum = _num;
				tempCount = 0;
			}

			if (tempCount < 2)
			{
				nums[j++] = _num;
			}
			tempCount++;
		}
		return j;
	}
}