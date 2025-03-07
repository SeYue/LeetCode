using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LoadResPriority
{
	RES_HIGHT = 0,//最高优先级
	RES_MIDDLE,//一般优先级
	RES_SLOW,//低优先级
	RES_NUM,
}

public class AsyncLoadResParam
{
	public List<AsyncCallBack> m_CallBackList = new List<AsyncCallBack>();
	public uint m_Crc;
	public string m_Path;
	public bool m_Sprite = false;
	public LoadResPriority m_Priority = LoadResPriority.RES_SLOW;

	public void Reset()
	{
		m_CallBackList.Clear();
		m_Crc = 0;
		m_Path = "";
		m_Sprite = false;
		m_Priority = LoadResPriority.RES_SLOW;
	}
}

public class AsyncCallBack
{
	//加载完成的回调(针对ObjectManager)
	public OnAsyncFinsih m_DealFinish = null;
	//ObjectManager对应的中间
	public ObjectItem m_ResObj = null;
	//---------------------------------------------
	//加载完成的回调
	public OnAsyncObjFinish m_DealObjFinish = null;
	//回调参数
	public object m_Param1 = null, m_Param2 = null, m_Param3 = null;

	public void Reset()
	{
		m_DealObjFinish = null;
		m_DealFinish = null;
		m_Param1 = null;
		m_Param2 = null;
		m_Param3 = null;
		m_ResObj = null;
	}
}

//资源加载完成回调
public delegate void OnAsyncObjFinish(string path, UnityEngine.Object obj, object param1 = null, object param2 = null, object param3 = null);

//实例化对象加载完成回调
public delegate void OnAsyncFinsih(string path, ObjectItem resObj, object param1 = null, object param2 = null, object param3 = null);

public class ResourceManager : UnitySingleton<ResourceManager>
{
	protected long m_Guid = 0;
	//缓存使用的资源列表
	public Dictionary<uint, ResourceItem> AssetDic = new Dictionary<uint, ResourceItem>();
	//缓存引用计数为零的资源列表，达到缓存最大的时候释放这个列表里面最早没用的资源
	protected CMapList<ResourceItem> m_NoRefrenceAssetMapList;

	//中间类，回调类的类对象池
	protected ClassObjectPool<AsyncLoadResParam> m_AsyncLoadResParamPool = new ClassObjectPool<AsyncLoadResParam>(50);
	protected ClassObjectPool<AsyncCallBack> m_AsyncCallBackPool = new ClassObjectPool<AsyncCallBack>(100);

	//正在异步加载的资源列表
	protected List<AsyncLoadResParam>[] m_LoadingAssetList = new List<AsyncLoadResParam>[(int)LoadResPriority.RES_NUM];
	//正在异步加载的Dic
	protected Dictionary<uint, AsyncLoadResParam> m_LoadingAssetDic = new Dictionary<uint, AsyncLoadResParam>();

	//最长连续卡着加载资源的时间，单位微妙
	private const long MAXLOADRESTIME = 200000;

	//最大缓存个数
	private const int MAXCACHECOUNT = 500;

	public void Init()
	{
		m_NoRefrenceAssetMapList = new CMapList<ResourceItem>();

		for (int i = 0; i < (int)LoadResPriority.RES_NUM; i++)
		{
			m_LoadingAssetList[i] = new List<AsyncLoadResParam>();
		}
		StartCoroutine(AsyncLoadCor());
	}

	/// <summary>
	/// 创建唯一的GUID
	/// </summary>
	/// <returns></returns>
	public long CreatGuid()
	{
		return m_Guid++;
	}

	/// <summary>
	/// 清空缓存
	/// </summary>
	public void ClearCache()
	{
		List<ResourceItem> tempList = new List<ResourceItem>();
		foreach (ResourceItem item in AssetDic.Values)
		{
			if (item.m_Clear)
			{
				tempList.Add(item);
			}
		}

		foreach (ResourceItem item in tempList)
		{
			DestoryResouceItme(item, true);
		}
		tempList.Clear();
	}

	/// <summary>
	/// 取消异步加载资源
	/// </summary>
	/// <returns></returns>
	public bool CancleLoad(ObjectItem res)
	{
		AsyncLoadResParam para = null;
		if (m_LoadingAssetDic.TryGetValue(res.m_Crc, out para) && m_LoadingAssetList[(int)para.m_Priority].Contains(para))
		{
			for (int i = para.m_CallBackList.Count; i >= 0; i--)
			{
				AsyncCallBack tempCallBack = para.m_CallBackList[i];
				if (tempCallBack != null && res == tempCallBack.m_ResObj)
				{
					tempCallBack.Reset();
					m_AsyncCallBackPool.Recycle(tempCallBack);
					para.m_CallBackList.Remove(tempCallBack);
				}
			}

			if (para.m_CallBackList.Count <= 0)
			{
				para.Reset();
				m_LoadingAssetList[(int)para.m_Priority].Remove(para);
				m_AsyncLoadResParamPool.Recycle(para);
				m_LoadingAssetDic.Remove(res.m_Crc);
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// 根据ResObj增加引用计数
	/// </summary>
	/// <returns></returns>
	public int IncreaseResouceRef(ObjectItem resObj, int count = 1)
	{
		return resObj != null ? IncreaseResouceRef(resObj.m_Crc, count) : 0;
	}

	/// <summary>
	/// 根据path增加引用计数
	/// </summary>
	/// <param name="crc"></param>
	/// <param name="count"></param>
	/// <returns></returns>
	public int IncreaseResouceRef(uint crc = 0, int count = 1)
	{
		ResourceItem item = null;
		if (!AssetDic.TryGetValue(crc, out item) || item == null)
		{
			Debugger.LogError("[wei]增加引用计数失败,crc:{0} 找不到对应ResourceItem", crc);
			return 0;
		}

		item.RefCount += count;
		item.m_LastUseTime = Time.realtimeSinceStartup;
		return item.RefCount;
	}

	/// <summary>
	/// 根据ResouceObj减少引用计数
	/// </summary>
	/// <param name="resObj"></param>
	/// <param name="count"></param>
	/// <returns></returns>
	public int DecreaseResoucerRef(ObjectItem resObj, int count = 1)
	{
		return resObj != null ? DecreaseResoucerRef(resObj.m_Crc, count) : 0;
	}

	/// <summary>
	/// 根据路径减少引用计数
	/// </summary>
	/// <param name="crc"></param>
	/// <param name="count"></param>
	/// <returns></returns>
	int DecreaseResoucerRef(uint crc, int count = 1)
	{
		ResourceItem item = null;
		if (!AssetDic.TryGetValue(crc, out item) || item == null)
			return 0;

		item.RefCount -= count;

		return item.RefCount;
	}

	/// <summary>
	/// 预加载资源
	/// </summary>
	/// <param name="path"></param>
	public void PreloadRes(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return;
		}
		uint crc = Crc32.GetCrc32(path);
		ResourceItem item = GetCacheResouceItem(crc, 0);
		if (item != null)
		{
			return;
		}

		UnityEngine.Object obj = null;
		if (obj == null)
		{
			item = AssetBundleManager.Instance.LoadResouceAssetBundle(crc);
			if (item != null && item.m_AssetBundle != null)
			{
				if (item.m_Obj != null)
				{
					obj = item.m_Obj;
				}
				else
				{
					obj = item.m_AssetBundle.LoadAsset<UnityEngine.Object>(item.m_AssetName);
				}
			}
		}

		CacheResource(path, ref item, crc, obj);
		//跳场景不清空缓存
		item.m_Clear = false;
		ReleaseResouce(obj, false);
	}

	/// <summary>
	/// 同步加载资源，针对给ObjectManager的接口
	/// </summary>
	/// <param name="path"></param>
	/// <param name="resObj"></param>
	/// <returns></returns>
	public ObjectItem LoadResource(string path, ObjectItem resObj)
	{
		if (resObj == null)
		{
			return null;
		}

		uint crc = resObj.m_Crc;
		ResourceItem item = GetCacheResouceItem(crc);
		if (item != null)
		{
			resObj.m_ResItem = item;
			return resObj;
		}

		UnityEngine.Object obj = null;
		item = AssetBundleManager.Instance.LoadResouceAssetBundle(crc);
		if (item == null)
		{
			return null;
		}

		if (item != null && item.m_AssetBundle != null)
		{
			if (item.m_Obj != null)
			{
				obj = item.m_Obj;
			}
			else
			{
				obj = item.m_AssetBundle.LoadAsset<UnityEngine.Object>(item.m_AssetName);
			}
		}

		CacheResource(path, ref item, crc, obj);
		resObj.m_ResItem = item;
		item.m_Clear = resObj.m_bClear;

		return resObj;
	}

	/// <summary>
	/// 同步资源加载，外部直接调用，仅加载不需要实例化的资源，例如Texture,音频等等
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="path"></param>
	/// <returns></returns>
	public T LoadResource<T>(string path) where T : UnityEngine.Object
	{
		if (string.IsNullOrEmpty(path))
		{
			return null;
		}
		uint crc = Crc32.GetCrc32(path);
		ResourceItem item = GetCacheResouceItem(crc);
		if (item != null)
		{
			return item.m_Obj as T;
		}

		T obj = null;
		/*
		#if UNITY_EDITOR
				if (!m_LoadFormAssetBundle)
				{
					item = AssetBundleManager.Instance.FindResourceItme(crc);
					if (item != null && item.m_AssetBundle != null)
					{
						if (item.m_Obj != null)
						{
							obj = (T)item.m_Obj;
						}
						else
						{
							obj = item.m_AssetBundle.LoadAsset<T>(item.m_AssetName);
						}
					}
					else
					{
						if (item == null)
						{
							item = new ResouceItem();
							item.m_Crc = crc;
						}
						obj = LoadAssetByEditor<T>(path);
					}
				}
		#endif
		*/
		if (obj == null)
		{
			item = AssetBundleManager.Instance.LoadResouceAssetBundle(crc);
			if (item != null && item.m_AssetBundle != null)
			{
				if (item.m_Obj != null)
				{
					obj = item.m_Obj as T;
				}
				else
				{
					obj = item.m_AssetBundle.LoadAsset<T>(item.m_AssetName);
				}
			}
		}

		CacheResource(path, ref item, crc, obj);
		return obj;
	}

	/// <summary>
	/// 根据ResouceObj卸载资源
	/// </summary>
	/// <param name="resObj"></param>
	/// <param name="destoryObj"></param>
	/// <returns></returns>
	public bool ReleaseResouce(ObjectItem resObj, bool destoryObj = false)
	{
		if (resObj == null)
			return false;

		ResourceItem item = null;
		if (!AssetDic.TryGetValue(resObj.m_Crc, out item) || null == item)
		{
			Debugger.LogError("AssetDic里不存在改资源：" + resObj.m_CloneObj.name + "  可能释放了多次");
		}

		Destroy(resObj.m_CloneObj);

		item.RefCount--;

		DestoryResouceItme(item, destoryObj);
		return true;
	}

	/// <summary>
	/// 不需要实例化的资源的卸载，根据对象
	/// </summary>
	/// <param name="obj"></param>
	/// <param name="destoryObj"></param>
	/// <returns></returns>
	public bool ReleaseResouce(UnityEngine.Object obj, bool destoryObj = false)
	{
		if (obj == null)
		{
			return false;
		}

		ResourceItem item = null;
		foreach (ResourceItem res in AssetDic.Values)
		{
			if (res.m_Guid == obj.GetInstanceID())
			{
				item = res;
			}
		}

		if (item == null)
		{
			Debug.LogError("AssetDic里不存在改资源：" + obj.name + "  可能释放了多次");
			return false;
		}

		item.RefCount--;

		DestoryResouceItme(item, destoryObj);
		return true;
	}

	/// <summary>
	/// 不需要实例化的资源卸载，根据路径
	/// </summary>
	/// <param name="path"></param>
	/// <param name="destoryObj"></param>
	/// <returns></returns>
	public bool ReleaseResouce(string path, bool destoryObj = false)
	{
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}

		uint crc = Crc32.GetCrc32(path);
		ResourceItem item = null;
		if (!AssetDic.TryGetValue(crc, out item) || null == item)
		{
			Debug.LogError("AssetDic里不存该资源：" + path + "  可能释放了多次");
		}

		item.RefCount--;

		DestoryResouceItme(item, destoryObj);
		return true;
	}

	/// <summary>
	/// 缓存加载的资源
	/// </summary>
	/// <param name="path"></param>
	/// <param name="item"></param>
	/// <param name="crc"></param>
	/// <param name="obj"></param>
	/// <param name="addrefcount"></param>
	void CacheResource(string path, ref ResourceItem item, uint crc, UnityEngine.Object obj, int addrefcount = 1)
	{
		//缓存太多，清除最早没有使用的资源
		WashOut();
		if (item == null)
		{
			Debug.LogError("ResouceItem is null, path: " + path);
			return;
		}
		if (obj == null)
		{
			Debug.LogError("ResouceLoad Fail :  " + path);
			return;
		}
		item.m_Obj = obj;
		item.m_Guid = obj.GetInstanceID();
		item.m_LastUseTime = Time.realtimeSinceStartup;
		item.RefCount += addrefcount;
		ResourceItem oldItme = null;
		if (AssetDic.TryGetValue(item.m_Crc, out oldItme))
		{
			AssetDic[item.m_Crc] = item;
		}
		else
		{
			AssetDic.Add(item.m_Crc, item);
		}
	}

	/// <summary>
	/// 缓存太多，清除最早没有使用的资源
	/// </summary>
	void WashOut()
	{
		//当大于缓存个数时，进行一半释放
		while (m_NoRefrenceAssetMapList.Size() >= MAXCACHECOUNT)
		{
			for (int i = 0; i < MAXCACHECOUNT / 2; i++)
			{
				ResourceItem item = m_NoRefrenceAssetMapList.Back();
				DestoryResouceItme(item, true);
			}
		}
	}

	/// <summary>
	/// 回收一个资源
	/// </summary>
	/// <param name="item"></param>
	/// <param name="destroy"></param>
	protected void DestoryResouceItme(ResourceItem item, bool destroyCache = false)
	{
		if (item == null || item.RefCount > 0)
		{
			return;
		}

		if (!destroyCache)
		{
			m_NoRefrenceAssetMapList.InsertToHead(item);
			return;
		}

		if (!AssetDic.Remove(item.m_Crc))
		{
			return;
		}

		m_NoRefrenceAssetMapList.Remove(item);

		//释放assetbundle引用
		AssetBundleManager.Instance.ReleaseAssetBundle(item);
		//清空资源对应的对象池
		ObjectManager.Instance.ClearPoolObject(item.m_Crc);

		if (item.m_Obj != null)
		{
			item.m_Obj = null;
		}
		Debugger.Log("[wei]释放资源:{0},{1}", item.m_AssetName, item.m_ABName);
	}

	/// <summary>
	/// 从资源池获取缓存资源
	/// </summary>
	/// <param name="crc"></param>
	/// <param name="addrefcount"></param>
	/// <returns></returns>
	ResourceItem GetCacheResouceItem(uint crc, int addrefcount = 1)
	{
		ResourceItem item = null;
		if (AssetDic.TryGetValue(crc, out item) &&
			item != null)
		{
			item.RefCount += addrefcount;
			item.m_LastUseTime = Time.realtimeSinceStartup;
		}

		return item;
	}

	/// <summary>
	/// 异步加载资源（仅仅是不需要实例化的资源，例如音频，图片等等）
	/// </summary>
	public void AsyncLoadResource(string path, OnAsyncObjFinish dealFinish, LoadResPriority priority, bool isSprite = false, object param1 = null, object param2 = null, object param3 = null, uint crc = 0)
	{
		if (crc == 0)
		{
			crc = Crc32.GetCrc32(path);
		}

		ResourceItem item = GetCacheResouceItem(crc);
		if (item != null)
		{
			if (dealFinish != null)
			{
				dealFinish(path, item.m_Obj, param1, param2, param3);
			}
			return;
		}

		//判断是否在加载中
		AsyncLoadResParam para = null;
		if (!m_LoadingAssetDic.TryGetValue(crc, out para) || para == null)
		{
			para = m_AsyncLoadResParamPool.Spawn();
			para.m_Crc = crc;
			para.m_Path = path;
			para.m_Sprite = isSprite;
			para.m_Priority = priority;
			m_LoadingAssetDic.Add(crc, para);
			m_LoadingAssetList[(int)priority].Add(para);
		}

		//往回调列表里面加回调
		AsyncCallBack callBack = m_AsyncCallBackPool.Spawn();
		callBack.m_DealObjFinish = dealFinish;
		callBack.m_Param1 = param1;
		callBack.m_Param2 = param2;
		callBack.m_Param3 = param3;
		para.m_CallBackList.Add(callBack);
	}

	/// <summary>
	/// 针对ObjectManager的异步加载接口
	/// </summary>
	/// <param name="path"></param>
	/// <param name="resObj"></param>
	/// <param name="dealfinish"></param>
	/// <param name="priority"></param>
	public void AsyncLoadResource(string path, ObjectItem resObj, OnAsyncFinsih dealfinish, LoadResPriority priority)
	{
		ResourceItem item = GetCacheResouceItem(resObj.m_Crc);
		if (item != null)
		{
			resObj.m_ResItem = item;
			if (dealfinish != null)
			{
				dealfinish(path, resObj);
			}
			return;
		}

		//判断是否在加载中
		AsyncLoadResParam para = null;
		if (!m_LoadingAssetDic.TryGetValue(resObj.m_Crc, out para) || para == null)
		{
			para = m_AsyncLoadResParamPool.Spawn();
			para.m_Crc = resObj.m_Crc;
			para.m_Path = path;
			para.m_Priority = priority;
			m_LoadingAssetDic.Add(resObj.m_Crc, para);
			m_LoadingAssetList[(int)priority].Add(para);
		}

		//往回调列表里面加回调
		AsyncCallBack callBack = m_AsyncCallBackPool.Spawn();
		callBack.m_DealFinish = dealfinish;
		callBack.m_ResObj = resObj;
		para.m_CallBackList.Add(callBack);
	}

	/// <summary>
	/// 异步加载
	/// </summary>
	/// <returns></returns>
	IEnumerator AsyncLoadCor()
	{
		List<AsyncCallBack> callBackList = null;
		//上一次yield的时间
		long lastYiledTime = System.DateTime.Now.Ticks;
		while (true)
		{
			bool haveYield = false;
			for (int i = 0; i < (int)LoadResPriority.RES_NUM; i++)
			{
				if (m_LoadingAssetList[(int)LoadResPriority.RES_HIGHT].Count > 0)
				{
					i = (int)LoadResPriority.RES_HIGHT;
				}
				else if (m_LoadingAssetList[(int)LoadResPriority.RES_MIDDLE].Count > 0)
				{
					i = (int)LoadResPriority.RES_MIDDLE;
				}

				List<AsyncLoadResParam> loadingList = m_LoadingAssetList[i];
				if (loadingList.Count <= 0)
					continue;

				AsyncLoadResParam loadingItem = loadingList[0];
				loadingList.RemoveAt(0);
				callBackList = loadingItem.m_CallBackList;

				UnityEngine.Object obj = null;
				ResourceItem item = null;
				if (obj == null)
				{
					item = AssetBundleManager.Instance.LoadResouceAssetBundle(loadingItem.m_Crc);
					if (item != null && item.m_AssetBundle != null)
					{
						AssetBundleRequest abRequest = null;
						if (loadingItem.m_Sprite)
						{
							abRequest = item.m_AssetBundle.LoadAssetAsync<Sprite>(item.m_AssetName);
						}
						else
						{
							abRequest = item.m_AssetBundle.LoadAssetAsync(item.m_AssetName);
						}
						yield return abRequest;
						if (abRequest.isDone)
						{
							obj = abRequest.asset;
						}
						lastYiledTime = System.DateTime.Now.Ticks;
					}
				}

				CacheResource(loadingItem.m_Path, ref item, loadingItem.m_Crc, obj, callBackList.Count);

				for (int j = 0; j < callBackList.Count; j++)
				{
					AsyncCallBack callBack = callBackList[j];

					if (callBack != null && callBack.m_DealFinish != null && callBack.m_ResObj != null)
					{
						ObjectItem tempResObj = callBack.m_ResObj;
						tempResObj.m_ResItem = item;
						callBack.m_DealFinish(loadingItem.m_Path, tempResObj, tempResObj.m_Param1, tempResObj.m_Param2, tempResObj.m_Param3);
						callBack.m_DealFinish = null;
						tempResObj = null;
					}

					if (callBack != null && callBack.m_DealObjFinish != null)
					{
						callBack.m_DealObjFinish(loadingItem.m_Path, obj, callBack.m_Param1, callBack.m_Param2, callBack.m_Param3);
						callBack.m_DealObjFinish = null;
					}

					callBack.Reset();
					m_AsyncCallBackPool.Recycle(callBack);
				}

				obj = null;
				callBackList.Clear();
				m_LoadingAssetDic.Remove(loadingItem.m_Crc);

				loadingItem.Reset();
				m_AsyncLoadResParamPool.Recycle(loadingItem);

				if (System.DateTime.Now.Ticks - lastYiledTime > MAXLOADRESTIME)
				{
					yield return null;
					lastYiledTime = System.DateTime.Now.Ticks;
					haveYield = true;
				}
			}

			if (!haveYield || System.DateTime.Now.Ticks - lastYiledTime > MAXLOADRESTIME)
			{
				lastYiledTime = System.DateTime.Now.Ticks;
				yield return null;
			}

		}
	}
}