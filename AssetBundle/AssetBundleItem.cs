using UnityEngine;

public class AssetBundleItem
{
	public AssetBundle assetBundle;
	public int RefCount;

	public void Reset()
	{
		assetBundle = null;
		RefCount = 0;
	}
}