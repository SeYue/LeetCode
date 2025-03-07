using System;

public class CycleQueue<T>
{
	T[] array;
	int front;
	int rear;

	public CycleQueue(int maxSize)
	{
		array = new T[maxSize];
	}

	public int Count()
	{
		// (0 + rear) + (array.Length - front)
		return rear - front + array.Length;
	}

	public bool IsEmpty()
	{
		return rear == front;
	}

	public bool IsFull()
	{
		return (rear + 1) % array.Length == front;
	}

	public void Enqueue(T data)
	{
		if (IsFull())
			throw new Exception("入队失败");

		array[rear] = data;
		rear++;
		if (rear >= array.Length)
			rear = 0;
	}

	public T Dequeue()
	{
		if (IsEmpty())
			throw new Exception("出队失败");

		T data = array[front];
		array[front] = default;
		front++;
		if (front >= array.Length)
		{
			front = 0;
		}
		return data;
	}

	public void DebugConsole()
	{
		if (IsEmpty())
			Console.WriteLine("队列为空");
		else
		{
			for (int i = 0; i < array.Length; i++)
			{
				Console.WriteLine($"{i}\t{array[i]}");
			}

			/*
			if (front > rear)
			{
				for (int i = front; i < array.Length; i++)
				{
					Console.WriteLine(array[i]);
				}
				for (int i = 0; i < rear; i++)
				{
					Console.WriteLine(array[i]);
				}
			}
			else
			{
				for (int i = front; i < rear; i++)
				{
					Console.WriteLine(array[i]);
				}
			}
			*/
		}
		Console.WriteLine();
	}
}

public class CycleQueueTest
{
	public static void Test()
	{
		CycleQueue<int> queue = new CycleQueue<int>(5);
		queue.Enqueue(1);
		queue.Enqueue(1);
		queue.Enqueue(1);
		queue.Enqueue(1);
		queue.DebugConsole();

		queue.Dequeue();
		queue.Dequeue();
		queue.DebugConsole();
		queue.Enqueue(2);
		queue.Enqueue(2);
		queue.DebugConsole();

		Console.WriteLine(queue.Count());
	}
}
