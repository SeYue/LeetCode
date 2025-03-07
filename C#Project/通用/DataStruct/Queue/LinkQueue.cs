using System;

public class LinkQueue<T>
{
	public LinkQueueNode<T> head;
	public LinkQueueNode<T> rear;

	public LinkQueue() { }

	public void Enqueue(T data)
	{
		LinkQueueNode<T> newNode = new LinkQueueNode<T>()
		{
			data = data
		};

		if (head == null && rear == null)
			head = rear = newNode;
		else
		{
			rear.next = newNode;
			rear = newNode;
		}
	}

	public T Dequeue()
	{
		T data = head.data;
		head = head.next;
		return data;
	}

	public void DebugConsole()
	{
		LinkQueueNode<T> nowNode = head;
		while (nowNode != null)
		{
			Console.WriteLine(nowNode.data);
			nowNode = nowNode.next;
		}
		Console.WriteLine();
	}
}

public class LinkQueueNode<T>
{
	public T data;
	public LinkQueueNode<T> next;
}

public class LinkQueueTest
{
	public static void Test()
	{
		LinkQueue<int> intQueue = new LinkQueue<int>();
		intQueue.Enqueue(1);
		intQueue.Enqueue(2);
		intQueue.Enqueue(3);
		intQueue.Enqueue(4);
		intQueue.Enqueue(5);
		intQueue.Enqueue(6);
		intQueue.DebugConsole();
		intQueue.Dequeue();
		intQueue.Dequeue();
		intQueue.Dequeue();
		intQueue.DebugConsole();
		intQueue.Enqueue(7);
		intQueue.Enqueue(8);
		intQueue.DebugConsole();
	}
}