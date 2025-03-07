// 孩子表示法
public class ChildTree<T>
{
	ChildTreeArray<T> array;
}

public class ChildTreeArray<T>
{
	public T data;
	public ChildTreeList head;
}

public class ChildTreeList
{
	public int index;
	public ChildTreeList next;
}