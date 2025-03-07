using System.IO;

namespace Sirenix.OdinInspector.Editor.Modules
{
	public class ModuleDataManager
	{
		public string DataPath;

		public string InstallPath;

		public virtual void SaveData(string id, byte[] data)
		{
			string fileName = DataPath.TrimEnd('/', '\\') + "/" + id + ".data";
			FileInfo fileInfo = new FileInfo(fileName);
			if (!fileInfo.Directory.Exists)
			{
				fileInfo.Directory.Create();
			}
			using FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Create);
			fileStream.Write(data, 0, data.Length);
		}

		public virtual bool HasData(string id)
		{
			string path = DataPath.TrimEnd('/', '\\') + "/" + id + ".data";
			return File.Exists(path);
		}

		public virtual byte[] LoadData(string id)
		{
			string path = DataPath.TrimEnd('/', '\\') + "/" + id + ".data";
			if (!File.Exists(path))
			{
				return null;
			}
			using FileStream fileStream = new FileStream(path, FileMode.Open);
			byte[] array = new byte[fileStream.Length];
			fileStream.Read(array, 0, array.Length);
			return array;
		}
	}
}
