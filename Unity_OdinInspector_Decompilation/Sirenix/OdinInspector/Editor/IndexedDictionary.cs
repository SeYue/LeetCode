using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Serialization;

namespace Sirenix.OdinInspector.Editor
{
	[AlwaysFormatsSelf]
	internal class IndexedDictionary : IEnumerable<KeyValuePair<ContextKey, GlobalPersistentContext>>, IEnumerable, ISelfFormatter
	{
		private class CKC : IEqualityComparer<ContextKey>
		{
			public bool Equals(ContextKey x, ContextKey y)
			{
				if (x.Key1234 == y.Key1234)
				{
					return x.Key5 == y.Key5;
				}
				return false;
			}

			public int GetHashCode(ContextKey obj)
			{
				return obj.GetHashCode();
			}
		}

		private Dictionary<ContextKey, GlobalPersistentContext> dictionary;

		private List<ContextKey> indexer;

		private static readonly Dictionary<Type, Type> GlobalPersistentContext_GenericVariantCache = new Dictionary<Type, Type>(FastTypeComparer.Instance);

		private static readonly Serializer<Type> TypeSerializer = Serializer.Get<Type>();

		public int Count => dictionary.Count;

		public GlobalPersistentContext this[ContextKey key]
		{
			get
			{
				return dictionary[key];
			}
			set
			{
				if (dictionary.ContainsKey(key))
				{
					dictionary[key] = value;
				}
				else
				{
					Add(key, value);
				}
			}
		}

		public IndexedDictionary()
		{
			dictionary = new Dictionary<ContextKey, GlobalPersistentContext>(0, new CKC());
			indexer = new List<ContextKey>(0);
		}

		public KeyValuePair<ContextKey, GlobalPersistentContext> Get(int index)
		{
			ContextKey key = indexer[index];
			dictionary.TryGetValue(key, out var value);
			return new KeyValuePair<ContextKey, GlobalPersistentContext>(key, value);
		}

		public ContextKey GeContextKey(int index)
		{
			return indexer[index];
		}

		public void Add(ContextKey key, GlobalPersistentContext value)
		{
			dictionary.Add(key, value);
			indexer.Add(key);
		}

		public void Clear()
		{
			indexer.Clear();
			dictionary.Clear();
		}

		public void RemoveAt(int index)
		{
			if (index >= 0 && index < Count)
			{
				ContextKey key = indexer[index];
				if (!dictionary.Remove(key))
				{
					throw new Exception("Fuck");
				}
				indexer.RemoveAt(index);
			}
		}

		public bool TryGetValue(ContextKey key, out GlobalPersistentContext value)
		{
			return dictionary.TryGetValue(key, out value);
		}

		public IEnumerator<KeyValuePair<ContextKey, GlobalPersistentContext>> GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<ContextKey, GlobalPersistentContext>>)dictionary).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<ContextKey, GlobalPersistentContext>>)dictionary).GetEnumerator();
		}

		public void Serialize(IDataWriter writer)
		{
			writer.BeginArrayNode(indexer.Count);
			for (int i = 0; i < indexer.Count; i++)
			{
				writer.BeginStructNode(null, null);
				ContextKey contextKey = indexer[i];
				GlobalPersistentContext value = Get(i).Value;
				contextKey.Serialize(writer);
				if (value == null)
				{
					writer.WriteNull(null);
				}
				else
				{
					TypeSerializer.WriteValue(value.ValueType, writer);
					value.Serialize(writer);
				}
				writer.EndNode(null);
			}
			writer.EndArrayNode();
		}

		public void Deserialize(IDataReader reader)
		{
			reader.EnterArray(out var length);
			indexer = new List<ContextKey>((int)length);
			dictionary = new Dictionary<ContextKey, GlobalPersistentContext>((int)length, new CKC());
			for (int i = 0; i < length; i++)
			{
				reader.EnterNode(out var _);
				ContextKey key = default(ContextKey);
				key.Deserialize(reader);
				string name;
				EntryType entryType = reader.PeekEntry(out name);
				if (entryType == EntryType.Null)
				{
					reader.ReadNull();
				}
				else
				{
					Type type2 = TypeSerializer.ReadValue(reader);
					GlobalPersistentContext globalPersistentContext = null;
					if (type2 != null)
					{
						Type value;
						lock (GlobalPersistentContext_GenericVariantCache)
						{
							if (!GlobalPersistentContext_GenericVariantCache.TryGetValue(type2, out value))
							{
								value = typeof(GlobalPersistentContext<>).MakeGenericType(type2);
								GlobalPersistentContext_GenericVariantCache.Add(type2, value);
							}
						}
						globalPersistentContext = (GlobalPersistentContext)Activator.CreateInstance(value);
						globalPersistentContext.Deserialize(reader);
						Add(key, globalPersistentContext);
					}
				}
				reader.ExitNode();
			}
			reader.ExitArray();
		}
	}
}
