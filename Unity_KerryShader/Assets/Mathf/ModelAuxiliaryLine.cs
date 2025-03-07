using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class ModelAuxiliaryLine : MonoBehaviour
{
	// ��ʾ����
	public bool m_showNormal = true;
	public Color m_normalColor = Color.white;
	[Range(0.1f, 10)] public float m_normalLength = 1;

	// ��ʾ�ӽǷ���
	[Space]
	public bool m_showViewLine = true;
	public Color m_viewLineColor = Color.blue;

	// ��ʾ���շ���
	[Space]
	public Light m_lightGo;
	public bool m_showLightDir = true;
	public Color m_lightDirColor = Color.green;

	// NDotL
	[Space]
	public bool m_showNDotL = true;
	public Color m_NDotLColor = Color.yellow;

	void OnDrawGizmos()
	{
		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
		Mesh mesh = meshFilter.mesh;

		List<Vector3> vertices = new List<Vector3>();
		List<Vector3> normals = new List<Vector3>();
		mesh.GetVertices(vertices);
		mesh.GetNormals(normals);

		Vector3 transformPosition = transform.position;
		for (int i = 0; i < vertices.Count; i++)
		{
			// ����
			Vector3 verticeModelPos = vertices[i];
			Vector3 verticeWorldPos = transformPosition + verticeModelPos;

			// ����
			Vector3 normalStartPos = verticeWorldPos;
			Vector3 normalEndPos = verticeWorldPos + normals[i] * m_normalLength;

			// �ӽǷ���
			Vector3 viewStartPos = Camera.main.transform.position;
			Vector3 viewEndPos = verticeWorldPos;

			// ���շ���
			Vector3 lightStartPos = Vector3.zero;
			Vector3 lightEndPos = Vector3.zero;
			if (m_lightGo)
			{
				lightStartPos = Vector3.zero;
				lightEndPos = m_lightGo.transform.rotation * Vector3.forward;
			}

			// NDotL
			if (m_showNormal)
			{
				Color originColor = Gizmos.color;
				Gizmos.color = m_normalColor;
				Gizmos.DrawLine(normalStartPos, normalEndPos);
				Gizmos.color = originColor;
			}

			if (m_showViewLine)
			{
				Color originColor = Gizmos.color;
				Gizmos.color = m_viewLineColor;
				Gizmos.DrawLine(viewStartPos, viewEndPos);
				Gizmos.color = originColor;
			}

			if (m_showLightDir)
			{
				Color originColor = Gizmos.color;
				Gizmos.color = m_lightDirColor;
				Gizmos.DrawLine(verticeWorldPos, verticeWorldPos + (lightEndPos - lightStartPos).normalized);
				Gizmos.color = originColor;
			}

			if (m_showNDotL)
			{
				//Vector3 startPos = verticeWorldPos;
				//Vector3 endPos =
			}
		}
	}
}
