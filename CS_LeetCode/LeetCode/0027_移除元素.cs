public class 移除元素
{
	public 移除元素()
	{
		int[] nums = [3, 2, 2, 3];
		int val = 3;
		int numsLength = RemoveElement(nums, val);

		Console.WriteLine($"数组1长度:{numsLength}");
		for (int i = 0; i < numsLength; i++)
		{
			Console.WriteLine(nums[i]);
		}

		nums = [0, 1, 2, 2, 3, 0, 4, 2];
		val = 2;
		numsLength = RemoveElement(nums, val);

		Console.WriteLine($"\n数组2长度:5:{numsLength}");
		for (int i = 0; i < numsLength; i++)
		{
			Console.WriteLine(nums[i]);
		}
	}

	public int RemoveElement(int[] nums, int val)
	{
		int i = 0;
		int j = 0;
		for (; i < nums.Length; i++)
		{
			if (nums[i] != val)
			{
				nums[j++] = nums[i];
			}
		}
		int[] res = new int[j];
		for (i = 0; i < j; i++)
		{
			res[i] = nums[i];
		}
		return j;
	}
}
