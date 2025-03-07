using System;

public class SimpleFactoryMain
{
	public static void Test()
	{
		Calculate.Start();
	}
}

public class Calculate
{
	public static void Start()
	{
		string str1 = Console.ReadLine();
		string operationStr = Console.ReadLine();
		string str2 = Console.ReadLine();

		Operation operation = OperationFactory.GetOperate(operationStr);
		double result = operation.Calculate(double.Parse(str1), double.Parse(str2));
		Console.WriteLine($"{str1} {operationStr} {str2} = {result}");
	}
}

public class OperationFactory
{
	public static Operation GetOperate(string operate)
	{
		switch (operate)
		{
			case "+":
				return new OperationAdd();
		}
		return null;
	}
}

public abstract class Operation
{
	public abstract double Calculate(double e, double b);
}

public class OperationAdd : Operation
{
	public override double Calculate(double a, double b)
	{
		return a + b;
	}
}