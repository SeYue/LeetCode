using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace 导表工具
{
	/*

	导表工具具体逻辑:
	1.获取全部文件
	获取全部.xlsx后缀的表文件,不包括~$开头的临时文件

	2.遍历并校验表数据
	2.1 获取表名，判断表名是否重复
	2.2 获取列名，判断列名是否重复
	2.3 获取类型，判断类型是否正确，是否可以解析
	2.4 生成类结构
	2.5 根据类结构生成类数据

	2.6 等待所有表都录入完成后，开始跳转校验
	2.7 根据类结构序列化生成cs类文件
	2.8 根据类结构序列化生成bytes二进制文件

	 */
	class Program
	{
		static void Main(string[] args)
		{
			CopyPath cp = new CopyPath();
			string s1 = JsonConvert.SerializeObject(cp);
			File.WriteAllText("config.txt", s1);
			return;
			string str = File.ReadAllText("config.txt", Encoding.UTF8);
			CopyPath copyPath = JsonConvert.DeserializeObject<CopyPath>(str);

			Start();
		}

		static void Start()
		{
			// 初始化
			ColTypeTools.Init();
			TotalClass.Init();

			// 获取文件夹
			Log.Info($"运行exe路径:{Config.cmdPath}");

			string excelDirectory = Path.Combine(Config.cmdPath, Config.excelPath);
			if (!Directory.Exists(excelDirectory))
			{
				Log.Error($"Excel文件夹不存在:{excelDirectory}");
				return;
			}

			// 获取文件列表
			string[] excels = Directory.GetFiles(excelDirectory, "*.xlsx");
			List<ExcelInfo> excelInfos = new List<ExcelInfo>();
			for (int i = excels.Length - 1; i >= 0; i--)
			{
				string filePath = excels[i];
				string fileName = Path.GetFileName(filePath);
				if (fileName.StartsWith("~$"))
				{
					Log.Warrning($"跳过临时文件{fileName}");
					continue;
				}

				ExcelInfo excelInfo = new ExcelInfo()
				{
					filePath = filePath,
					fileName = fileName,
					fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName),
				};
				excelInfos.Add(excelInfo);
			}

			Log.Info($"总共有{excelInfos.Count}个表\n");

			// 遍历表,收集表数据
			Log.Info($"开始解析表数据:");
			foreach (var i in excelInfos)
			{
				Log.Info($"正在解析:{i.fileName}");
				if (!i.AnalyseExcel())
					return;
			}

			// 校验表数据
			Log.Info("\n开始校验表数据:");
			foreach (ExcelInfo i in excelInfos)
			{
				Log.Info($"正在校验:{i.fileName}");
				// 判断表名是否重复
				foreach (var j in excelInfos)
				{
					if (j != i && j.excelName == i.excelName)
					{
						Log.Error($"表名重复:{j.fileName}, {i.fileName}");
						return;
					}
				}

				// id不允许重复
				HashSet<int> idList = new HashSet<int>();
				foreach (var j in i.rowDatas)
				{
					if (!idList.Contains(j.Id))
						idList.Add(j.Id);
					else
					{
						Log.Error($"id重复:{j.Id}");
						return;
					}
				}

				foreach (var j in i.columnInfos)
				{
					if (j.checkExcel)
					{
						ExcelInfo targetExcel = excelInfos.Find(x => x.excelName == j.checkExcelName);
						if (targetExcel == null)
						{
							Log.Error($"对应表{j.checkExcelName}不存在");
							return;
						}

						// 遍历每一行的数据
						foreach (var k in i.rowDatas)
						{
							// 该行每一个格子
							GridData gridData = k.GetData(j.name);
							if (gridData != null && gridData.data != null)
							{
								if (gridData.data.GetType().IsArray)
								{
									// 数组还没处理
									foreach (int gridDataObj in gridData.data as int[])
									{
										object gridObj = targetExcel.GetData(gridDataObj, "Id");
										if (gridObj == null)
										{
											Log.Error($"{j.checkExcelName}表,{"Id"}列,{gridDataObj}不存在");
											return;
										}
									}
								}
								else
								{
									object gridObj = targetExcel.GetData((int)gridData.data, "Id");
									if (gridObj == null)
									{
										Log.Error($"{j.checkExcelName}表,{"Id"}列,{gridData.data}不存在");
										return;
									}
								}
							}
						}
					}
				}
			}

			Log.Info("\n开始导出外部文件");
			foreach (var i in excelInfos)
			{
				i.CreateFile();
			}

			// 生成dll
			Log.Info("\n开始生成dll");
			TotalClass.CreateDll();

			// 生成bytes数据
			Log.Info("\n开始生成bytes");
			foreach (ExcelInfo i in excelInfos)
			{
				Log.Info($"{i.fileName}");
				i.CreateBytes();
			}

			// 开始复制数据
			Thread.Sleep(100);
			Log.Info("\n开始复制文件夹");
			Log.Info($"复制文件夹:{Config.dllCopyDirectory}");
			DirectoryTools.Copy(Config.outputDll, Config.dllCopyDirectory);

			Log.Info($"复制文件夹:{Config.bytesCopyDirectory}");
			DirectoryTools.Copy(Config.outputBytes, Config.bytesCopyDirectory);

			Log.Info("\n导表结束,按任意键关闭");
			Console.ReadKey();
		}
	}
}