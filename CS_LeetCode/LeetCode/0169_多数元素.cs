// https://leetcode.cn/problems/majority-element/description/?envType=study-plan-v2&envId=top-interview-150
public class 多数元素
{
	public 多数元素()
	{
		int[] nums = [3, 2, 3];
		int res = MajorityElement(nums);
		Console.WriteLine(res);
	}

	// 暴力解法
	public int MajorityElement_1(int[] nums)
	{
		Dictionary<int, int> repeateDic = new Dictionary<int, int>();
		int maxNum = int.MinValue;
		int maxCount = 0;
		for (int i = 0; i < nums.Length; i++)
		{
			int num = nums[i];
			int count = 0;
			if (!repeateDic.ContainsKey(num))
			{
				count = repeateDic[num] = 1;
			}
			else
			{
				count = ++repeateDic[num];
			}

			if (maxNum == int.MinValue)
			{
				maxNum = num;
				maxCount = count;
			}
			else
			{
				if (count > maxCount)
				{
					maxNum = num;
					maxCount = count;
				}
			}
		}

		return maxNum;
	}

	// 同类相消解法
	public int MajorityElement(int[] nums)
	{
		int tempNum = 0;
		int currentNum = nums[0];
		int currentCount = 1;
		for (int i = 1; i < nums.Length; i++)
		{
			tempNum = nums[i];
			if (currentCount == 0)
			{
				currentNum = tempNum;
				currentCount++;
			}
			else
			{
				if (currentNum == tempNum)
				{
					currentCount++;
				}
				else
				{
					currentCount--;
				}
			}
		}
		return currentNum;
	}
}