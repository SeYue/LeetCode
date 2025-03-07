using ExcelDataReader;
using System;
using System.Data;
using System.IO;
using System.Text;

namespace 导表工具
{
	public class ExcelTools
	{
		public static DataSet GetExcelDataSet(string filePath)
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			using (FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				using (IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fileStream))
				{
					return GetDataSet(filePath);// excelReader.AsDataSet();
				}
			}
		}

		static DataSet GetDataSet(string path)
		{
			using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				using (IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream))
				{
					DataSet ds = new DataSet();
					do
					{
						DataTable dt = GetTable(excelReader);
						ds.Merge(dt);
					} while (excelReader.NextResult());

					return ds;
				}
			}
		}

		// 从excel到cs
		static DataTable GetTable(IExcelDataReader excelReader)
		{
			DataTable dt = new DataTable();
			dt.TableName = excelReader.Name;

			bool isInit = false;
			string[] ItemArray = null;
			int rowsNum = 0;
			while (excelReader.Read())
			{
				rowsNum++;
				if (!isInit)
				{
					isInit = true;
					for (int i = 0; i < excelReader.FieldCount; i++)
					{
						dt.Columns.Add("", typeof(string));
					}
					ItemArray = new string[excelReader.FieldCount];
				}

				for (int i = 0; i < excelReader.FieldCount; i++)
				{
					string value = excelReader.IsDBNull(i) ? "" : Convert.ToString(excelReader.GetValue(i));
					ItemArray[i] = value;
				}
				dt.Rows.Add(ItemArray);
			}
			return dt;
		}
	}
}