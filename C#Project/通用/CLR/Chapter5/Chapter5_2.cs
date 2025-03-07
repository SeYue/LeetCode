using System;

public class Chapter5_2
{
	public static void Test()
	{
		SomeRef r1 = new SomeRef();
		SomeRef r2 = r1;
		r1.intValue = 6;
		Console.WriteLine($"{r1.intValue},{r2.intValue}");

		SomeValue v1 = new SomeValue();
		v1.sr = new SomeRef();
		SomeValue v2 = v1;
		v1.intValue = 1;
		v2.intValue = 2;
		v2.sr.intValue = 1111;
		Console.WriteLine($"{v1.intValue},{v2.intValue},{v1.sr.intValue}");
	}
}

class SomeRef
{
	public int intValue = 5;
}

struct SomeValue
{
	public SomeRef sr;
	public int intValue;
}
