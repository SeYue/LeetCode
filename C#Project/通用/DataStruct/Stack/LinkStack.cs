// 栈的链式存储结构,链栈	
using System;

public class LinkStack<T>
{
	LinkStackNode<T> head;
	int length;

	public LinkStack() { }

	public void Push(T data)
	{
		LinkStackNode<T> newNode = new LinkStackNode<T>()
		{
			data = data,
		};

		newNode.next = head;
		head = newNode;
		length++;
	}

	public T Pop()
	{
		if (head == null)
			return default;

		T data = head.data;
		head = head.next;
		length--;
		return data;
	}

	public void DebugConsole()
	{
		LinkStackNode<T> node = head;
		while (node != null)
		{
			Console.WriteLine(node.data);
			node = node.next;
		}
	}
}

public class LinkStackNode<T>
{
	public T data;
	public LinkStackNode<T> next;
}

public class LinkStackTest
{
	public static void Test()
	{
		LinkStack<int> s = new LinkStack<int>();
		s.Push(1);
		s.Push(2);
		s.Push(3);
		s.DebugConsole();

		s.Pop();
		s.DebugConsole();
	}
}