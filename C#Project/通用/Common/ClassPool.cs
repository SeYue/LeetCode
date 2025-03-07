using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class ClassPoolMgr : Singleton_Class<ClassPoolMgr>
{
	public Dictionary<Type, IClassPool> m_poolDic = new Dictionary<Type, IClassPool>();

	~ClassPoolMgr()
	{
		Debug();
	}

	public void RegisterPool(IClassPool pool)
	{
		if (!m_poolDic.ContainsKey(pool.Type))
		{
			m_poolDic[pool.Type] = pool;
		}
		else
		{
			LogManager.LogFormat("已存在池,不能重复注册:{0}", pool.Type);
		}
	}

	// 工具
	public void Debug()
	{
		StringBuilder sb = ClassPool<StringBuilder>.Instance.Get();
		sb.Append("析构类对象池,未回收对象:");
		foreach (var i in m_poolDic)
		{
			sb.AppendFormat("\n类型:{0}\n池里:{1}\t使用中:{2}\t获取:{3}\t回收:{4}", i.Key, i.Value.PoolCount, i.Value.UsingCount, i.Value.GetTimes, i.Value.RecycleTimes);
		}
		LogManager.Log(sb.ToString());

		sb.Clear();
		ClassPool<StringBuilder>.Instance.Recycle(sb);
	}
}

public interface IClassPool
{
	Type Type { get; }
	int PoolCount { get; }
	int UsingCount { get; }
	ulong GetTimes { get; }
	ulong RecycleTimes { get; }
}

public class ClassPool<T> : Singleton_Class<ClassPool<T>>, IClassPool
	where T : class, new()
{
	public Type Type => typeof(T);
	public int PoolCount => m_poolReadIndex;
	int m_usingCount;
	public int UsingCount => m_usingCount;
	ulong m_getTimes;
	public ulong GetTimes => m_getTimes;
	ulong m_recycleTimes;
	public ulong RecycleTimes => m_recycleTimes;

	int m_poolCapacity;
	int m_poolReadIndex;
	T[] m_poolItemArr;

	public ClassPool()
	{
		ClassPoolMgr.Instance.RegisterPool(this);
		m_poolCapacity = 2;
		m_poolItemArr = new T[m_poolCapacity];
	}

	public T Get()
	{
		T t;
		if (m_poolReadIndex <= 0)
		{
			t = new T();
		}
		else
		{
			m_poolReadIndex--;
			t = m_poolItemArr[m_poolReadIndex];
			m_poolItemArr[m_poolReadIndex] = null;
		}

		m_usingCount++;
		m_getTimes++;
		return t;
	}

	public void Recycle(T t)
	{
		if (t == null)
			return;

		if (m_poolReadIndex >= m_poolItemArr.Length)
		{
			m_poolCapacity *= 2;
			T[] newArr = new T[m_poolCapacity];
			Array.Copy(m_poolItemArr, newArr, m_poolReadIndex);
			m_poolItemArr = newArr;
			// LogManager.LogFormat("对象池扩容,{0},{1}", typeof(T), m_poolCapacity);
		}

		m_poolItemArr[m_poolReadIndex] = t;
		m_poolReadIndex++;

		m_usingCount--;
		m_recycleTimes++;
	}
}

public static class ClassPoolTools
{
	public static void Recycle<T>(this ICollection<T> list)
		where T : class, new()
	{
		if (list == null)
			return;
		if (list is IDictionary)
		{
			LogManager.LogErrorFormat("字典类型请使用RecycleKey(),RecycleValue(),RecycleKeyValue()进行回收");
			return;
		}
		if (typeof(T).IsValueType)
		{
			list.Clear();
			return;
		}

		foreach (var i in list)
		{
			ClassPool<T>.Instance.Recycle(i);
		}
		list.Clear();
	}

	public static void RecycleKey<K, V>(this IDictionary<K, V> dic)
		where K : class, new()
	{
		if (dic == null)
			return;

		foreach (var i in dic)
		{
			ClassPool<K>.Instance.Recycle(i.Key);
		}
		dic.Clear();
	}

	public static void RecycleValue<K, V>(this IDictionary<K, V> dic)
		where V : class, new()
	{
		if (dic == null)
			return;

		foreach (var i in dic)
		{
			ClassPool<V>.Instance.Recycle(i.Value);
		}
		dic.Clear();
	}

	public static void RecycleKeyValue<K, V>(this IDictionary<K, V> dic)
		where K : class, new()
		where V : class, new()
	{
		if (dic == null)
			return;

		foreach (var i in dic)
		{
			ClassPool<K>.Instance.Recycle(i.Key);
			ClassPool<V>.Instance.Recycle(i.Value);
		}
		dic.Clear();
	}

#if UNITY_EDITOR
	[UnityEditor.MenuItem("Tools/类对象池/显示当前使用数据")]
#endif
	public static void DebugLog()
	{
		ClassPoolMgr.Instance.Debug();
	}
}

public interface IClassPoolItemBase_Get
{
	void OnGet();
}

public interface IClassPoolItemBase_Recycle
{
	void OnRecycle();
}

public abstract class ClassPoolItemBase<T> : IDisposable
	where T : class, new()
{
	protected abstract T Self { get; }

	public void Dispose()
	{
		ClassPool<T>.Instance.Recycle(Self);
	}

	// 语法糖
	public static T Get()
	{
		return ClassPool<T>.Instance.Get();
	}

	// 语法糖
	public void Recycle()
	{
		ClassPool<T>.Instance.Recycle(Self);
	}
}

// 特殊池
public class ClassPoolSpecial<T> : Singleton_Class<ClassPoolSpecial<T>>
{
	Stack<T[]> m_pools = new Stack<T[]>();

	public void PushPool(T[] obj)
	{
		m_pools.Push(obj);
	}

	public T[] PopPool(int defaultSize)
	{
		if (m_pools.Count > 0)
			return m_pools.Pop();
		else
			return new T[defaultSize];
	}
}
