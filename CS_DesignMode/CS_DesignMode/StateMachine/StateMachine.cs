using System;

namespace CS_DesignMode
{
	public class GumballMachineTestDrive
	{
		static void Main1(string[] args)
		{
			GumballMachine gumballMachine = new GumballMachine(5);
			Console.WriteLine(gumballMachine);

			// 投入25分钱;
			Console.WriteLine("-------------");
			gumballMachine.insertQuarter();
			gumballMachine.trunCrank();
			Console.WriteLine(gumballMachine);

			Console.WriteLine("-------------");
			gumballMachine.insertQuarter();
			gumballMachine.ejectQuarter();
			gumballMachine.trunCrank();

			Console.WriteLine("-------------");
			gumballMachine.ejectQuarter();
			gumballMachine.trunCrank();
			gumballMachine.ejectQuarter();
			gumballMachine.trunCrank();

			Console.WriteLine(gumballMachine);
		}
	}

	// 糖果机
	public class GumballMachine
	{
		// 不同的状态枚举
		public const int SOLD_OUT = 0;
		public const int NO_QUARTER = 1;    // 没钱
		public const int HAS_QUARTER = 2;   // 有钱
		public const int SOLD = 3;          // 出售中

		// 状态
		int state = SOLD_OUT;
		int count = 0;

		public GumballMachine(int count)
		{
			this.count = count;
			state = count > 0 ? NO_QUARTER : SOLD_OUT;
		}

		// 投入25分钱
		public void insertQuarter()
		{
			if (state == HAS_QUARTER)
			{
				Console.WriteLine("有钱,不能投");
			}
			else if (state == SOLD_OUT)
			{
				Console.WriteLine("无道具,不能投");
			}
			else if (state == SOLD)
			{
				Console.WriteLine("请稍等,正在出货");
			}
			else if (state == NO_QUARTER)
			{
				state = HAS_QUARTER;
				Console.WriteLine("投入钱了");
			}
		}

		// 退回25分钱
		public void ejectQuarter()
		{
			if (state == HAS_QUARTER)
			{
				Console.WriteLine("退钱中");
				state = NO_QUARTER;
			}
			else if (state == NO_QUARTER)
			{
				Console.WriteLine("没钱,不能退");
			}
			else if (state == SOLD)
			{
				Console.WriteLine("已经买了,不能退");
			}
			else if (state == SOLD_OUT)
			{
				Console.WriteLine("不能退,没有钱");
			}
		}

		// 转动曲柄
		public void trunCrank()
		{
			if (state == SOLD)
			{
				Console.WriteLine("别想骗过机器拿两次糖果");
			}
			else if (state == NO_QUARTER)
			{
				Console.WriteLine("没钱,不能出糖果");
			}
			else if (state == SOLD_OUT)
			{
				Console.WriteLine("卖完了");
			}
			else if (state == HAS_QUARTER)
			{
				Console.WriteLine("正在出糖果");
				state = SOLD;
				dispense();
			}
		}

		// 发放糖果
		void dispense()
		{
			if (state == SOLD)
			{
				count -= 1;
				if (count == 0)
				{
					Console.WriteLine("发放糖果了,没有糖果了!");
					state = SOLD_OUT;
				}
				else
				{
					state = NO_QUARTER;
				}
			}
			else if (state == NO_QUARTER)
			{
				Console.WriteLine("没钱,请投钱");
			}
			else if (state == SOLD_OUT)
			{
				Console.WriteLine("接口不对");
			}
			else if (state == HAS_QUARTER)
			{
				Console.WriteLine("接口不对");
			}
		}

		// ToString()
		public override string ToString()
		{
			return string.Format("当前状态:{0},剩余糖果:{1}", state, count);
		}
	}
}
