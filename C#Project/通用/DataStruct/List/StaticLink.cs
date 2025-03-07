using System;

// 静态链表
class StaticLink<T>
{
	public int length;
	public StaticLinkListNode<T>[] Lst;

	// 初始化
	public StaticLink(int maxSize)
	{
		if (maxSize <= 0)
			throw new System.Exception("初始化静态链表失败");

		maxSize += 2;
		Lst = new StaticLinkListNode<T>[maxSize];

		for (int i = 0; i < Lst.Length; i++)
		{
			Lst[i] = new StaticLinkListNode<T>();
			Lst[i].cur = i + 1;
		}
		Lst[Lst.Length - 1].cur = 0;
		length = 0;
	}

	// 申请一个空结点,当申请的结点位置和最后一个结点位置相同时，说明链表已满
	int malloc()
	{
		int i = Lst[0].cur;
		// 链表已满
		if (i == Lst.Length - 1)
			throw new Exception("静态链表已满,无法申请新结点");
		else
		{
			return i;
		}
	}

	// 归还结点,传入要删除的结点的前驱位置
	void free(int i)
	{
		Lst[i].cur = Lst[0].cur;    // 更新删除结点的下标	
		Lst[0].cur = i;             // 更新备用链表结点
	}

	public void Insert(int i, T data)
	{
		if (i < 1 || i > length + 1 || i > Lst.Length - 2)
			throw new Exception("插入出错");

		int newNodeIndex = malloc();    // 获取插入结点
		if (newNodeIndex > 0)
		{
			Lst[newNodeIndex].data = data; // 在新节点填充数据

			int j = Lst[Lst.Length - 1].cur;    // 获取已用链表的头节点,找到插入位置的上一个结点
			if (j == 0)
			{
				// 静态链表是空的
				Lst[0].cur = Lst[newNodeIndex].cur;
				Lst[newNodeIndex].cur = 0;
				Lst[Lst.Length - 1].cur = newNodeIndex;
			}
			else
			{
				// 静态链表非空
				for (int l = 1; l < i - 1; l++)    // 找到插入结点的上一个结点
					j = Lst[j].cur;

				if (i == 1)
				{
					// 在头部插入新节点
					Lst[0].cur = Lst[newNodeIndex].cur;
					Lst[newNodeIndex].cur = Lst[Lst.Length - 1].cur;
					Lst[Lst.Length - 1].cur = newNodeIndex;
				}
				else if (i == length + 1)
				{
					// 表示在末尾添加新节点
					Lst[0].cur = Lst[newNodeIndex].cur;
					Lst[newNodeIndex].cur = 0;
					Lst[j].cur = newNodeIndex;
				}
				else
				{
					// 中间插入新节点
					Lst[0].cur = Lst[newNodeIndex].cur;    // 更新备用链表首结点	
					Lst[newNodeIndex].cur = Lst[j].cur;    // 更新插入结点的指针域
					Lst[j].cur = newNodeIndex;             // 更新插入结点的上一个结点的指针域
				}
			}

			length++;
		}
	}

	public void Delete(int i)
	{
		if (length == 0 || i < 1 || i > length)
			throw new Exception("删除出错");

		// 找到要删除的结点的上一个结点
		int k = Lst.Length - 1;
		for (int _j = 1; _j < i; _j++)
			k = Lst[k].cur;
		int j = Lst[k].cur;  // 要删除的结点下标

		Lst[j].data = default;
		Lst[k].cur = Lst[j].cur;    // 更新删除结点的上一个结点的下标
		free(j);

		length--;
	}

	public void DebugString()
	{
		for (int i = 0; i < Lst.Length; i++)
		{
			Console.WriteLine($"下标:{i}\t {Lst[i].data}\t:\t{Lst[i].cur}");
		}
		Console.WriteLine();
	}
}

class StaticLinkListNode<T>
{
	public T data;
	public int cur;
}

public class StaticLinkTest
{
	public static void Test()
	{
		StaticLink<int> sl = new StaticLink<int>(2);
		sl.Insert(1, 1);
		sl.Insert(1, 1);
		//sl.insert(3, 1);
		sl.DebugString();
		//sl.insert(1, 4);
		sl.Delete(1);
		//sl.DeleteNode(1);
		//sl.DeleteNode(1);
		sl.DebugString();
	}
}
