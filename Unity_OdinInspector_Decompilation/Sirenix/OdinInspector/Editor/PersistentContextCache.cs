using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Persistent Context cache object.
	/// </summary>
	[InitializeOnLoad]
	public class PersistentContextCache
	{
		private static class CachePurger
		{
			private static readonly List<KeyValuePair<int, GlobalPersistentContext>> buffer = new List<KeyValuePair<int, GlobalPersistentContext>>();

			private static double lastUpdate;

			private static IEnumerator purger;

			public static void Run()
			{
				if (purger != null)
				{
					double timeSinceStartup = EditorApplication.get_timeSinceStartup();
					do
					{
						if (!purger.MoveNext())
						{
							EndPurge();
							break;
						}
					}
					while (EditorApplication.get_timeSinceStartup() - timeSinceStartup < 0.004999999888241291);
				}
				else if (EditorApplication.get_timeSinceStartup() - lastUpdate > 1.0)
				{
					lastUpdate = EditorApplication.get_timeSinceStartup();
					if (Instance.CacheSize > Instance.MaxCacheByteSize)
					{
						int count = (Instance.CacheSize - Instance.MaxCacheByteSize) / (Instance.CacheSize / Instance.EntryCount) + 1;
						purger = Purge(count);
					}
				}
			}

			private static void EndPurge()
			{
				if (purger != null)
				{
					purger = null;
					if (buffer != null)
					{
						buffer.Clear();
					}
					lastUpdate = EditorApplication.get_timeSinceStartup();
				}
			}

			private static IEnumerator Purge(int count)
			{
				EditorApplication.get_timeSinceStartup();
				long newest = DateTime.Now.Ticks;
				for (int i = 0; i < Instance.EntryCount; i++)
				{
					KeyValuePair<ContextKey, GlobalPersistentContext> keyValuePair = Instance.cache.Get(i);
					bool flag = false;
					if (keyValuePair.Value == null)
					{
						buffer.Insert(0, new KeyValuePair<int, GlobalPersistentContext>(i, keyValuePair.Value));
					}
					else if (keyValuePair.Value.TimeStamp < newest)
					{
						for (int j = 0; j < buffer.Count; j++)
						{
							if (buffer[j].Value != null && buffer[j].Value.TimeStamp >= keyValuePair.Value.TimeStamp)
							{
								if (buffer.Count >= count)
								{
									buffer[buffer.Count - 1] = new KeyValuePair<int, GlobalPersistentContext>(i, keyValuePair.Value);
								}
								else
								{
									buffer.Insert(j, new KeyValuePair<int, GlobalPersistentContext>(i, keyValuePair.Value));
								}
								break;
							}
						}
					}
					if (!flag && buffer.Count < count)
					{
						buffer.Add(new KeyValuePair<int, GlobalPersistentContext>(i, keyValuePair.Value));
						flag = true;
					}
					if (flag)
					{
						GlobalPersistentContext value = buffer[buffer.Count - 1].Value;
						if (value != null)
						{
							newest = value.TimeStamp;
						}
					}
					yield return null;
				}
				foreach (KeyValuePair<int, GlobalPersistentContext> item in buffer.OrderByDescending((KeyValuePair<int, GlobalPersistentContext> e) => e.Key))
				{
					Instance.cache.RemoveAt(item.Key);
					yield return null;
				}
			}
		}

		private static readonly object instance_LOCK;

		private static PersistentContextCache instance;

		private const int MAX_CACHE_SIZE_UPPER_LIMIT = 1000000;

		private static readonly string tempCacheFilename;

		private const int defaultApproximateSizePerEntry = 50;

		private static bool configsLoaded;

		private static bool internalEnableCaching;

		private static int internalMaxCacheByteSize;

		private static bool internalWriteToFile;

		[NonSerialized]
		private bool isInitialized;

		[NonSerialized]
		private DateTime lastSave = DateTime.MinValue;

		private int approximateSizePerEntry;

		[NonSerialized]
		private IndexedDictionary cache = new IndexedDictionary();

		public static PersistentContextCache Instance
		{
			get
			{
				if (instance == null)
				{
					lock (instance_LOCK)
					{
						if (instance == null)
						{
							instance = new PersistentContextCache();
						}
					}
				}
				return instance;
			}
		}

		/// <summary>
		/// Estimated cache size in bytes.
		/// </summary>
		public int CacheSize => ((approximateSizePerEntry > 0) ? approximateSizePerEntry : 50) * EntryCount;

		/// <summary>
		/// The current number of context entries in the cache.
		/// </summary>
		public int EntryCount => cache.Count;

		/// <summary>
		/// If <c>true</c> then persistent context is disabled entirely.
		/// </summary>
		[ShowInInspector]
		public bool EnableCaching
		{
			get
			{
				LoadConfigs();
				return internalEnableCaching;
			}
			set
			{
				internalEnableCaching = value;
				EditorPrefs.SetBool("PersistentContextCache.EnableCaching", value);
			}
		}

		/// <summary>
		/// If <c>true</c> the context will be saved to a file in the temp directory.
		/// </summary>
		[ShowInInspector]
		[EnableIf("EnableCaching")]
		public bool WriteToFile
		{
			get
			{
				LoadConfigs();
				return internalWriteToFile;
			}
			set
			{
				internalWriteToFile = value;
				EditorPrefs.SetBool("PersistentContextCache.WriteToFile", value);
			}
		}

		/// <summary>
		/// The max size of the cache in bytes.
		/// </summary>
		[ShowInInspector]
		[EnableIf("EnableCaching")]
		[CustomValueDrawer("DrawCacheSize")]
		[SuffixLabel("KB", false, Overlay = true)]
		public int MaxCacheByteSize
		{
			get
			{
				LoadConfigs();
				return internalMaxCacheByteSize;
			}
			private set
			{
				internalMaxCacheByteSize = value;
				EditorPrefs.SetInt("PersistentContextCache.MaxCacheByteSize", value);
			}
		}

		[ShowInInspector]
		[FilePath]
		[ReadOnly]
		private string CacheFileLocation
		{
			get
			{
				return Path.Combine(SirenixAssetPaths.OdinTempPath, tempCacheFilename).Replace("\\", "/");
			}
			set
			{
			}
		}

		[ShowInInspector]
		[ProgressBar(0.0, 100.0, 0.15f, 0.47f, 0.74f)]
		[SuffixLabel("$CurrentCacheSizeSuffix", false, Overlay = true)]
		[ReadOnly]
		private int CurrentCacheSize
		{
			get
			{
				LoadConfigs();
				return (int)((float)CacheSize / (float)MaxCacheByteSize * 100f);
			}
		}

		private string CurrentCacheSizeSuffix => StringUtilities.NicifyByteSize(CacheSize) + " / " + StringUtilities.NicifyByteSize(MaxCacheByteSize);

		private PersistentContextCache()
		{
		}

		static PersistentContextCache()
		{
			instance_LOCK = new object();
			tempCacheFilename = "PersistentContextCache_v3.cache";
			configsLoaded = false;
			UnityEditorEventUtility.DelayAction(delegate
			{
				Instance.EnsureIsInitialized();
			});
		}

		private void EnsureIsInitialized()
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Expected O, but got Unknown
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Expected O, but got Unknown
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected O, but got Unknown
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Expected O, but got Unknown
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Expected O, but got Unknown
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Expected O, but got Unknown
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Expected O, but got Unknown
			if (!isInitialized)
			{
				isInitialized = true;
				EditorApplication.update = (CallbackFunction)Delegate.Remove((Delegate)(object)EditorApplication.update, (Delegate)new CallbackFunction(UpdateCallback));
				EditorApplication.update = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.update, (Delegate)new CallbackFunction(UpdateCallback));
				AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;
				AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;
				EditorApplication.playmodeStateChanged = (CallbackFunction)Delegate.Remove((Delegate)(object)EditorApplication.playmodeStateChanged, (Delegate)new CallbackFunction(OnPlaymodeChanged));
				EditorApplication.playmodeStateChanged = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.playmodeStateChanged, (Delegate)new CallbackFunction(OnPlaymodeChanged));
				LoadCache();
			}
		}

		private void OnPlaymodeChanged()
		{
			DateTime now = DateTime.Now;
			if (now - lastSave > TimeSpan.FromSeconds(1.0))
			{
				lastSave = now;
				SaveCache();
			}
		}

		private static string FormatSize(int size)
		{
			if (size <= 1000000)
			{
				if (size <= 1000)
				{
					return size + " bytes";
				}
				return size / 1000 + " kB";
			}
			return size / 1000000 + " MB";
		}

		private static void LoadConfigs()
		{
			if (!configsLoaded)
			{
				internalEnableCaching = EditorPrefs.GetBool("PersistentContextCache.EnableCaching", true);
				internalMaxCacheByteSize = EditorPrefs.GetInt("PersistentContextCache.MaxCacheByteSize", 1000000);
				internalWriteToFile = EditorPrefs.GetBool("PersistentContextCache.WriteToFile", true);
				configsLoaded = true;
			}
		}

		private static void UpdateCallback()
		{
			CachePurger.Run();
		}

		internal GlobalPersistentContext<TValue> GetContext<TValue>(int key1, int key2, int key3, int key4, int key5, out bool isNew)
		{
			ContextKey key6 = new ContextKey(key1, key2, key3, key4, key5);
			return TryGetContext<TValue>(key6, out isNew);
		}

		private int DrawCacheSize(int value, GUIContent label)
		{
			value /= 1000;
			value = SirenixEditorFields.DelayedIntField("Max Cache Size", value);
			value = Mathf.Clamp(value, 1, 1000000);
			return value * 1000;
		}

		private void OnDomainUnload(object sender, EventArgs e)
		{
			SaveCache();
		}

		[Button(ButtonSizes.Medium)]
		[ButtonGroup("_DefaultGroup", 0f)]
		[EnableIf("EnableCaching")]
		private void LoadCache()
		{
			string fileName = Path.Combine(SirenixAssetPaths.OdinTempPath, tempCacheFilename).Replace("\\", "/");
			FileInfo fileInfo = new FileInfo(fileName);
			try
			{
				approximateSizePerEntry = 50;
				if (fileInfo.Exists)
				{
					using (FileStream stream = fileInfo.OpenRead())
					{
						DeserializationContext deserializationContext = new DeserializationContext();
						deserializationContext.Config.DebugContext.LoggingPolicy = LoggingPolicy.Silent;
						deserializationContext.Config.DebugContext.ErrorHandlingPolicy = ErrorHandlingPolicy.Resilient;
						cache = SerializationUtility.DeserializeValue<IndexedDictionary>(stream, DataFormat.Binary, new List<Object>(), deserializationContext);
						if (cache == null)
						{
							cache = new IndexedDictionary();
						}
					}
					if (EntryCount > 0)
					{
						approximateSizePerEntry = (int)(fileInfo.Length / EntryCount);
					}
				}
				else
				{
					cache.Clear();
				}
			}
			catch (Exception ex)
			{
				cache = new IndexedDictionary();
				Debug.LogError((object)"Exception happened when loading Persistent Context from file.");
				Debug.LogException(ex);
			}
		}

		[Button(ButtonSizes.Medium)]
		[ButtonGroup("_DefaultGroup", 0f)]
		[EnableIf("EnableCaching")]
		private void SaveCache()
		{
			if (!WriteToFile || !EnableCaching)
			{
				return;
			}
			try
			{
				approximateSizePerEntry = 50;
				string fileName = Path.Combine(SirenixAssetPaths.OdinTempPath, tempCacheFilename).Replace("\\", "/");
				FileInfo fileInfo = new FileInfo(fileName);
				if (cache.Count == 0)
				{
					if (fileInfo.Exists)
					{
						DeleteCache();
					}
					return;
				}
				if (!Directory.Exists(SirenixAssetPaths.OdinTempPath))
				{
					Directory.CreateDirectory(SirenixAssetPaths.OdinTempPath);
				}
				using (FileStream stream = fileInfo.OpenWrite())
				{
					SerializationUtility.SerializeValue(cache, stream, DataFormat.Binary, out var unityObjects);
					if (unityObjects != null && unityObjects.Count > 0)
					{
						Debug.LogError((object)"Cannot reference UnityEngine Objects with PersistentContext.");
					}
				}
				approximateSizePerEntry = (int)(fileInfo.Length / EntryCount);
			}
			catch (Exception ex)
			{
				Debug.LogError((object)"Exception happened when saving Persistent Context to file.");
				Debug.LogException(ex);
			}
		}

		/// <summary>
		/// Delete the persistent cache file.
		/// </summary>
		[Button(ButtonSizes.Medium)]
		[ButtonGroup("_DefaultGroup", 0f)]
		[EnableIf("EnableCaching")]
		public void DeleteCache()
		{
			approximateSizePerEntry = 50;
			cache.Clear();
			string path = Path.Combine(SirenixAssetPaths.OdinTempPath, tempCacheFilename).Replace("\\", "/");
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}

		private GlobalPersistentContext<TValue> TryGetContext<TValue>(ContextKey key, out bool isNew)
		{
			EnsureIsInitialized();
			if (EnableCaching && cache.TryGetValue(key, out var value) && value is GlobalPersistentContext<TValue>)
			{
				isNew = false;
				return (GlobalPersistentContext<TValue>)value;
			}
			isNew = true;
			GlobalPersistentContext<TValue> globalPersistentContext = GlobalPersistentContext<TValue>.Create();
			cache[key] = globalPersistentContext;
			return globalPersistentContext;
		}
	}
}
