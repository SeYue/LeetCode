using System;

// 单向循环链表
public class SingleCycleLink<T>
{
	public SingleCycleLinkNode<T> head;
	public int length;

	public SingleCycleLink()
	{
		head = new SingleCycleLinkNode<T>();
		head.next = head;
	}

	public void Insert(int pos, T data)
	{
		if (pos > length)
			throw new Exception("插入失败");

		SingleCycleLinkNode<T> newNode = new SingleCycleLinkNode<T>()
		{
			data = data
		};

		if (length == 0)
		{
			head.next = newNode;
			newNode.next = head;
		}
		else
		{
			// 找到尾结点
			SingleCycleLinkNode<T> preNode = head;
			for (int i = 0; i < pos; i++)
				preNode = preNode.next;
			SingleCycleLinkNode<T> nextNode = preNode.next;

			preNode.next = newNode;
			newNode.next = nextNode;
		}
		length++;
	}

	public void Delete(int pos)
	{
		if (pos < 0 || pos > length - 1 || length == 0)
			throw new Exception("删除失败");

		SingleCycleLinkNode<T> preNode = head;
		for (int i = 0; i <= pos - 1; i++)
			preNode = preNode.next;

		SingleCycleLinkNode<T> deleteNode = preNode.next;
		SingleCycleLinkNode<T> nextNode = deleteNode.next;

		preNode.next = nextNode;

		// TODO 回收删除的结点

	}

	public void DebugString()
	{
		if (length == 0)
			return;
		SingleCycleLinkNode<T> nowNode = head.next;
		while (nowNode != head)
		{
			Console.WriteLine(nowNode.data);

			nowNode = nowNode.next;
		}
		Console.WriteLine();
	}

}

public class SingleCycleLinkNode<T>
{
	public T data;
	public SingleCycleLinkNode<T> next;
}

public class SingleCycleLinkTest
{
	public static void Test()
	{
		SingleCycleLink<int> link = new SingleCycleLink<int>();
		link.Insert(0, 0);
		link.Insert(1, 1);
		link.Insert(2, 2);

		link.DebugString();

		link.Delete(0);
		link.DebugString();
	}
}
