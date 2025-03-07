public class BinaryTree<T>
	where T : class
{
	public BinaryTreeNode<T> m_rootNode;

	public BinaryTree(T rootData)
	{
		m_rootNode = new BinaryTreeNode<T>(rootData);
	}

	public BinaryTreeNode<T> GetNodeWithData(T nodeData)
	{
		return m_rootNode.EqualsData(nodeData);
	}
}

public class BinaryTreeNode<T>
	where T : class
{
	public T m_data;
	public BinaryTreeNode<T> m_leftChild;
	public BinaryTreeNode<T> m_rightChild;

	public BinaryTreeNode(T data)
	{
		m_data = data;
	}

	public BinaryTreeNode<T> EqualsData(T data)
	{
		if (m_data == null || data == null)
		{
			return null;
		}
		if (m_data == data)
		{
			return this;
		}

		if (m_leftChild != null)
		{
			BinaryTreeNode<T> node = m_leftChild.EqualsData(data);
			if (node != null)
			{
				return node;
			}
		}

		if (m_rightChild != null)
		{
			BinaryTreeNode<T> node = m_rightChild.EqualsData(data);
			if (node != null)
			{
				return node;
			}
		}
		return null;
	}

	public BinaryTreeNode<T> AddLeftChild(T data)
	{
		if (data == null)
		{
			return null;
		}
		m_leftChild = new BinaryTreeNode<T>(data);
		return m_leftChild;
	}

	public BinaryTreeNode<T> AddRightChild(T data)
	{
		if (data == null)
		{
			return null;
		}
		m_rightChild = new BinaryTreeNode<T>(data);
		return m_rightChild;
	}
}
