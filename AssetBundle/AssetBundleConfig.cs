using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
public class AssetBundleConfig
{
	[XmlElement("ABList")] public List<ABBase> ABList { get; set; }
}

[Serializable]
public class ABBase
{
	[XmlAttribute("Path")] public string Path { get; set; }

	[XmlAttribute("Crc")] public uint Crc { get; set; }

	// StreamingAssets文件夹里的ab包名,不带后缀
	[XmlAttribute("ABName")] public string ABName { get; set; }

	// ab包全名带后缀,不是全路径
	[XmlAttribute("AssetName")] public string AssetName { get; set; }

	[XmlAttribute("ABDependce")] public List<string> ABDependce { get; set; }
}