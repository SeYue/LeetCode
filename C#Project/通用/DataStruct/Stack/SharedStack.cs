using System;

// 共享栈
public class SharedStack<T>
{
	T[] array;
	int top1;
	int top2;

	public SharedStack(int capacity)
	{
		array = new T[capacity];
		top1 = -1;
		top2 = capacity;
	}

	public void Push1(T data)
	{
		if (top2 - top1 == 1)
			throw new Exception("栈满");

		top1++;
		array[top1] = data;
	}

	public void Push2(T data)
	{
		if (top2 - top1 == 1)
			throw new Exception("栈满");

		top2--;
		array[top2] = data;
	}

	public T Pop1()
	{
		if (top1 == -1)
			throw new Exception("栈空");

		T data = array[top1];
		top1--;
		return data;
	}

	public T Pop2()
	{
		if (top2 == array.Length)
			throw new Exception("栈空");

		T data = array[top2];
		top2++;
		return data;
	}

	public void DebugConsole()
	{
		Console.WriteLine("stack1:");
		for (int i = 0; i <= top1; i++)
		{
			Console.WriteLine(array[i]);
		}
		Console.WriteLine("\nstack2:");
		for (int i = array.Length - 1; i >= top2; i--)
		{
			Console.WriteLine(array[i]);
		}
	}
}

public class SharedStackTest
{
	public static void Test()
	{
		SharedStack<int> s = new SharedStack<int>(2);
		s.Push1(1);
		s.Push2(3);
		s.Pop1();
		s.Push2(4);
		s.DebugConsole();
		s.Push2(3);
	}
}
