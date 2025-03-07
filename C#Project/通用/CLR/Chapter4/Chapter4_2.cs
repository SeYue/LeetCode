using Chapter4_2s;

public class Chapter4_2
{
	public static void Test()
	{
		object o1 = new object();
		object o2 = new B();
		object o3 = new D();
		object o4 = o3;

		B b1 = new B();
		B b2 = new D();
		D d1 = new D();
		//B b3 = new object();
		B b4 = d1;
		D d4 = b4 as D;
		b4 = o1 as D;
	}
}

namespace Chapter4_2s
{
	public class B { }

	public class D : B { }
}