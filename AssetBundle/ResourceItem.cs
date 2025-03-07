using System.Collections.Generic;
using UnityEngine;

public class ResourceItem
{
	public uint m_Crc;
	//该资源的文件名
	public string m_AssetName;
	//该资源所在的AssetBundle
	public string m_ABName;
	//该资源所依赖的AssetBundle
	public List<string> m_DependAssetBundle;
	//该资源加载完的AB包
	public AssetBundle m_AssetBundle;
	//-----------------------------------------------------
	//资源对象
	public Object m_Obj;
	//资源唯一标识
	public int m_Guid;
	//资源最后所使用的时间
	public float m_LastUseTime;
	//是否跳场景清掉
	public bool m_Clear = true;
	//引用计数
	int m_RefCount;
	public int RefCount
	{
		get { return m_RefCount; }
		set
		{
			m_RefCount = value;
			if (m_RefCount < 0)
			{
				Debug.LogError("refcount < 0" + m_RefCount + " ," + (m_Obj != null ? m_Obj.name : "name is null"));
			}
		}
	}
}
