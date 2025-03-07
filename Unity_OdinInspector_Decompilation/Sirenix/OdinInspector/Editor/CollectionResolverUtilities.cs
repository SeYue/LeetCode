using Sirenix.Utilities.Editor;

namespace Sirenix.OdinInspector.Editor
{
	public static class CollectionResolverUtilities
	{
		public static string DefaultIndexToChildName(int index)
		{
			return "$" + index;
		}

		public static int DefaultChildNameToIndex(ref StringSlice name)
		{
			if (name.Length <= 1)
			{
				return -1;
			}
			StringSlice stringSlice;
			if (name[0] == '$')
			{
				stringSlice = name.Slice(1);
			}
			else
			{
				if (name.Length <= 2 || name[0] != '[' || name[name.Length - 1] != ']')
				{
					return -1;
				}
				stringSlice = name.Slice(1, name.Length - 2);
			}
			if (stringSlice.TryParseToInt(out var result) && result >= 0)
			{
				return result;
			}
			return -1;
		}

		public static int DefaultChildNameToIndex(string name)
		{
			StringSlice name2 = name;
			return DefaultChildNameToIndex(ref name2);
		}
	}
}
