using System;

public class Chapter4_1
{
	public static void Test()
	{
		object o = new Employee();  // 隐式转换,该类可以隐式转换为基类型
		Employee e = (Employee)o;   // 显示转换,基类实例转换为子类型实例时，需要标注类型
		e = o as Employee;

		//Employee e = new Object();

		PromoteEmployee(new Manager());
		PromoteEmployee(new DateTime(1999, 1, 1));
	}

	public static void PromoteEmployee(object o)
	{
		Employee e = (Employee)o;
	}
}

public class Employee : System.Object
{
	public override bool Equals(object? obj)
	{
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override string ToString()
	{
		return base.ToString();
	}
}

public class Manager : Employee
{

}
