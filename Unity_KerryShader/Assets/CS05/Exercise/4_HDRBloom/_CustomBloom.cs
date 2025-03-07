using UnityEngine;

// 双重均值模糊
[ExecuteInEditMode]
public class _CustomBloom : MonoBehaviour
{
	public Material mat;

	[Range(0, 10)]
	public float _Threshold = 1;    // 亮度的阈值，超过这个阈值的亮度会做Bloom效果
	[Range(0, 10)]
	public float _Intensity = 1;

	private void Start()
	{
		if (!mat || !mat.shader || !mat.shader.isSupported)
			enabled = false;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		// 1 / width : 就是将屏幕分辨率转换到[0, 1]

		int width = source.width;
		int height = source.height;

		RenderTexture RT1 = RenderTexture.GetTemporary(width / 2, height / 2, 0, source.format);
		RenderTexture RT2 = RenderTexture.GetTemporary(width / 4, height / 4, 0, source.format);
		RenderTexture RT3 = RenderTexture.GetTemporary(width / 8, height / 8, 0, source.format);
		RenderTexture RT4 = RenderTexture.GetTemporary(width / 16, height / 16, 0, source.format);
		RenderTexture RT5 = RenderTexture.GetTemporary(width / 32, height / 32, 0, source.format);
		RenderTexture RT6 = RenderTexture.GetTemporary(width / 64, height / 64, 0, source.format);

		RenderTexture RT5_up = RenderTexture.GetTemporary(width / 32, height / 32, 0, source.format);
		RenderTexture RT4_up = RenderTexture.GetTemporary(width / 16, height / 16, 0, source.format);
		RenderTexture RT3_up = RenderTexture.GetTemporary(width / 8, height / 8, 0, source.format);
		RenderTexture RT2_up = RenderTexture.GetTemporary(width / 4, height / 4, 0, source.format);
		RenderTexture RT1_up = RenderTexture.GetTemporary(width / 2, height / 2, 0, source.format);

		float intensity = Mathf.Exp(_Intensity / 10f * 0.693f) - 1;
		mat.SetFloat("_Intensity", intensity);
		mat.SetFloat("_Threshold", _Threshold);
		Graphics.Blit(source, RT1, mat, 0); // 判断阈值

		// 模糊,降采样
		Graphics.Blit(RT1, RT2, mat, 1);
		Graphics.Blit(RT2, RT3, mat, 1);
		Graphics.Blit(RT3, RT4, mat, 1);
		Graphics.Blit(RT4, RT5, mat, 1);
		Graphics.Blit(RT5, RT6, mat, 1);

		//模糊, 升采样
		mat.SetTexture("_BloomTex", RT5);
		Graphics.Blit(RT6, RT5_up, mat, 2);
		mat.SetTexture("_BloomTex", RT4);
		Graphics.Blit(RT5_up, RT4_up, mat, 2);
		mat.SetTexture("_BloomTex", RT3);
		Graphics.Blit(RT4_up, RT3_up, mat, 2);
		mat.SetTexture("_BloomTex", RT2);
		Graphics.Blit(RT3_up, RT2_up, mat, 2);
		mat.SetTexture("_BloomTex", RT1);
		Graphics.Blit(RT2_up, RT1_up, mat, 2);

		// 合并
		mat.SetTexture("_BloomTex", RT1_up);
		Graphics.Blit(source, destination, mat, 3);

		// 用完画布之后，需要释放掉
		RenderTexture.ReleaseTemporary(RT1);
		RenderTexture.ReleaseTemporary(RT2);
		RenderTexture.ReleaseTemporary(RT3);
		RenderTexture.ReleaseTemporary(RT4);
		RenderTexture.ReleaseTemporary(RT5);
		RenderTexture.ReleaseTemporary(RT6);
		RenderTexture.ReleaseTemporary(RT1_up);
		RenderTexture.ReleaseTemporary(RT2_up);
		RenderTexture.ReleaseTemporary(RT3_up);
		RenderTexture.ReleaseTemporary(RT4_up);
		RenderTexture.ReleaseTemporary(RT5_up);
	}
}
