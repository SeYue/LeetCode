using System;

// 线索二叉树
public class BinaryThreadTree
{
	public BinaryThreadTreeNode root;
	public BinaryThreadTreeNode head;   // 指向头节点

	// 通过前序遍历构建二叉树
	int index;
	public void CreateTree(string str)
	{
		CreateTree(str, root, false);
	}

	public void CreateTree(string str, BinaryThreadTreeNode parentNode, bool isLeftChild)
	{
		char ch = str[index++];
		if (ch == '#')
			return;
		else
		{
			BinaryThreadTreeNode newNode = new BinaryThreadTreeNode(ch);
			if (root == null)
			{
				parentNode = root = newNode;
			}
			else
			{
				if (isLeftChild)
					parentNode.lChild = newNode;
				else
					parentNode.rChild = newNode;
			}

			// 左孩子
			CreateTree(str, newNode, true);

			// 右孩子
			CreateTree(str, newNode, false);
		}
	}

	// 上一个结点
	BinaryThreadTreeNode threadingPreNode;

	// 通过中序遍历线索化二叉树
	public void InThreading()
	{
		head = new BinaryThreadTreeNode();
		head.lChild = root;

		InThreading(root);

		// 右指针
		head.rtag = true;
		head.rChild = threadingPreNode;

		threadingPreNode.rtag = true;
		threadingPreNode.rChild = head;
	}

	void InThreading(BinaryThreadTreeNode node)
	{
		if (node == null)
			return;

		InThreading(node.lChild);

		// 如果该节点没有左孩子，则线索化
		if (threadingPreNode != null)
		{
			if (node.lChild == null)
			{
				node.ltag = true;
				node.lChild = threadingPreNode;
			}
			if (threadingPreNode.rChild == null)
			{
				threadingPreNode.rtag = true;
				threadingPreNode.rChild = node;
			}
		}
		else
		{
			// 中序遍历第一个结点
			node.ltag = true;
			node.lChild = head;
		}
		threadingPreNode = node;

		InThreading(node.rChild);
	}

	// 用中序遍历来遍历线索二叉树
	public void InOrderTraverse_Thr()
	{
		BinaryThreadTreeNode p = head.lChild;
		while (p != head)   // 空树或者遍历结束的时候,p==根节点
		{
			while (p.ltag == false) // 当左标记==0的时候到达中序遍历第一个结点
				p = p.lChild;

			Console.WriteLine(p.data);  // 打印当前结点

			while (p.rtag == true && p.rChild != head)
			{
				p = p.rChild;
				Console.WriteLine(p.data);  // 打印后继结点
			}

			p = p.rChild;
		}
	}
}

public class BinaryThreadTreeNode
{
	public char data;
	public BinaryThreadTreeNode lChild;
	public bool ltag;
	public BinaryThreadTreeNode rChild;
	public bool rtag;

	public BinaryThreadTreeNode() { }

	public BinaryThreadTreeNode(char _data)
	{
		data = _data;
	}
}

public class BinaryThreadTreeTest
{
	public static void Test()
	{
		BinaryThreadTree tree = new BinaryThreadTree();
		tree.CreateTree("ABC###D#E##");
		tree.InThreading();
		tree.InOrderTraverse_Thr();
	}
}