using System;

// 二叉树
public class BinaryTree<T>
{
	public BinaryTreeNode<T> root;

	// 根据前序遍历构建二叉树,#号表示空子树
	int index = 0;
	public void CreateTree(string str, BinaryTreeNode<T> _root, bool isLeftChild)
	{
		if (index >= str.Length)
			return;
		char ch = str[index++];
		if (ch == '#')
		{
			return;
		}
		else
		{
			BinaryTreeNode<T> newNode = new BinaryTreeNode<T>(ch.ToString());
			if (_root == null)
			{
				_root = root = newNode;
			}
			else
			{
				if (isLeftChild)
					_root.leftChild = newNode;
				else
					_root.rightChild = newNode;
			}
			CreateTree(str, newNode, true);
			CreateTree(str, newNode, false);
		}
	}

	// 前序遍历
	public void PreOrderTraverse()
	{
		Console.WriteLine("前序遍历");
		PreOrderTraverse(root);
	}

	public void PreOrderTraverse(BinaryTreeNode<T> node)
	{
		if (node == null)
			return;

		Console.WriteLine(node.data);
		PreOrderTraverse(node.leftChild);
		PreOrderTraverse(node.rightChild);
	}

	// 中序遍历
	public void InOrderTraverse()
	{
		Console.WriteLine("中序遍历");
		InOrderTraverse(root);
	}

	public void InOrderTraverse(BinaryTreeNode<T> node)
	{
		if (node == null)
			return;

		InOrderTraverse(node.leftChild);
		Console.WriteLine(node.data);
		InOrderTraverse(node.rightChild);
	}

	// 后序遍历
	public void PostOrderTraverse()
	{
		Console.WriteLine("后续遍历:");
		PostOrderTraverse(root);
	}

	public void PostOrderTraverse(BinaryTreeNode<T> node)
	{
		if (node == null)
			return;

		PostOrderTraverse(node.leftChild);
		PostOrderTraverse(node.rightChild);
		Console.WriteLine(node.data);
	}
}

public class BinaryTreeNode<T>
{
	public string data;

	public BinaryTreeNode<T> leftChild;
	public BinaryTreeNode<T> rightChild;

	public BinaryTreeNode() { }

	public BinaryTreeNode(T _data)
	{
		data = _data.ToString();
	}

	public BinaryTreeNode(string _data)
	{
		data = _data;
	}
}

public class BinaryTreeTest
{
	public static void Test()
	{
		BinaryTree<int> tree = new BinaryTree<int>();
		tree.CreateTree("124###3##", null, false);

		tree.PreOrderTraverse();
		tree.InOrderTraverse();
		tree.PostOrderTraverse();
	}
}
