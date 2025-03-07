using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 导表工具
{
	public class DirectoryTools
	{
		public static void Copy(string origin, string target)
		{
			if (!Directory.Exists(origin))
			{
				Log.Error($"无法复制,源文件夹不存在{origin}");
				return;
			}
			if (!Directory.Exists(target))
				Directory.CreateDirectory(target);

			string[] files = Directory.GetFiles(origin, "*", SearchOption.AllDirectories);
			foreach (var i in files)
			{
				string targetPath = Path.Combine(target, Path.GetFileName(i));
				try
				{
					File.Copy(i, targetPath, true);
				}
				catch (Exception ex)
				{
					Log.Error($"复制失败:{ex}");
				}
			}
			Log.Info($"复制了{files.Length}个文件");
		}
	}
}
