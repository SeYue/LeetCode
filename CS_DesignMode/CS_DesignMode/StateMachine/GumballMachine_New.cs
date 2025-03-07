using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS_DesignMode
{
	public class GumballMachine_New
	{
		public GumballMachineState soldOutState;
		public GumballMachineState soldState;
		public GumballMachineState hasQuarterState;
		public GumballMachineState noQuarterState;

		public GumballMachineState m_state;
		public int count = 0;

		public GumballMachine_New(int count)
		{
			soldOutState = new SoldOutState(this);
			soldState = new SoldState(this);
			hasQuarterState = new HasQuarterState(this);
			noQuarterState = new NoQuarterState(this);

			m_state = count > 0 ? noQuarterState : soldOutState;
			this.count = count;
		}

		public void insertQuarter()
		{
			m_state.insertQuarter();
		}

		public void ejectQuarter()
		{
			m_state.ejectQuarter();
		}

		public void turnCrank()
		{
			m_state.turnCrank();
			m_state.dispense();
		}

		public void setState(GumballMachineState state)
		{
			this.m_state = state;
		}

		public void releaseBall()
		{
			Console.WriteLine("正在出糖果");
			if (count > 0)
			{
				count -= 1;
			}
		}
	}

	// 状态基类
	public interface GumballMachineState
	{
		void insertQuarter();
		void ejectQuarter();
		void turnCrank();
		void dispense();
	}

	// 出售状态
	public class SoldState : GumballMachineState
	{
		GumballMachine_New gumballMachine;

		public SoldState(GumballMachine_New gumballMachine)
		{
			this.gumballMachine = gumballMachine;
		}

		public void dispense() { }

		public void ejectQuarter() { }

		public void insertQuarter()
		{
			Console.WriteLine("[出售状态, 投钱]");
		}

		public void turnCrank() { }
	}

	// 售完状态
	public class SoldOutState : GumballMachineState
	{
		GumballMachine_New gumballMachine;

		public SoldOutState(GumballMachine_New gumballMachine)
		{
			this.gumballMachine = gumballMachine;
		}

		public void dispense()
		{
			Console.WriteLine("[出售状态,出糖果]出糖果成功");
			gumballMachine.releaseBall();
			if (gumballMachine.count > 0)
				gumballMachine.setState(gumballMachine.noQuarterState);
			else
				gumballMachine.setState(gumballMachine.soldOutState);
		}

		public void ejectQuarter() { Console.WriteLine("[出售状态,退钱]不能再退钱了"); }

		public void insertQuarter() { Console.WriteLine("[出售状态,投币]不能投币"); }

		public void turnCrank() { Console.WriteLine("[出售状态,转动曲柄]不能"); }
	}

	// 没有25分钱
	public class NoQuarterState : GumballMachineState
	{
		GumballMachine_New gumballMachine;

		public NoQuarterState(GumballMachine_New gumballMachine)
		{
			this.gumballMachine = gumballMachine;
		}

		public void dispense() { Console.WriteLine("[没钱状态,出糖果]不能出糖果"); }

		public void ejectQuarter() { Console.WriteLine("[没钱状态,退钱]不能退钱"); }

		public void insertQuarter()
		{
			Console.WriteLine("[没钱状态,投钱]投钱了");
			gumballMachine.setState(gumballMachine.hasQuarterState);
		}

		public void turnCrank() { Console.WriteLine("[没钱状态,转动曲柄]不能出糖果"); }
	}

	public class HasQuarterState : GumballMachineState
	{
		GumballMachine_New gumballMachine;

		public HasQuarterState(GumballMachine_New gumballMachine)
		{
			this.gumballMachine = gumballMachine;
		}

		public void dispense() { Console.WriteLine("[有钱状态,出糖果]不能出糖果"); }

		public void ejectQuarter()
		{
			Console.WriteLine("[有钱状态,退钱]退钱成功");
			gumballMachine.setState(gumballMachine.noQuarterState);
		}

		public void insertQuarter() { Console.WriteLine("[有钱状态,投钱]不能重复投钱"); }

		public void turnCrank()
		{
			Console.WriteLine("[有钱状态,摇动曲柄]正在摇动...");
			gumballMachine.setState(gumballMachine.soldState);
		}
	}

	// 赢家状态
	public class WinnerState : GumballMachineState
	{
		GumballMachine_New gumballMachine;

		public WinnerState(GumballMachine_New gumballMachine)
		{
			this.gumballMachine = gumballMachine;
		}

		public void dispense() { }

		public void ejectQuarter() { }

		public void insertQuarter() { }

		public void turnCrank() { }
	}
}
