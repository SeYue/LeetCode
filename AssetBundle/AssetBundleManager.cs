using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class AssetBundleManager : UnitySingleton<AssetBundleManager>
{
	public static string m_ABConfigABName = "assetbundleconfig_bytes";
	public static string ABLoadPath { get { return Application.streamingAssetsPath + "/AssetBundle/"; } }

	AssetBundleConfig m_config;
	//资源关系依赖配表，可以根据crc来找到对应资源块
	Dictionary<uint, ResourceItem> m_AssetBundleConfigDic = new Dictionary<uint, ResourceItem>();
	//储存已加载的AB包，key为crc
	Dictionary<uint, AssetBundleItem> m_AssetBundleItemDic = new Dictionary<uint, AssetBundleItem>();
	ClassObjectPool<AssetBundleItem> m_AssetBundleItemPool;

	public bool LoadAssetBundleConfig()
	{
		string configPath = ABLoadPath + m_ABConfigABName;
		AssetBundle configAB = AssetBundle.LoadFromFile(configPath);
		if (configAB == null)
		{
			Debugger.LogError("[wei]初始化AssetBundle失败,配置文件不存在:{0}", configPath);
			return false;
		}
		TextAsset textAsset = configAB.LoadAsset<TextAsset>(m_ABConfigABName);
		if (textAsset == null)
		{
			Debugger.LogError("[wei]初始化AssetBundle失败,配置文件无法转成TextAsset:{0}", m_ABConfigABName);
			return false;
		}

		m_config = null;
		using (MemoryStream stream = new MemoryStream(textAsset.bytes))
		{
			BinaryFormatter bf = new BinaryFormatter();
			m_config = (AssetBundleConfig)bf.Deserialize(stream);
		}
		if (m_config == null)
		{
			Debugger.LogError("[wei]反序列化AssetBundleConfig失败:{0}", m_ABConfigABName);
			return false;
		}
		m_AssetBundleItemPool = ObjectManager.Instance.GetOrCreatClassPool<AssetBundleItem>(m_config.ABList.Count);
		m_AssetBundleConfigDic.Clear();
		foreach (ABBase i in m_config.ABList)
		{
			CreateResourceItem(i);
		}
		Debugger.Log("[wei]初始化AssetBundle成功,配置表中ab包数量:{0},游戏中ab包数量:{1}", m_config.ABList.Count, m_AssetBundleConfigDic.Count);
		return true;
	}

	void CreateResourceItem(ABBase abBase)
	{
		if (m_AssetBundleConfigDic.ContainsKey(abBase.Crc))
		{
			return;
		}

		ResourceItem item = new ResourceItem
		{
			m_Crc = abBase.Crc,
			m_AssetName = abBase.AssetName,
			m_ABName = abBase.ABName,
			m_DependAssetBundle = abBase.ABDependce
		};
		m_AssetBundleConfigDic.Add(item.m_Crc, item);

		foreach (var i in item.m_DependAssetBundle)
		{
			foreach (var j in m_config.ABList)
			{
				if (i.Equals(j.ABName))
				{
					CreateResourceItem(j);
					break;
				}
			}
		}
	}

	/// <summary>
	/// 根据路径的crc加载中间类ResoucItem
	/// </summary>
	/// <param name="crc"></param>
	/// <returns></returns>
	public ResourceItem LoadResouceAssetBundle(uint crc)
	{
		ResourceItem item = null;
		if (!m_AssetBundleConfigDic.TryGetValue(crc, out item) || item == null)
		{
			Debugger.LogError(string.Format("[wei]加载ab包失败,无法根据crc获取ab包信息,crc:{0} ", crc));
			return null;
		}
		if (item.m_AssetBundle == null)
		{
			CreateResourceItemAssetBundle(item);
		}
		return item;
	}

	void CreateResourceItemAssetBundle(ResourceItem item)
	{
		item.m_AssetBundle = LoadAssetBundle(item.m_ABName);
		if (item.m_DependAssetBundle != null &&
			item.m_DependAssetBundle.Count > 0)
		{
			foreach (var i in item.m_DependAssetBundle)
			{
				foreach (var j in m_AssetBundleConfigDic)
				{
					if (j.Value.m_ABName.Equals(i))
					{
						CreateResourceItemAssetBundle(j.Value);
						break;
					}
				}
			}
		}
	}

	/// <summary>
	/// 根据名字加载单个assetbundle
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	AssetBundle LoadAssetBundle(string name)
	{
		AssetBundleItem item = null;
		uint crc = Crc32.GetCrc32(name);
		if (!m_AssetBundleItemDic.TryGetValue(crc, out item))
		{
			string fullPath = ABLoadPath + name.ToLower();
			AssetBundle assetBundle = AssetBundle.LoadFromFile(fullPath);
			if (assetBundle == null)
			{
				Debugger.LogError("[wei]加载AssetBundle失败:{0}", fullPath);
			}
			item = m_AssetBundleItemPool.Spawn();
			item.assetBundle = assetBundle;
			item.RefCount++;
			m_AssetBundleItemDic.Add(crc, item);
		}
		else
		{
			item.RefCount++;
		}
		return item.assetBundle;
	}

	/// <summary>
	/// 释放资源
	/// </summary>
	/// <param name="item"></param>
	public void ReleaseAssetBundle(ResourceItem item)
	{
		if (item == null)
		{
			return;
		}

		if (item.m_DependAssetBundle != null && item.m_DependAssetBundle.Count > 0)
		{
			for (int i = 0; i < item.m_DependAssetBundle.Count; i++)
			{
				UnloadAssetBundle(item.m_DependAssetBundle[i]);
			}
		}
		UnloadAssetBundle(item.m_ABName);
	}

	void UnloadAssetBundle(string name)
	{
		AssetBundleItem item = null;
		uint crc = Crc32.GetCrc32(name);
		if (m_AssetBundleItemDic.TryGetValue(crc, out item) &&
			item != null)
		{
			item.RefCount--;
			if (item.RefCount <= 0 && item.assetBundle != null)
			{
				item.assetBundle.Unload(true);
				item.Reset();
				m_AssetBundleItemPool.Recycle(item);
				m_AssetBundleItemDic.Remove(crc);
				Debugger.Log("[wei]回收AssetBundle:{0}", name);
			}
		}
	}

	/// <summary>
	/// 根据crc朝赵ResouceItem
	/// </summary>
	/// <param name="crc"></param>
	/// <returns></returns>
	public ResourceItem FindResourceItme(uint crc)
	{
		ResourceItem item = null;
		m_AssetBundleConfigDic.TryGetValue(crc, out item);
		return item;
	}
}