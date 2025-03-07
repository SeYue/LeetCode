using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using Sirenix.Utilities.Editor;

namespace Sirenix.OdinInspector.Editor
{
	[ResolverPriority(-1.0)]
	public class EditableKeyValuePairResolver<TKey, TValue> : OdinPropertyResolver<EditableKeyValuePair<TKey, TValue>>, IHasSpecialPropertyPaths, IMaySupportPrefabModifications
	{
		private static Dictionary<SerializationBackend, InspectorPropertyInfo[]> ChildInfos = new Dictionary<SerializationBackend, InspectorPropertyInfo[]>();

		private SerializationBackend backend;

		public bool MaySupportPrefabModifications => DictionaryKeyUtility.KeyTypeSupportsPersistentPaths(typeof(TKey));

		public string GetSpecialChildPath(int childIndex)
		{
			if (base.Property.Parent != null && base.Property.Parent.ChildResolver is IKeyValueMapResolver)
			{
				TKey key = base.ValueEntry.SmartValue.Key;
				string dictionaryKeyString = DictionaryKeyUtility.GetDictionaryKeyString(key);
				switch (childIndex)
				{
				case 0:
					return base.Property.Parent.Path + "." + dictionaryKeyString + "#key";
				case 1:
					return base.Property.Parent.Path + "." + dictionaryKeyString;
				}
			}
			else
			{
				switch (childIndex)
				{
				case 0:
					return base.Property.Path + ".Key";
				case 1:
					return base.Property.Path + ".Value";
				}
			}
			throw new ArgumentOutOfRangeException();
		}

		protected override void Initialize()
		{
			backend = base.Property.ValueEntry.SerializationBackend;
			if (!ChildInfos.ContainsKey(backend))
			{
				ChildInfos[backend] = InspectorPropertyInfoUtility.GetDefaultPropertiesForType(base.Property, typeof(EditableKeyValuePair<TKey, TValue>), includeSpeciallySerializedMembers: false);
			}
		}

		public override InspectorPropertyInfo GetChildInfo(int childIndex)
		{
			return ChildInfos[backend][childIndex];
		}

		protected override int GetChildCount(EditableKeyValuePair<TKey, TValue> value)
		{
			return ChildInfos[backend].Length;
		}

		public override int ChildNameToIndex(string name)
		{
			StringSlice name2 = name;
			return ChildNameToIndex(ref name2);
		}

		public override int ChildNameToIndex(ref StringSlice name)
		{
			if (name == "Key")
			{
				return 0;
			}
			if (name == "Value")
			{
				return 1;
			}
			if (name == "#Value")
			{
				return 1;
			}
			return -1;
		}
	}
}
