using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

// 单链表
public class LinkList<T> : IEnumerator<T>, IEnumerable<T>
{
	public Ds_LinkListNode<T> m_head;
	public int m_count;

	public LinkList() { }

	public T this[int index]
	{
		get
		{
			Ds_LinkListNode<T> current = GetNode(index);
			if (current != null)
				return current.m_data;
			return default(T);
		}
	}

	// 获取节点
	public Ds_LinkListNode<T> GetNode(int index)
	{
		if (m_head == null || index < 0 || index >= m_count)
			return null;

		int currentIndex = 0;
		Ds_LinkListNode<T> node = m_head;
		while (node != null && currentIndex < index)
		{
			node = m_head.m_next;
			currentIndex++;
		}
		return node;
	}

	// 获取长度
	public uint GetLength()
	{
		uint i = 0;
		Ds_LinkListNode<T> current = m_head;

		while (current != null)
		{
			i++;
			current = current.m_next;
		}
		return i;
	}

	// 单链表是否为空
	public bool IsEmpty()
	{
		return m_head == null;
	}

	// 加到末尾
	public void Append(T data)
	{
		Ds_LinkListNode<T> foot = ClassPool<Ds_LinkListNode<T>>.Instance.Get().Init(data);
		if (m_head == null)
		{
			m_head = foot;
			m_count++;
			return;
		}

		Ds_LinkListNode<T> current = m_head;
		while (current.m_next != null)
		{
			current = current.m_next;
		}
		current.m_next = foot;
		foot.m_previous = current;
		m_count++;
	}

	// 在指定位置插入
	public void Insert(uint index, T node)
	{
		if (IsEmpty())
			return;
		if (index < 0 || index >= m_count)
			throw new Exception("超出索引");

		Ds_LinkListNode<T> newNode = ClassPool<Ds_LinkListNode<T>>.Instance.Get().Init(node);

		// 头节点位置插入
		Ds_LinkListNode<T> nextNode = null;
		if (index == 0)
		{
			nextNode = m_head.m_next;
			newNode.m_previous = m_head;
			newNode.m_next = nextNode;
			m_head.m_next = newNode;
			nextNode.m_previous = newNode;
			m_count++;
			return;
		}
		else
		{
			int currentIndex = 0;
			Ds_LinkListNode<T> currentNode = m_head;
			while (currentIndex < index)
			{
				currentNode = currentNode.m_next;
				nextNode = currentNode.m_next;
				currentIndex++;
			}

			newNode.m_previous = currentNode;
			newNode.m_next = nextNode;

			currentNode.m_next = newNode;
			if (nextNode != null)
				nextNode.m_previous = newNode;
			m_count++;
		}
	}

	// 删除指定位置元素
	public void Delete(uint index)
	{
		if (IsEmpty())
			return;

		if (index == 0)
		{
			Ds_LinkListNode<T> tmp = m_head;
			m_head = m_head.m_next;
			ClassPool<Ds_LinkListNode<T>>.Instance.Recycle(tmp);
			m_count--;
			return;
		}

		Ds_LinkListNode<T> current = m_head;
		Ds_LinkListNode<T> next = current.m_next;
		uint i = 1;
		while (next != null && i < index)
		{
			current = next;
			next = next.m_next;
			i++;
		}
		if (i == index)
		{
			if (next != null)
				current.m_next = next.m_next;
			else
				current.m_next = null;
			ClassPool<Ds_LinkListNode<T>>.Instance.Recycle(next);
			m_count--;
		}
	}

	public override string ToString()
	{
		StringBuilder sb = new StringBuilder("linkList content:");
		Ds_LinkListNode<T> node = m_head;
		while (node != null)
		{
			sb.AppendFormat("\nvalue:{0}", node.m_data);
			node = node.m_next;
		}
		return sb.ToString();
	}

	// IEnumerator
	bool isStart;
	Ds_LinkListNode<T> m_enumeratorData;

	public T Current => m_enumeratorData.m_data;
	object IEnumerator.Current => Current;

	public bool MoveNext()
	{
		if (isStart)
		{
			isStart = false;
			m_enumeratorData = m_head;
		}
		else
			m_enumeratorData = m_enumeratorData.m_next;
		return m_enumeratorData != null;
	}

	public void Reset()
	{
		isStart = true;
		m_enumeratorData = null;
	}

	public void Dispose()
	{
		m_enumeratorData = null;
	}

	// IEnumerable
	public IEnumerator<T> GetEnumerator()
	{
		Reset();
		return this;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}

public class Ds_LinkListNode<T>
{
	public T m_data;                    // 数据域
	public Ds_LinkListNode<T> m_previous;  // 前驱
	public Ds_LinkListNode<T> m_next;       // 后继

	public Ds_LinkListNode() { }

	public Ds_LinkListNode(T data)
	{
		Init(data);
	}

	public Ds_LinkListNode<T> Init(T data)
	{
		m_data = data;
		return this;
	}
}
