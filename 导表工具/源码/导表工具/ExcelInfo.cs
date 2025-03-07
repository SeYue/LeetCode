using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace 导表工具
{
	// 表数据
	public class ExcelInfo
	{
		// 文件信息
		public string filePath;
		public string fileName;
		public string fileNameWithoutExtension;

		// 表信息
		public string excelName;
		public List<ColumnInfo> columnInfos = new List<ColumnInfo>();
		public List<RowData> rowDatas = new List<RowData>();

		// dll信息
		public string dicClassName;
		public string rowClassName;

		public bool AnalyseExcel()
		{
			DataSet dataSet = ExcelTools.GetExcelDataSet(filePath);
			DataTableCollection tables = dataSet.Tables;
			for (int i = 0; i < tables.Count; i++)
			{
				if (!AnalyseSheet(tables[i]))
					return false;
			}
			return true;
		}

		// 解析表
		bool AnalyseSheet(DataTable table)
		{
			// 遍历每一行
			for (int i = 0; i < table.Rows.Count; i++)
			{
				// 解析这一行的数据
				DataRow row = table.Rows[i];
				List<string> listDatas = AnalyseRow(row);
				// 行数据不合法
				if (listDatas.Count == 0)
					continue;
				if (!listDatas.Exists(x => !string.IsNullOrEmpty(x)))
					continue;

				string firstGrid = listDatas[0].ToString().Trim();
				// 空行跳过
				if (firstGrid.StartsWith("#"))
					continue;

				switch (i + 1)  // i+1是因为excel表里的行数是从1开始的
				{
					// 表名
					case 2:
						excelName = firstGrid.ToString();
						break;
					// 列名
					case 7:
						int valitColCount = 0;
						for (int colIndex = 0; colIndex < listDatas.Count; colIndex++)
						{
							string colName = listDatas[colIndex].ToString().Trim();
							bool isColValit = !string.IsNullOrEmpty(colName);

							// 合法的列数量
							if (isColValit)
								valitColCount++;

							columnInfos.Add(new ColumnInfo()
							{
								index = colIndex,
								isColValit = isColValit,
								name = colName,
							});
						}

						if (valitColCount <= 0)
						{
							Log.Error($"表{excelName}没有合法的列");
							return false;
						}

						break;
					case 8:
						// 列类型
						for (int colIndex = 0; colIndex < listDatas.Count; colIndex++)
						{
							ColumnInfo columnInfo = columnInfos[colIndex];
							// 空列则跳过
							if (!columnInfo.isColValit)
								continue;

							string typeName = listDatas[colIndex].ToString();
							columnInfo.valueType = ColTypeTools.GetColumnType(typeName);
							if (columnInfo.valueType == EColType.None)
							{
								Log.Error($"列类型不合法:{typeName}");
								return false;
							}
						}
						break;
					case 11:
						// 该列可否为空
						for (int colIndex = 0; colIndex < listDatas.Count; colIndex++)
						{
							ColumnInfo columnInfo = columnInfos[colIndex];
							// 空列则跳过
							if (!columnInfo.isColValit)
								continue;

							string boolValue = listDatas[colIndex].ToString().Trim().ToLower();
							bool cantNull;
							if (string.IsNullOrEmpty(boolValue) || boolValue.Equals("false"))
								cantNull = false;
							else if (boolValue.Equals("true"))
								cantNull = true;
							else
							{
								Log.Error($"校验列填写错误:{columnInfo.name}");
								return false;
							}
							// 可否为空
							columnInfo.cantNull = cantNull;
						}
						break;
					case 13:
						// 连接到其他表的检验数据
						for (int colIndex = 0; colIndex < listDatas.Count; colIndex++)
						{
							ColumnInfo columnInfo = columnInfos[colIndex];
							if (!columnInfo.isColValit)
								continue;

							string str = listDatas[colIndex].ToString().Trim();
							//string[] checkExcels = str.Split(',');
							if (!string.IsNullOrEmpty(str))
							{
								columnInfo.checkExcel = true;
								columnInfo.checkExcelName = str;
							}
						}
						break;
					default:
						// 其余的每一行都是一条表数据
						RowData rowData = new RowData();
						for (int colIndex = 0; colIndex < listDatas.Count; colIndex++)
						{
							ColumnInfo columnInfo = columnInfos[colIndex];
							// 空列则跳过
							if (!columnInfo.isColValit)
								continue;

							string dataValue = listDatas[colIndex].ToString().Trim();

							// 空值
							if (string.IsNullOrEmpty(dataValue))
							{
								// 如果可空，就返回空；如果不可空，就报错
								if (columnInfo.cantNull)
								{
									Log.Error($"该列不能为空:{columnInfo.name}");
									return false;
								}
							}

							GridData gridData = new GridData()
							{
								colName = columnInfo.name,
								data = ColTypeTools.GetColumnValue(columnInfo.valueType, dataValue)
							};
							rowData.gridDatas.Add(gridData);
						}
						rowDatas.Add(rowData);

						rowData.Id = (int)rowData.gridDatas[0].data;
						break;
				}
			}
			return true;
		}

		// 解析行
		List<string> AnalyseRow(DataRow row)
		{
			List<string> objs = new List<string>();
			for (int i = 0; i < row.ItemArray.Length; i++)
			{
				objs.Add(row.ItemArray[i].ToString());
			}
			return objs;
		}

		// 解析数据后,生成外部数据
		public void CreateFile()
		{
			Log.Info(fileName);
			//CreateJson();
			CreateCS();
		}

		// 生成json
		void CreateJson()
		{
			string outputPath = Path.Combine(Config.cmdPath, Config.outputDirectory + "/Json");
			if (Directory.Exists(outputPath))
				Directory.Delete(outputPath, true);
			Directory.CreateDirectory(outputPath);

			string jsonStr = JsonConvert.SerializeObject(rowDatas);
			File.WriteAllText($"{outputPath}/{excelName}.json", jsonStr);
		}

		// 创建CS用的文件需要创建两部分，1部分是代码，1部分是二进制文件
		void CreateCS()
		{
			string outputPath = Path.Combine(Config.cmdPath, Config.outputDirectory + "/CS");
			if (Directory.Exists(outputPath))
				Directory.Delete(outputPath, true);

			// 1.创建CS文件夹
			outputPath = Path.Combine(Config.cmdPath, Config.outputDirectory + "/CS/CSFile");
			Directory.CreateDirectory(outputPath);

			// 创建cs文件
			dicClassName = $"{excelName.ToUpper()}_DIC";
			rowClassName = excelName.ToUpper();

			StringBuilder sb = new StringBuilder();
			foreach (ColumnInfo i in columnInfos)
			{
				sb.AppendLine($"\tpublic {ColTypeTools.GetCSType(i.valueType)} {i.name.ToUpper()};");
			}

			string str =
			$@"using System;
			using System.IO;
			using System.Collections.Generic;

			public class {dicClassName}
			{{
				const string fileName = ""{dicClassName}"";
				static Dictionary<int, {rowClassName}> para;
				public static Dictionary<int, {rowClassName}> Para => para;

				static {dicClassName}()
				{{
					if (!ExcelLoaderConfig.m_init)
						return;
					using (FileStream fs = new FileStream(ExcelLoaderConfig.bytesDirectory +""{rowClassName}.bytes"", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
					{{
						var bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
						para = bf.Deserialize(fs) as Dictionary<int, {rowClassName}>;
					}}
				}}
			}}

			[Serializable] // 单条数据的数据文件
			public class {rowClassName}
			{{
				{sb}
			}};";

			File.WriteAllText($"{ outputPath}/{ excelName.ToUpper()}.cs", str);
			TotalClass.Add(str.ToString());
		}

		// 2.生成bytes,首先动态生成类结构,然后用类结构去创建类实例，然后将类实例序列化成bytes
		public void CreateBytes()
		{
			// 读取整表结构
			Assembly assembly = Assembly.LoadFile(Config.dllPath);

			// 读取每一条数据的结构
			Type rowDataType = assembly.GetType(rowClassName);

			// 反射创建字典类型
			Type generic = typeof(Dictionary<,>);
			Type[] typeArgs2 = { typeof(int), rowDataType };
			generic = generic.MakeGenericType(typeArgs2);

			IDictionary dic = Activator.CreateInstance(generic) as IDictionary;

			// 给每一条数据创建字段
			foreach (RowData i in rowDatas)
			{
				object rowInstance = assembly.CreateInstance(rowClassName);
				foreach (GridData gridData in i.gridDatas)
				{
					FieldInfo colInfo = rowDataType.GetField(gridData.colName.ToUpper());
					colInfo.SetValue(rowInstance, gridData.data);
				}

				dic[i.Id] = rowInstance;
			}

			if (!Directory.Exists(Config.outputBytes))
				Directory.CreateDirectory(Config.outputBytes);

			string filePath = $"{Config.outputBytes}/{excelName.ToUpper()}.bytes";
			using (MemoryStream ms = new MemoryStream())
			{
				BinaryFormatter bf = new BinaryFormatter();
				bf.Serialize(ms, dic);
				File.WriteAllBytes(filePath, ms.ToArray());
			}
		}

		// 获取对应id对应列的数据
		public object GetData(int id, string colName)
		{
			RowData rowData = rowDatas.Find(x => x.Id == id);
			if (rowData != null)
			{
				GridData gridData = rowData.GetData(colName);
				if (gridData != null)
				{
					return gridData.data;
				}
			}
			return null;
		}
	}

	public enum EAnalyseExcelError
	{
		Fail = 0,   // 失败
		Success,    // 成功
		ExcelNameEmpty, // 失败,表名为空
		NotValitCol,    // 失败,表没有有效列
		Fail_CheckNullError,    // 校验列，空校验填写出错
		Fail_CantNull,    // 校验列，不允许空值
	}

	// 列数据
	public class ColumnInfo
	{
		public int index;
		public bool isColValit; // 这一类是否是有效的

		public string name;     // 列名
		public EColType valueType;  // 列类型

		public bool cantNull;    // 该列可否为空

		// 链接到对应表
		public bool checkExcel;
		public string checkExcelName;
		//public string checkExcelColName;
	}

	[Serializable]
	public class RowData
	{
		public int Id;
		public List<GridData> gridDatas = new List<GridData>();

		public GridData GetData(string colName)
		{
			return gridDatas.Find(x => x.colName == colName);
		}
	}

	[Serializable]
	public class GridData
	{
		public string colName;
		public object data;
	}
}
