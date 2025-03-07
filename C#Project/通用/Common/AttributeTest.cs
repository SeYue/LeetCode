using System;
using System.Reflection;

public class AttributeTest
{
	public static void Test()
	{
		NumClass c = new NumClass()
		{
			num = 10
		};

		NumAdd1Attribute a = c.GetType().GetField("num").GetCustomAttribute<NumAdd1Attribute>();
		Console.WriteLine($"{a.addNum}");

		Console.WriteLine(c.num + a.addNum);
	}

	~AttributeTest()
	{

	}
}


class NumClass
{
	[NumAdd1(1)]
	public int num;
}

[AttributeUsage(AttributeTargets.Field)]
class NumAdd1Attribute : Attribute
{
	public int addNum;

	public NumAdd1Attribute(int addNum)
	{
		this.addNum = addNum;
	}


}