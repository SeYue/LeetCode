using System;
using System.Text;
using Sirenix.Serialization.Utilities;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
	public struct NamedValues
	{
		private const int BASE_VALUES_COUNT = 8;

		private NamedValue v0;

		private NamedValue v1;

		private NamedValue v2;

		private NamedValue v3;

		private NamedValue v4;

		private NamedValue v5;

		private NamedValue v6;

		private NamedValue v7;

		private NamedValue[] array;

		private int count;

		public int Count => count;

		public NamedValue this[int index]
		{
			get
			{
				switch (index)
				{
				case 0:
					return v0;
				case 1:
					return v1;
				case 2:
					return v2;
				case 3:
					return v3;
				case 4:
					return v4;
				case 5:
					return v5;
				case 6:
					return v6;
				case 7:
					return v7;
				default:
					if (array != null && index - 8 < array.Length)
					{
						return array[index - 8];
					}
					throw new IndexOutOfRangeException();
				}
			}
		}

		public void UpdateValues(ref ValueResolverContext context, int selectionIndex)
		{
			if (v0.Type == null)
			{
				return;
			}
			if (v0.ValueGetter != null)
			{
				v0.CurrentValue = v0.ValueGetter(ref context, selectionIndex);
			}
			if (v1.Type == null)
			{
				return;
			}
			if (v1.ValueGetter != null)
			{
				v1.CurrentValue = v1.ValueGetter(ref context, selectionIndex);
			}
			if (v2.Type == null)
			{
				return;
			}
			if (v2.ValueGetter != null)
			{
				v2.CurrentValue = v2.ValueGetter(ref context, selectionIndex);
			}
			if (v3.Type == null)
			{
				return;
			}
			if (v3.ValueGetter != null)
			{
				v3.CurrentValue = v3.ValueGetter(ref context, selectionIndex);
			}
			if (v4.Type == null)
			{
				return;
			}
			if (v4.ValueGetter != null)
			{
				v4.CurrentValue = v4.ValueGetter(ref context, selectionIndex);
			}
			if (v5.Type == null)
			{
				return;
			}
			if (v5.ValueGetter != null)
			{
				v5.CurrentValue = v5.ValueGetter(ref context, selectionIndex);
			}
			if (v6.Type == null)
			{
				return;
			}
			if (v6.ValueGetter != null)
			{
				v6.CurrentValue = v6.ValueGetter(ref context, selectionIndex);
			}
			if (v7.Type == null)
			{
				return;
			}
			if (v7.ValueGetter != null)
			{
				v7.CurrentValue = v7.ValueGetter(ref context, selectionIndex);
			}
			NamedValue[] array = this.array;
			if (array == null)
			{
				return;
			}
			for (int i = 0; i < array.Length && array[i].Type != null; i++)
			{
				ValueResolverFunc<object> valueGetter = array[i].ValueGetter;
				if (valueGetter != null)
				{
					array[i].CurrentValue = valueGetter(ref context, selectionIndex);
				}
			}
		}

		public void Set(string name, object value)
		{
			if (v0.Name == name)
			{
				v0.CurrentValue = value;
				return;
			}
			if (v1.Name == name)
			{
				v1.CurrentValue = value;
				return;
			}
			if (v2.Name == name)
			{
				v2.CurrentValue = value;
				return;
			}
			if (v3.Name == name)
			{
				v3.CurrentValue = value;
				return;
			}
			if (v4.Name == name)
			{
				v4.CurrentValue = value;
				return;
			}
			if (v5.Name == name)
			{
				v5.CurrentValue = value;
				return;
			}
			if (v6.Name == name)
			{
				v6.CurrentValue = value;
				return;
			}
			if (v7.Name == name)
			{
				v7.CurrentValue = value;
				return;
			}
			NamedValue[] array = this.array;
			if (array != null)
			{
				for (int i = 0; i < array.Length; i++)
				{
					string name2 = array[i].Name;
					if (name2 == null)
					{
						break;
					}
					if (name2 == name)
					{
						array[i].CurrentValue = value;
						return;
					}
				}
			}
			throw new ArgumentException("No named value '" + name + "' found to set.");
		}

		public object GetValue(string name)
		{
			if (v0.Name == name)
			{
				return v0.CurrentValue;
			}
			if (v1.Name == name)
			{
				return v1.CurrentValue;
			}
			if (v2.Name == name)
			{
				return v2.CurrentValue;
			}
			if (v3.Name == name)
			{
				return v3.CurrentValue;
			}
			if (v4.Name == name)
			{
				return v4.CurrentValue;
			}
			if (v5.Name == name)
			{
				return v5.CurrentValue;
			}
			if (v6.Name == name)
			{
				return v6.CurrentValue;
			}
			if (v7.Name == name)
			{
				return v7.CurrentValue;
			}
			NamedValue[] array = this.array;
			if (array != null)
			{
				for (int i = 0; i < array.Length; i++)
				{
					string name2 = array[i].Name;
					if (name2 == null)
					{
						break;
					}
					if (name2 == name)
					{
						return array[i].CurrentValue;
					}
				}
			}
			throw new ArgumentException("No named value '" + name + "' found to get.");
		}

		public bool TryGetValue(string name, out NamedValue value)
		{
			if (v0.Name == name)
			{
				value = v0;
				return true;
			}
			if (v1.Name == name)
			{
				value = v1;
				return true;
			}
			if (v2.Name == name)
			{
				value = v2;
				return true;
			}
			if (v3.Name == name)
			{
				value = v3;
				return true;
			}
			if (v4.Name == name)
			{
				value = v4;
				return true;
			}
			if (v5.Name == name)
			{
				value = v5;
				return true;
			}
			if (v6.Name == name)
			{
				value = v6;
				return true;
			}
			if (v7.Name == name)
			{
				value = v7;
				return true;
			}
			NamedValue[] array = this.array;
			if (array != null)
			{
				for (int i = 0; i < array.Length; i++)
				{
					string name2 = array[i].Name;
					if (name2 == null)
					{
						break;
					}
					if (name2 == name)
					{
						value = array[i];
						return true;
					}
				}
			}
			value = default(NamedValue);
			return false;
		}

		public void Add(string name, Type type, ValueResolverFunc<object> valueGetter)
		{
			Add(new NamedValue
			{
				Name = name,
				Type = type,
				CurrentValue = null,
				ValueGetter = valueGetter
			});
		}

		public void Add(string name, Type type, object value)
		{
			Add(new NamedValue
			{
				Name = name,
				Type = type,
				CurrentValue = value,
				ValueGetter = null
			});
		}

		public void Add(NamedValue value)
		{
			switch (count)
			{
			case 0:
				v0 = value;
				break;
			case 1:
				v1 = value;
				break;
			case 2:
				v2 = value;
				break;
			case 3:
				v3 = value;
				break;
			case 4:
				v4 = value;
				break;
			case 5:
				v5 = value;
				break;
			case 6:
				v6 = value;
				break;
			case 7:
				v7 = value;
				break;
			case 8:
				array = new NamedValue[4];
				array[0] = value;
				break;
			default:
			{
				int num = count - 8;
				if (num < array.Length)
				{
					array[num] = value;
					break;
				}
				NamedValue[] destinationArray = new NamedValue[array.Length * 2];
				Array.Copy(array, destinationArray, array.Length);
				array = destinationArray;
				array[num] = value;
				break;
			}
			}
			count++;
		}

		public string GetValueOverviewString()
		{
			int num = count;
			using Cache<StringBuilder> cache = Cache<StringBuilder>.Claim();
			StringBuilder value = cache.Value;
			value.Length = 0;
			for (int i = 0; i < num; i++)
			{
				if (i > 0)
				{
					value.AppendLine();
				}
				NamedValue namedValue = this[i];
				value.Append(namedValue.Name);
				value.Append(" (");
				value.Append(namedValue.Type.GetNiceName());
				value.Append(")");
			}
			return value.ToString();
		}
	}
}
