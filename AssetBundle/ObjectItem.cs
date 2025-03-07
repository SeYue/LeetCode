using UnityEngine;

public class ObjectItem
{
	//路径对应CRC
	public uint m_Crc = 0;
	//存ResouceItem
	public ResourceItem m_ResItem = null;
	//实例化出来的GameObject
	public GameObject m_CloneObj = null;
	//是否跳场景清除
	public bool m_bClear = true;
	//储存GUID
	public long m_Guid = 0;
	//是否已经放回对象池
	public bool m_Already = false;
	//--------------------------------
	//实例化资源加载完成回调
	public OnAsyncObjFinish m_DealFinish = null;
	//异步参数
	public object m_Param1, m_Param2, m_Param3 = null;

	public void Reset()
	{
		m_Crc = 0;
		m_CloneObj = null;
		m_bClear = true;
		m_Guid = 0;
		m_ResItem = null;
		m_Already = false;
		m_DealFinish = null;
		m_Param1 = m_Param2 = m_Param3 = null;
	}
}