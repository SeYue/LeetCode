using System;
using System.Collections.Generic;
using System.Reflection;

namespace 导表工具
{
	public class ColTypeTools
	{
		// 配置类型
		static List<ColTypeBase> m_columnTypeDic = new List<ColTypeBase>();

		public static void Init()
		{
			Assembly asm = Assembly.GetAssembly(typeof(ColTypeAttribute));
			Type[] types = asm.GetExportedTypes();
			foreach (var i in types)
			{
				ColTypeAttribute attribute = i.GetCustomAttribute<ColTypeAttribute>();
				if (attribute != null && attribute.isUsing)
				{
					// 创建实例
					object instance = i.Assembly.CreateInstance(i.FullName);
					m_columnTypeDic.Add(instance as ColTypeBase);
				}
			}
		}

		public static bool IsVailtCol(string excelColName)
		{
			return GetColumnType(excelColName) != EColType.None;
		}

		// 获取列类型
		public static EColType GetColumnType(string excelColName)
		{
			foreach (ColTypeBase i in m_columnTypeDic)
			{
				if (i.EqualsValutStr(excelColName))
				{
					return i.columnType;
				}
			}
			Log.Error($"未定义的列:{excelColName}");
			return EColType.None;
		}

		// 获取这个格子的值
		public static object GetColumnValue(EColType valueType, string data)
		{
			foreach (ColTypeBase i in m_columnTypeDic)
			{
				if (i.columnType == valueType)
				{
					return i.ExcelDataToValue(data);
				}
			}
			Log.Error($"未定义的列:{valueType}");
			return EColType.None;
		}

		public static string GetCSType(EColType valueType)
		{
			foreach (ColTypeBase i in m_columnTypeDic)
			{
				if (i.columnType == valueType)
				{
					return i.csType.ToString();
				}
			}
			Log.Error("找不到cs类型");
			return string.Empty;
		}
	}
}
