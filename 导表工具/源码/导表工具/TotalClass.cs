using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace 导表工具
{
	public class TotalClass
	{
		public static List<string> m_classString = new List<string>();

		public static void Init()
		{
			m_classString.Clear();
		}

		public static void Add(string v)
		{
			m_classString.Add(v);
		}

		public static void CreateDll()
		{
			if (m_classString.Count == 0)
				return;

			// 创建文件夹
			if (Directory.Exists(Config.outputDll))
				Directory.Delete(Config.outputDll, true);
			Directory.CreateDirectory(Config.outputDll);

			// 代码
			List<SyntaxTree> trees = new List<SyntaxTree>();

			//生成版本信息
			//StringBuilder asmInfo = new StringBuilder();
			//asmInfo.AppendLine("using System.Reflection;");
			//asmInfo.AppendLine($"[assembly: AssemblyTitle(\"{Config.dllName}\")]");
			//asmInfo.AppendLine("[assembly: AssemblyVersion(\"1.0.0.0\")]");
			//asmInfo.AppendLine("[assembly: AssemblyFileVersion(\"1.0.0.0\")]");
			//// Product Info
			//asmInfo.AppendLine($"[assembly: AssemblyProduct(\"{Config.dllName}\")]");
			//asmInfo.AppendLine("[assembly: AssemblyInformationalVersion(\"1.0.0.0\")]");

			//SyntaxTree VerInfoSyntaxTree = CSharpSyntaxTree.ParseText(asmInfo.ToString());
			//trees.Add(VerInfoSyntaxTree);

			CSharpCompilationOptions defaultCompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).
				WithOptimizationLevel(OptimizationLevel.Debug).
				WithPlatform(Platform.AnyCpu);
			if (System.Diagnostics.Debugger.IsAttached)
			{
				//生成调试信息
				defaultCompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithOptimizationLevel(OptimizationLevel.Debug).WithPlatform(Platform.AnyCpu);
			}

			// 代码
			SyntaxTree tree;
			foreach (string i in m_classString)
			{
				tree = CSharpSyntaxTree.ParseText(i);
				trees.Add(tree);
			}

			// 管理器
			string configClass =
			@"public class ExcelLoaderConfig
			{
				public static string bytesDirectory;
				public static bool m_init;
				public static void Init()
				{
					if (!System.IO.Directory.Exists(bytesDirectory))
					{
						throw new System.Exception(""初始化表数据失败,bytes文件夹不存在"");
					}
					m_init = true;
				}
			}";

			string outputPath = Path.Combine(Config.cmdPath, Config.outputDirectory + "/CS/CSFile");
			File.WriteAllText($"{outputPath}/ExcelLoaderConfig.cs", configClass);

			tree = CSharpSyntaxTree.ParseText(configClass);
			trees.Add(tree);

			// 添加library
			MetadataReference[] references = new MetadataReference[]
			{
					MetadataReference.CreateFromFile(typeof(object).Assembly.Location),//mscorlib.dll
                    //MetadataReference.CreateFromFile(typeof(Uri).Assembly.Location),//System.dll
                    //MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),//System.Core.dll
					//MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Collections.dll")),
					//MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Data.dll")),
					//MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Linq.dll")),
					//MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.XML.dll")),
					//MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.IO.dll")),
					//MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
					//MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Private.CoreLib.dll")),
					//MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "Microsoft.CSharp.dll")),
					//MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
					//MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.Serialization.dll")),
					//MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Dynamic.Runtime.dll")),
					//MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Reflection.dll")),
			};

			CSharpCompilation compilation = CSharpCompilation.Create(Config.dllName, trees, references, defaultCompilationOptions);

			// 编译输出
			using (MemoryStream dllStream = new MemoryStream())
			{
				using (MemoryStream pdbStream = new MemoryStream())
				{
					EmitResult result = compilation.Emit(dllStream, pdbStream);//result表明了执行是否成功
					if (!result.Success)
						Log.Error($"编译dll失败??\n{Config.dllPath}\n{Config.pdbPath}");
					else
					{
						File.WriteAllBytes(Config.dllPath, dllStream.ToArray());
						File.WriteAllBytes(Config.pdbPath, pdbStream.ToArray());

						Log.Info($"{Config.dllPath}\n{Config.pdbPath}");
						Log.Info($"编译dll成功");
					}

					if (result.Diagnostics.Count() > 0)
					{
						Log.Info("编译log:");
						foreach (var i in result.Diagnostics)
						{
							Log.Warrning(i.ToString());
						}
					}
				}
			}

		}
	}
}
