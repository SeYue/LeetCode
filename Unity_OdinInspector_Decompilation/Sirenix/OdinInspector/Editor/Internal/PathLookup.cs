using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class PathLookup<T> where T : class
	{
		public class Node
		{
			public int Version;

			public Dictionary<StringSlice, Node> Children;

			public int ValueIndex = -1;

			public bool ValuesExistForChildren;

			private static StringSlice[] childrenToRemove = new StringSlice[2];

			private static int childrenToRemoveCount;

			public void Cleanup(int version)
			{
				if (Children == null)
				{
					return;
				}
				childrenToRemoveCount = 0;
				foreach (KeyValuePair<StringSlice, Node> item in Children.GFIterator())
				{
					if (item.Value.Version != version)
					{
						if (childrenToRemoveCount >= childrenToRemove.Length)
						{
							Array.Resize(ref childrenToRemove, childrenToRemove.Length * 2);
						}
						childrenToRemove[childrenToRemoveCount++] = item.Key;
					}
				}
				for (int i = 0; i < childrenToRemoveCount; i++)
				{
					Children.Remove(childrenToRemove[i]);
				}
				foreach (Node item2 in Children.GFValueIterator())
				{
					item2.Cleanup(version);
				}
			}
		}

		public int Version;

		public Node Root = new Node();

		public int Count;

		public T[] Values = new T[32];

		public int CleanUpAtMostEveryXVersions = 20;

		public int VersionsSinceLastCleanUp;

		public bool IsRebuilding;

		public int NodesUpdatedToLatestVersion;

		public bool HasUnUpdatedNodesSinceLastCleanUp;

		public void BeginRebuild()
		{
			if (IsRebuilding)
			{
				throw new Exception("PathLookup is already rebuilding");
			}
			IsRebuilding = true;
			Version++;
			Root.Version = Version;
			Root.ValueIndex = -1;
			T[] values = Values;
			int count = Count;
			for (int i = 0; i < count; i++)
			{
				values[i] = null;
			}
			Count = 0;
			NodesUpdatedToLatestVersion = 0;
		}

		public void FinishRebuild()
		{
			if (!IsRebuilding)
			{
				throw new Exception("PathLookup is not rebuilding");
			}
			IsRebuilding = false;
			VersionsSinceLastCleanUp++;
			if (NodesUpdatedToLatestVersion != Count)
			{
				HasUnUpdatedNodesSinceLastCleanUp = true;
			}
			if (HasUnUpdatedNodesSinceLastCleanUp && VersionsSinceLastCleanUp >= CleanUpAtMostEveryXVersions)
			{
				HasUnUpdatedNodesSinceLastCleanUp = false;
				VersionsSinceLastCleanUp = 0;
				CleanUp();
			}
		}

		public void CleanUp()
		{
			int num = Values.Length;
			while (Count * 3 < num)
			{
				num /= 2;
			}
			if (num < Count)
			{
				num = Count;
			}
			if (num != Values.Length)
			{
				Array.Resize(ref Values, num);
			}
			Root.Cleanup(Version);
		}

		public bool TryGetValue(StringSlice path, out StringSlice nearestPath, out bool childValuesExistForValue, out T value)
		{
			Node node = Root;
			StringSlice stringSlice = path;
			nearestPath = default(StringSlice);
			value = null;
			childValuesExistForValue = false;
			StringSlice key;
			Node value2;
			while (true)
			{
				if (node.Version != Version || node.Children == null)
				{
					nearestPath = ((stringSlice.Index == path.Index) ? ((StringSlice)string.Empty) : path.Slice(0, stringSlice.Index - path.Index - 1));
					return false;
				}
				int num = stringSlice.FirstIndexOf('.');
				key = ((num == -1) ? stringSlice : stringSlice.Slice(0, num));
				if (!node.Children.TryGetValue(key, out value2))
				{
					nearestPath = ((key.Index == path.Index) ? ((StringSlice)string.Empty) : path.Slice(0, key.Index - path.Index - 1));
					return false;
				}
				if (num == -1)
				{
					break;
				}
				node = value2;
				stringSlice = stringSlice.Slice(num + 1);
			}
			childValuesExistForValue = value2.ValuesExistForChildren;
			if (value2.ValueIndex == -1)
			{
				nearestPath = ((key.Index == path.Index) ? ((StringSlice)string.Empty) : path.Slice(0, key.Index - path.Index - 1));
				return false;
			}
			value = Values[value2.ValueIndex];
			return true;
		}

		public void AddValue(StringSlice path, T value)
		{
			if (!IsRebuilding)
			{
				throw new Exception("Cannot add values to a PathLookup while it is not rebuilding");
			}
			int count = Count;
			while (count >= Values.Length)
			{
				T[] values = Values;
				T[] array = new T[Math.Max(values.Length, 4) * 2];
				for (int i = 0; i < values.Length; i++)
				{
					array[i] = values[i];
				}
				Values = array;
			}
			Values[count] = value;
			Count++;
			Node node = Root;
			StringSlice stringSlice = path;
			Node value2;
			while (true)
			{
				if (node.Version != Version)
				{
					node.Version = Version;
					node.ValueIndex = -1;
				}
				node.ValuesExistForChildren = true;
				int num = stringSlice.FirstIndexOf('.');
				if (node.Children == null)
				{
					node.Children = new Dictionary<StringSlice, Node>(StringSliceEqualityComparer.Instance);
				}
				StringSlice key = ((num == -1) ? stringSlice : stringSlice.Slice(0, num));
				if (!node.Children.TryGetValue(key, out value2))
				{
					value2 = new Node();
					node.Children.Add(key, value2);
				}
				if (value2.Version != Version)
				{
					value2.Version = Version;
					value2.ValueIndex = -1;
					value2.ValuesExistForChildren = false;
				}
				if (num == -1)
				{
					break;
				}
				node = value2;
				stringSlice = stringSlice.Slice(num + 1);
			}
			value2.ValueIndex = count;
		}
	}
}
