using UnityEngine;

[ExecuteInEditMode]
public class _GaussianBlur : MonoBehaviour
{
	public Material mat;
	[Range(0, 10)]
	public int m_iteration = 1; // 迭代次数
	[Range(0, 15)]
	public float _BlurOffset = 5;   // uv偏移
	[Range(1, 10)]
	public float _DownSample = 2;   // 降采样倍率

	private void Start()
	{
		if (!mat || !mat.shader || !mat.shader.isSupported)
			enabled = false;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		// 1 / width : 就是将屏幕分辨率转换到[0, 1]
		// 乘上_BlurOffset就是uv偏移

		int width = (int)(source.width / _DownSample);
		int height = (int)(source.height / _DownSample);
		// 新申请一张画布
		RenderTexture RT1 = RenderTexture.GetTemporary(width, height);
		RenderTexture RT2 = RenderTexture.GetTemporary(width, height);

		Graphics.Blit(source, RT1);

		mat.SetVector("_BlurOffset", new Vector4(_BlurOffset / width, _BlurOffset / height, 0, 0));
		for (int i = 0; i < m_iteration; i++)
		{
			Graphics.Blit(RT1, RT2, mat, 0);	// 水平方向
			Graphics.Blit(RT2, RT1, mat, 1);	// 垂直方向
		}

		Graphics.Blit(RT1, destination);

		// 用完画布之后，需要释放掉
		RenderTexture.ReleaseTemporary(RT1);
		RenderTexture.ReleaseTemporary(RT2);
	}
}
