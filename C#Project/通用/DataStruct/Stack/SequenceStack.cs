using System;

// 线性栈
public class SequenceStack<T>
{
	public T[] array;
	public int top;

	public SequenceStack(int capacity)
	{
		array = new T[capacity];
		top = 0;
	}

	public void Push(T data)
	{
		if (top == array.Length)
			throw new System.Exception("进栈失败");

		array[top] = data;
		top++;
	}

	public T Pop()
	{
		if (top == 0)
			throw new System.Exception("出栈失败");

		T data = array[top - 1];
		top--;
		return data;
	}

	public void DebugConsole()
	{
		for (int i = 0; i < top; i++)
		{
			Console.WriteLine(array[i]);
		}
		Console.WriteLine();
	}
}

public class LinerStackTest
{
	public static void Test()
	{
		SequenceStack<int> intStack = new SequenceStack<int>(1);
		intStack.Push(1);
		intStack.Pop();
		intStack.Push(2);
		intStack.DebugConsole();
		//intStack.Push(2);


	}
}