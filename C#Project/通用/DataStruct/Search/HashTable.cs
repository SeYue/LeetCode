public class HashTable
{
	int?[] element;    // 数据元素存储基址，动态分配数组
	int count;      // 当前元素个数

	public void InitHashTable(int m)
	{
		count = m;
		element = new int?[count];
		for (int i = 0; i < count; i++)
		{
			element[i] = null;
		}
	}

	int Hash(int key)
	{
		return key % count;
	}

	public void InsertHash(int key)
	{
		int address = Hash(key);
		while (element[address] != null)
		{
			address = (address + 1) % count;
		}
		element[address] = key;
	}

	public bool SearchHash(int key)
	{
		int address = Hash(key);
		while (element[address] != key)
		{
			address = (address + 1) % count;
			if (element[address] == null || address == Hash(key))
				return false;
		}
		return true;
	}
}

public class HashTableTest
{
	public static void Test()
	{

	}
}