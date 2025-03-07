// 双亲表示法
public class ParentTree<T>
{
	ParentTreeNode<T>[] array;
	int r;  // 根的位置
	int n;  // 结点数量
}

public class ParentTreeNode<T>
{
	public T data;
	public int parent;
}
