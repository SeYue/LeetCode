using UnityEngine;

[ExecuteInEditMode]
public class _ACESTonemapping : MonoBehaviour
{
	public Material material;

	//public float _Brightness = 1;
	//public float _Saturation = 1;
	//public float _Contrast = 1;
	//public float _vignetteIntensity = 1.5f;
	//public float _VignetteRoundness = 5;
	//public float _VignetteSmoothness = 1;   // 光滑度

	private void Start()
	{
		if (!material || material.shader == null || material.shader.isSupported == false)
		{
			enabled = false;
			return;
		}
	}

	// 相机完成所有渲染之后，才会调用这个函数，这个函数的脚本必须挂在摄像机上
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		//material.SetFloat("_Brightness", _Brightness);
		//material.SetFloat("_Saturation", _Saturation);
		//material.SetFloat("_Contrast", _Contrast);
		//material.SetFloat("_vignetteIntensity", _vignetteIntensity);
		//material.SetFloat("_VignetteRoundness", _VignetteRoundness);
		//material.SetFloat("_VignetteSmoothness", _VignetteSmoothness);
		Graphics.Blit(source, destination, material, 0);
	}
}
