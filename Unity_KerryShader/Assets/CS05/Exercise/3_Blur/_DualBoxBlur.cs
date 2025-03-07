using UnityEngine;

// ˫�ؾ�ֵģ��
[ExecuteInEditMode]
public class _DualBoxBlur : MonoBehaviour
{
	public Material mat;
	[Range(0, 10)]
	public int m_iteration = 1; // ��������
	[Range(0, 15)]
	public float _BlurOffset = 5;   // uvƫ��
	[Range(1, 10)]
	public float _DownSample = 2;   // ����������

	private void Start()
	{
		if (!mat || !mat.shader || !mat.shader.isSupported)
			enabled = false;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		// 1 / width : ���ǽ���Ļ�ֱ���ת����[0, 1]
		// ����_BlurOffset����uvƫ��

		int width = (int)(source.width / _DownSample);
		int height = (int)(source.height / _DownSample);
		// ������һ�Ż���
		RenderTexture RT1 = RenderTexture.GetTemporary(width, height);
		RenderTexture RT2 = RenderTexture.GetTemporary(width, height);

		Graphics.Blit(source, RT1);

		mat.SetVector("_BlurOffset", new Vector4(_BlurOffset / width, _BlurOffset / height, 0, 0));

		// ��С����
		for (int i = 0; i < m_iteration; i++)
		{
			// ��RT2��Сһ����Ȼ���RT1����RT2��
			width /= 2;
			height /= 2;

			RT2 = RenderTexture.GetTemporary(width, height);
			Graphics.Blit(RT1, RT2, mat, 0);
			RenderTexture.ReleaseTemporary(RT1);

			width /= 2;
			height /= 2;

			RT1 = RenderTexture.GetTemporary(width, height);
			Graphics.Blit(RT2, RT1, mat, 1);
			RenderTexture.ReleaseTemporary(RT2);
		}
		// �Ŵ�
		for (int i = 0; i < m_iteration; i++)
		{
			// ��RT2��Сһ����Ȼ���RT1����RT2��
			width *= 2;
			height *= 2;

			RT2 = RenderTexture.GetTemporary(width, height);
			Graphics.Blit(RT1, RT2, mat, 0);
			RenderTexture.ReleaseTemporary(RT1);

			width *= 2;
			height *= 2;
			RT1 = RenderTexture.GetTemporary(width, height);
			Graphics.Blit(RT2, RT1, mat, 1);
			RenderTexture.ReleaseTemporary(RT2);
		}

		Graphics.Blit(RT1, destination);

		// ���껭��֮����Ҫ�ͷŵ�
		RenderTexture.ReleaseTemporary(RT1);
		RenderTexture.ReleaseTemporary(RT2);
	}
}
