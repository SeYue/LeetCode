public class 爬楼梯
{
	public 爬楼梯()
	{
		int n = 2;
		int res = ClimbStairs(n);
		Console.WriteLine($"有{n}个台阶,有{res}种不同的方式到达楼顶");

		n = 3;
		res = ClimbStairs(n);
		Console.WriteLine($"有{n}个台阶,有{res}种不同的方式到达楼顶");
	}

	/* 暴力解法
	 * 如果有2个台阶，有2种方法爬到楼顶:
	 * 1 1
	 * 2
	 * 
	 *  如果有3个台阶，有3种方法爬到楼顶：
	 *  1 1 1
	 *  1 2
	 *  2 1
	 *  
	 *  如果有4个台阶，有5种方法爬到楼顶：
	 *  1 1 1 1 
	 *  2 1 1 
	 *  1 2 1
	 *  1 1 2
	 *  2 2
	 *  
	 *  如果有5个台阶，有5种方法爬到楼顶：
	 *  1 1 1 1 1
	 *  2 1 1 1
	 *  2 2 1
	 *  2 1 2
	 *  1 2 1 1
	 *  1 2 2 
	 *  1 1 2 1
	 *  1 1 1 2
	 *  2 2 1
	 */
	public int ClimbStairs(int n)
	{
		return 0;
	}
}