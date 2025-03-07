using System.Text;
using Sirenix.Serialization.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Contains information about a change that is going to occur/has occurred to a collection.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.CollectionChangeType" />
	public struct CollectionChangeInfo
	{
		public CollectionChangeType ChangeType;

		public object Key;

		public object Value;

		public int Index;

		public int SelectionIndex;

		public override string ToString()
		{
			using Cache<StringBuilder> cache = Cache<StringBuilder>.Claim();
			StringBuilder value = cache.Value;
			value.Length = 0;
			value.Append("CollectionChangeInfo { ");
			AppendValue(value, "ChangeType", ChangeType, prependComma: false);
			switch (ChangeType)
			{
			case CollectionChangeType.Unspecified:
				AppendValue(value, "Key", Key);
				AppendValue(value, "Value", Value);
				AppendValue(value, "Index", Index);
				break;
			case CollectionChangeType.Add:
				AppendValue(value, "Value", Value);
				break;
			case CollectionChangeType.Insert:
				AppendValue(value, "Value", Value);
				AppendValue(value, "Index", Index);
				break;
			case CollectionChangeType.RemoveValue:
				AppendValue(value, "Value", Value);
				break;
			case CollectionChangeType.RemoveIndex:
				AppendValue(value, "Index", Index);
				break;
			case CollectionChangeType.RemoveKey:
				AppendValue(value, "Key", Key);
				break;
			case CollectionChangeType.SetKey:
				AppendValue(value, "Key", Key);
				AppendValue(value, "Value", Value);
				break;
			}
			AppendValue(value, "SelectionIndex", SelectionIndex);
			value.Append(" }");
			return value.ToString();
		}

		private static void AppendValue(StringBuilder sb, string name, object value, bool prependComma = true)
		{
			if (prependComma)
			{
				sb.Append(", ");
			}
			sb.Append(name);
			sb.Append(" = ");
			sb.Append(value ?? "null");
		}
	}
}
