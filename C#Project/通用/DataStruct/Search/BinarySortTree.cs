using System;
using System.Collections.Generic;

// 二叉排序树
public class BinarySortTree
{
	public const int LH = 1;    // 左高
	public const int EH = 0;    // 等高
	public const int RH = -1;   // 右高

	public BinarySortNode root;

	int index = 0;
	public void CreateTree(string str, BinarySortNode parent = null, bool isLeft = true)
	{
		char c = str[index++];
		while (c == ' ')
			c = str[index++];

		if (c == '#')
			return;

		BinarySortNode newNode = new BinarySortNode(int.Parse(c.ToString()));
		if (root == null)
			root = newNode;
		else if (isLeft)
			parent.lchild = newNode;
		else
			parent.rchild = newNode;

		CreateTree(str, newNode, true);
		CreateTree(str, newNode, false);
	}

	// 在二叉排序树中查询指定key
	public bool SearchBST(int key, out BinarySortNode p)
	{
		return SearchBST(root, key, null, out p);
	}

	bool SearchBST(BinarySortNode node, int key, BinarySortNode f, out BinarySortNode p)
	{
		if (node == null)
		{
			p = f;
			return false;
		}
		else if (key == node.data)
		{
			p = node;
			return true;
		}
		else if (key < node.data)
		{
			return SearchBST(node.lchild, key, node, out p);
		}
		else
		{
			return SearchBST(node.rchild, key, node, out p);
		}
	}

	// 插入关键字
	public bool InsertBST(int key)
	{
		if (root == null)
		{
			root = new BinarySortNode(key);
			return true;
		}

		if (!SearchBST(key, out BinarySortNode p))
		{
			BinarySortNode newNode = new BinarySortNode(key);
			if (key < p.data)
			{
				p.lchild = newNode;
				newNode.parent = p;
			}
			else if (key > p.data)
			{
				p.rchild = newNode;
				newNode.parent = p;
			}
			return true;
		}
		else
		{
			return false;
		}
	}

	// 删除
	public bool DeleteBST(BinarySortNode node, int key)
	{
		if (node == null)
			return false;
		else
		{
			if (key == node.data)
				return Delete(node);
			else if (key < node.data)
				return DeleteBST(node.lchild, key);
			else
				return DeleteBST(node.rchild, key);
		}
	}

	// 删除二叉排序树的结点，并尝试连接它的左子树和右子树
	public bool Delete(BinarySortNode node)
	{
		if (node.rchild == null)
		{
			if (node.IsLeftChild())
				node.parent.lchild = node.lchild;
			else
				node.parent.rchild = node.lchild;
		}
		else if (node.lchild == null)
		{
			if (node.IsLeftChild())
				node.parent.lchild = node.rchild;
			else
				node.parent.rchild = node.rchild;
		}
		else
		{
			BinarySortNode q = node;
			BinarySortNode s = node.lchild;
			while (s.rchild != null)    // 循环找到最右侧的结点
			{
				q = s;
				s = s.rchild;
			}

			node.data = s.data;
			// s到这里的时候，其实他已经没有右孩子了，如果有有孩子的话，上面的while循环就不会结束，所以这里没有右孩子，只有左孩子。
			if (q != node)
				// 因为q的右孩子是s，当s被放到node的位置时，s就要被删除；然后s没有右孩子，所以用s的左孩子代替s的位置
				//就有了这句代码
				q.rchild = s.lchild;
			else
				// 如果q和node相等，就表示while循环一次都没有执行，s没有右孩子，只有左孩子。这里直接用s的左孩子代替s就好了。
				q.lchild = s.lchild;
		}
		return true;
	}

	// 二叉平衡树

	// LL型,右旋
	public void R_Rotate(BinarySortNode root)
	{
		BinarySortNode rootParent = root.parent;
		BinarySortNode lchild = root.lchild;

		root.parent = lchild;
		root.lchild = lchild.rchild;

		lchild.parent = rootParent;
		lchild.rchild = root;

		if (rootParent != null)
		{
			if (rootParent.lchild == root)
				rootParent.lchild = lchild;
			else if (rootParent.rchild == root)
				rootParent.rchild = lchild;
		}
		else
		{
			this.root = lchild;
		}
	}

	// RR型,左旋
	public void L_Rotate(BinarySortNode root)
	{
		BinarySortNode parent = root.parent;
		BinarySortNode rchild = root.rchild;

		root.parent = rchild;
		root.rchild = rchild.lchild;

		rchild.lchild = root;
		rchild.parent = parent;

		if (parent != null)
		{
			if (parent.lchild == root)
				parent.lchild = rchild;
			else if (parent.rchild == root)
				parent.rchild = rchild;
		}
		else
		{
			this.root = rchild;
		}
	}

	// 左平衡
	public void LeftBalance(BinarySortNode root)
	{
		if (root.lchild.GetBF() > 0)
		{
			R_Rotate(root);
		}
		else
		{
			L_Rotate(root.lchild);
			R_Rotate(root);
		}
	}

	// 右平衡
	public void RightBalance(BinarySortNode root)
	{
		if (root.rchild.GetBF() < 0)
		{
			L_Rotate(root);
		}
		else
		{
			R_Rotate(root.rchild);
			L_Rotate(root);
		}
	}

	public bool InsertAVL(int key)
	{
		return InsertAVL(root, key);
	}

	public bool InsertAVL(BinarySortNode node, int key)
	{
		if (node == null)
		{
			root = new BinarySortNode(key);
		}
		else
		{
			// 如果树中存在相同关键字的结点，则不再插入新节点
			if (node.data == key)
				return false;

			// 判断当前关键字比最近的结点小还是大，小则插入倒左子树，大则插入到右子树
			if (key < node.data)
			{
				if (node.lchild == null)
				{
					node.lchild = new BinarySortNode(key, node);
				}
				else if (!InsertAVL(node.lchild, key))
				{
					return false;
				}

				if (node.GetBF() > 1)
				{
					LeftBalance(node);
				}
			}
			else
			{
				if (node.rchild == null)
				{
					node.rchild = new BinarySortNode(key, node);
				}
				else if (!InsertAVL(node.rchild, key))
				{
					return false;
				}

				if (node.GetBF() < -1)
				{
					RightBalance(node);
				}
			}
		}
		return true;
	}

	public void ConsoleTree()
	{
		Console.WriteLine("二叉排序树");
		Queue<BinarySortNode> queue = new Queue<BinarySortNode>();

		queue.Enqueue(root);
		bool isLevel1Count = true;
		int level1Count = 1;
		int level2Count = 0;

		while (queue.Count > 0)
		{
			BinarySortNode node = queue.Dequeue();
			Console.Write(node.data + "\t");

			int leftCount = isLevel1Count ? --level1Count : --level2Count;

			if (node.lchild != null)
			{
				if (isLevel1Count)
					level2Count++;
				else
					level1Count++;
				queue.Enqueue(node.lchild);
			}
			if (node.rchild != null)
			{
				if (isLevel1Count)
					level2Count++;
				else
					level1Count++;
				queue.Enqueue(node.rchild);
			}

			if (leftCount == 0)
			{
				isLevel1Count = !isLevel1Count;
				Console.WriteLine();
			}
		}
	}
}

// 二叉链表结点
public class BinarySortNode
{
	public int data;
	public BinarySortNode parent, lchild, rchild;

	public BinarySortNode() { }

	public BinarySortNode(int data)
	{
		this.data = data;
	}

	public BinarySortNode(int data, BinarySortNode parent) : this(data)
	{
		this.parent = parent;
	}

	// 是父节点的左孩子还是右孩子？
	public bool IsLeftChild()
	{
		if (parent == null)
			return false;
		else
		{
			if (parent.lchild == this)
				return true;
			else
				return false;
		}
	}

	// 当前结点高度
	public int Height()
	{
		int lHeight = 0, rHeight = 0;
		if (lchild != null)
			lHeight = lchild.Height();
		if (rchild != null)
			rHeight = rchild.Height();
		return (lHeight > rHeight ? lHeight : rHeight) + 1;
	}

	// 二叉平衡树的平衡因子
	public int GetBF()
	{
		int lHeight = 0, rHeight = 0;
		if (lchild != null)
			lHeight = lchild.Height();
		if (rchild != null)
			rHeight = rchild.Height();
		return lHeight - rHeight;
	}
}

public class BinarySortTreeTest
{
	public static void Test()
	{
		BinarySortTree t = new BinarySortTree();
		//t.CreateTree("65##8#9##");

		// 创建二叉排序树
		//int[] keys = new int[] { 62, 88, 58, 47, 35, 73, 51, 99, 37, 93 };
		//foreach (var i in keys)
		//{
		//	t.InsertBST(i);
		//}

		//t.SearchBST(93, out BinarySortNode p);
		//Console.WriteLine(p.data);

		// 旋转测试
		int[] keys = new int[] { 3, 2, 1, 4, 5, 6, 7, 10, 9, 8 };
		foreach (var i in keys)
			t.InsertAVL(i);
		t.ConsoleTree();
		Console.WriteLine(t.root.Height());
	}
}