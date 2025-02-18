/*
给定一个整数数组 nums 和一个整数目标值 target，请你在该数组中找出 和为目标值 target  的那 两个 整数，并返回它们的数组下标。
你可以假设每种输入只会对应一个答案，并且你不能使用两次相同的元素。
你可以按任意顺序返回答案。

示例 1：
输入：nums = [2,7,11,15], target = 9
输出：[0,1]
解释：因为 nums[0] + nums[1] == 9 ，返回 [0, 1] 。

示例 2：
输入：nums = [3,2,4], target = 6
输出：[1,2]

示例 3：
输入：nums = [3,3], target = 6
输出：[0,1]
 */
using System.Text;

public class 两数之和
{
	public 两数之和()
	{
		int[] nums = new[] { 1, 1, 1, 1, 1, 4, 1, 1, 1, 1, 1, 7, 1, 1, 1, 1, 1 };
		var target = 11;
		int[] result = TwoSum(nums, target);

		StringBuilder sb = new StringBuilder();
		foreach (var i in nums)
		{
			sb.Append(i + " ");
		}
		sb.AppendLine("\n\n结果:");
		foreach (var i in result)
		{
			sb.Append(i + " ");
		}
		Console.WriteLine(sb.ToString());
	}

	public int[] TwoSum(int[] nums, int target)
	{
		Dictionary<int, int> dic = new Dictionary<int, int>();
		int _index = 0;
		for (int i1 = 0, j1 = nums.Length - 1; i1 <= j1; i1++, j1--)
		{
			if (dic.TryGetValue(target - nums[i1], out _index))
			{
				return new int[] { i1, _index };
			}
			else
			{
				dic.TryAdd(nums[i1], i1);
			}

			if (dic.TryGetValue(target - nums[j1], out _index))
			{
				return new int[] { j1, _index };
			}
			else
			{
				dic.TryAdd(nums[j1], j1);
			}
		}
		return null;
	}
}
