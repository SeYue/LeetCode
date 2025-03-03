public class 买卖股票的最佳时机
{
	public 买卖股票的最佳时机()
	{
		int[] ints = [7, 1, 5, 3, 6, 4];
		var result = MaxProfit(ints);
		Console.WriteLine($"目标:5,最大利润:{result}");

		ints = [7, 6, 4, 3, 1];
		result = MaxProfit(ints);
		Console.WriteLine($"目标:0,最大利润:{result}");
	}

	// 暴力解法
	public int MaxProfit_1(int[] prices)
	{
		int maxValue = 0;
		for (int i = 0; i < prices.Length; i++)
		{
			for (int j = i + 1; j < prices.Length; j++)
			{
				if (prices[j] > prices[i] && prices[j] - prices[i] > maxValue)
				{
					maxValue = prices[j] - prices[i];
				}
			}
		}
		return maxValue;
	}

	// 记录历史最低值
	public int MaxProfit(int[] prices)
	{
		int maxValue = 0;
		int minValue = 0;
		int minValueDate = -1;
		for (int i = 0; i < prices.Length; i++)
		{
			if (minValueDate == -1)
			{
				minValueDate = i;
				minValue = prices[i];
			}
			else
			{
				if (prices[i] > minValue)
				{
					if (prices[i] - minValue > maxValue)
					{
						maxValue = prices[i] - minValue;
					}
				}
				else if (prices[i] < minValue)
				{
					minValue = prices[i];
					minValueDate = i;
				}
			}
		}
		return maxValue;
	}
}