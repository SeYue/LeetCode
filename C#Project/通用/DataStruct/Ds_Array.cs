public class Ds_Array<T>
{
	T[] array;
	public int contentLength = 0;

	public Ds_Array(int listLength)
	{
		array = new T[listLength];
	}

	public T GetElement(int index)
	{
		if (index < 0 || index >= contentLength)
		{
			throw new System.Exception("获取元素失败");
		}
		return array[index];
	}

	public void Insert(int index, T data)
	{
		if (index < 0 || index >= array.Length)
		{
			throw new System.Exception("插入失败");
		}

		if (index < contentLength)
		{
			for (int i = contentLength - 1; i >= index; i--)
			{
				array[i + 1] = array[i];
			}
		}
		array[index] = data;
		contentLength++;
	}

	public void Delete(int index)
	{
		if (index < 0 || index >= contentLength)
		{
			throw new System.Exception("删除失败");
		}

		if (index == contentLength - 1)
		{
			array[index] = default;
		}
		else
		{
			for (int i = index; i < contentLength - 1; i++)
			{
				array[i] = array[i + 1];
			}
			array[contentLength - 1] = default;
		}
		contentLength--;
	}
}