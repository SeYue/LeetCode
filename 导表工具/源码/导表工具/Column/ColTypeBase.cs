using System;
using System.Collections.Generic;

namespace 导表工具
{
	// 列类型,和表里一一对应
	public enum EColType
	{
		None = 0,
		Int,
		IntArray,
		Float,
		FloatArray,
		Bool,
		BoolArray,
		String,
		StringArray,
	}

	public abstract class ColTypeBase
	{
		// 类型枚举
		public EColType columnType;
		// 类型字符串
		public string valueStr;
		public string csType;

		protected ColTypeBase(EColType columnType, string valueStr, string csType)
		{
			this.columnType = columnType;
			this.valueStr = valueStr;
			this.csType = csType;
		}

		public bool EqualsValutStr(string excelColType)
		{
			return valueStr.Equals(excelColType);
		}

		public abstract object ExcelDataToValue(string data);

		// 切割字符串
		public static string[] SplitString(string data)
		{
			// 格式不正确
			if (!data.StartsWith("[") || !data.EndsWith("]"))
				return null;

			string temp = data.Replace("[", string.Empty).Replace("]", string.Empty);
			string[] temps = temp.Split(',');
			return temps;
		}
	}

	[ColType(true)]
	public class ValueType_Int : ColTypeBase
	{
		public ValueType_Int() : base(EColType.Int, "int", "int") { }

		public override object ExcelDataToValue(string data)
		{
			if (string.IsNullOrEmpty(data))
				return 0;
			return int.Parse(data);
		}
	}

	[ColType(true)]
	public class ValueType_IntArray : ColTypeBase
	{
		public ValueType_IntArray() : base(EColType.IntArray, "[int]", "int[]") { }

		public override object ExcelDataToValue(string data)
		{
			if (string.IsNullOrEmpty(data))
				return new int[0];

			string[] temps = SplitString(data);
			if (temps == null)
				return new int[0];

			int[] output = new int[temps.Length];
			for (int i = 0; i < output.Length; i++)
			{
				output[i] = int.Parse(temps[i]);
			}
			return output;
		}
	}

	[ColType(true)]
	public class ValueType_Float : ColTypeBase
	{
		public ValueType_Float() : base(EColType.Float, "float", "float") { }

		public override object ExcelDataToValue(string data)
		{
			if (string.IsNullOrEmpty(data))
				return 0;
			return float.Parse(data);
		}
	}

	[ColType(true)]
	public class ValueType_FloatArray : ColTypeBase
	{
		public ValueType_FloatArray() : base(EColType.FloatArray, "[float]", "float[]") { }

		public override object ExcelDataToValue(string data)
		{
			if (string.IsNullOrEmpty(data))
				return new float[0];

			string[] temps = SplitString(data);
			if (temps == null)
				return new float[0];

			float[] output = new float[temps.Length];
			for (int i = 0; i < output.Length; i++)
			{
				output[i] = float.Parse(temps[i]);
			}
			return output;
		}
	}

	[ColType(true)]
	public class ValueType_Bool : ColTypeBase
	{
		public ValueType_Bool() : base(EColType.Bool, "bool", "bool") { }

		public override object ExcelDataToValue(string data)
		{
			if (string.IsNullOrEmpty(data))
				return false;
			return bool.Parse(data);
		}
	}

	[ColType(true)]
	public class ValueType_BoolArray : ColTypeBase
	{
		public ValueType_BoolArray() : base(EColType.BoolArray, "[bool]", "bool[]") { }

		public override object ExcelDataToValue(string data)
		{
			if (string.IsNullOrEmpty(data))
				return new bool[0];

			string[] temps = SplitString(data);
			if (temps == null)
				return new bool[0];

			bool[] output = new bool[temps.Length];
			for (int i = 0; i < output.Length; i++)
			{
				output[i] = bool.Parse(temps[i]);
			}
			return output;
		}
	}

	[ColType(true)]
	public class ValueType_String : ColTypeBase
	{
		public ValueType_String() : base(EColType.String, "string", "string") { }

		public override object ExcelDataToValue(string data)
		{
			return data;
		}
	}

	[ColType(true)]
	public class ValueType_StringArray : ColTypeBase
	{
		public ValueType_StringArray() : base(EColType.StringArray, "[string]", "string[]") { }

		public override object ExcelDataToValue(string data)
		{
			if (string.IsNullOrEmpty(data))
				return new string[0];

			string[] temps = SplitString(data);
			if (temps == null)
				return new string[0];

			string[] output = new string[temps.Length];
			for (int i = 0; i < output.Length; i++)
			{
				output[i] = temps[i];
			}
			return output;
		}
	}
}
