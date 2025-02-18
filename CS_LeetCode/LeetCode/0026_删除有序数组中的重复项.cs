public class 删除有序数组中的重复项
{
	public 删除有序数组中的重复项()
	{
		int[] ints = { 1, 1, 2 };
		int res = RemoveDuplicates(ints);
		Console.WriteLine($"结果数组长度:{res}");
	}

	public int RemoveDuplicates(int[] nums)
	{
		int j = 0;
		for (int i = 0; i < nums.Length; i++)
		{
			if (nums[i] != nums[j])
			{
				nums[++j] = nums[i];
			}
		}
		return j + 1;
	}
}