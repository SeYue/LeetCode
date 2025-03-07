using System.Collections.Generic;

public class CMapList<T>
	where T : class, new()
{
	DoubleLinkedList<T> m_DLink = new DoubleLinkedList<T>();
	Dictionary<T, DoubleLinkedListNode<T>> m_FindMap = new Dictionary<T, DoubleLinkedListNode<T>>();

	~CMapList()
	{
		Clear();
	}

	/// <summary>
	/// 清空列表
	/// </summary>
	public void Clear()
	{
		while (m_DLink.Tail != null)
		{
			Remove(m_DLink.Tail.t);
		}
	}

	/// <summary>
	/// 插入一个节点到表头
	/// </summary>
	/// <param name="t"></param>
	public void InsertToHead(T t)
	{
		DoubleLinkedListNode<T> node = null;
		if (m_FindMap.TryGetValue(t, out node) && node != null)
		{
			m_DLink.AddToHeader(node);
			return;
		}
		m_DLink.AddToHeader(t);
		m_FindMap.Add(t, m_DLink.Head);
	}

	/// <summary>
	/// 从表尾弹出一个结点
	/// </summary>
	public void Pop()
	{
		if (m_DLink.Tail != null)
		{
			Remove(m_DLink.Tail.t);
		}
	}

	/// <summary>
	/// 删除某个节点
	/// </summary>
	/// <param name="t"></param>
	public void Remove(T t)
	{
		DoubleLinkedListNode<T> node = null;
		if (!m_FindMap.TryGetValue(t, out node) || node == null)
		{
			return;
		}
		m_DLink.RemoveNode(node);
		m_FindMap.Remove(t);
	}

	/// <summary>
	/// 获取到尾部节点
	/// </summary>
	/// <returns></returns>
	public T Back()
	{
		return m_DLink.Tail == null ? null : m_DLink.Tail.t;
	}

	/// <summary>
	/// 返回节点个数
	/// </summary>
	/// <returns></returns>
	public int Size()
	{
		return m_FindMap.Count;
	}

	/// <summary>
	/// 查找是否存在该节点
	/// </summary>
	/// <param name="t"></param>
	/// <returns></returns>
	public bool Find(T t)
	{
		DoubleLinkedListNode<T> node = null;
		if (!m_FindMap.TryGetValue(t, out node) || node == null)
			return false;

		return true;
	}

	/// <summary>
	/// 刷新某个节点，把节点移动到头部
	/// </summary>
	/// <param name="t"></param>
	/// <returns></returns>
	public bool Reflesh(T t)
	{
		DoubleLinkedListNode<T> node = null;
		if (!m_FindMap.TryGetValue(t, out node) || node == null)
			return false;

		m_DLink.MoveToHead(node);
		return true;
	}
}
