using System;

// 队列的顺序存储结构
public class SequenceQueue<T>
{
	T[] array;
	int rear;

	public SequenceQueue(int arrayLength)
	{
		array = new T[arrayLength];
		rear = 0;
	}

	public void Enqueue(T data)
	{
		if (rear == array.Length)
			throw new Exception("入队失败");

		array[rear] = data;
		rear++;
	}

	public T Dequeue()
	{
		T data = array[0];
		for (int i = 1; i < rear; i++)
		{
			array[i - 1] = array[i];
		}
		rear--;
		return data;
	}

	public void DebugConsole()
	{
		for (int i = 0; i < rear; i++)
		{
			Console.WriteLine(array[i]);
		}
		Console.WriteLine();
	}
}

public class SequenceQueueTest
{
	public static void Test()
	{
		SequenceQueue<int> queue = new SequenceQueue<int>(3);
		queue.Enqueue(1);
		queue.Enqueue(2);
		queue.Enqueue(3);
		queue.DebugConsole();

		queue.Dequeue();
		queue.DebugConsole();
		queue.Enqueue(1);
		queue.DebugConsole();
	}
}
