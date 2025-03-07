using System;

// 策略模式
public class StrategyModelTest
{
	public static void Test()
	{
		CalculateOperate operate = null;

		string operateStr = "-";
		switch (operateStr)
		{
			case "+":
				operate = new CalculateAdd();
				break;
			case "-":
				operate = new CalculateSub();
				break;
		}
		double res = operate.Calculate(100, 10);
		Console.WriteLine(res);
	}
}

// 算法家族
public abstract class CalculateOperate
{
	public abstract double Calculate(double num1, double num2);
}

public class CalculateAdd : CalculateOperate
{
	public override double Calculate(double num1, double num2)
	{
		return num1 + num2;
	}
}

public class CalculateSub : CalculateOperate
{
	public override double Calculate(double num1, double num2)
	{
		return num1 - num2;
	}
}