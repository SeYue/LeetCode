using System.Reflection.Metadata.Ecma335;

public class 买卖股票的最佳时机II
{
    public 买卖股票的最佳时机II()
    {
        int[] prices = [7, 1, 5, 3, 6, 4];
        Console.WriteLine($"目标价格:7,{MaxProfit(prices)}");
        prices = [1, 2, 3, 4, 5];
        Console.WriteLine($"目标价格:4,{MaxProfit(prices)}");
        prices = [7, 6, 4, 3, 1];
        Console.WriteLine($"目标价格:0,{MaxProfit(prices)}");
    }

    public int MaxProfit(int[] prices)
    {
        if (prices.Length <= 1)
        {
            return 0;
        }

        int minPrice = prices[0];
        int maxPrice = prices[0];
        int sellMoney = 0;
        for (int i = 1; i < prices.Length; i++)
        {
            int todayPrice = prices[i];
            if (todayPrice > maxPrice)
            {
                maxPrice = todayPrice;
            }
            else
            {
                sellMoney += maxPrice - minPrice;
                minPrice = todayPrice;
                maxPrice = todayPrice;
            }
        }
        sellMoney += maxPrice - minPrice;
        return sellMoney;
    }
}