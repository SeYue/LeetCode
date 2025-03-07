using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ShowNormalLine : MonoBehaviour
{
	public bool m_isShow = true;

	// Update is called once per frame
	void Update()
	{
		if (m_isShow)
		{
			MeshFilter mf = GetComponent<MeshFilter>();
			if (mf)
			{
				if (mf.sharedMesh)
				{
					foreach (var i in mf.sharedMesh.normals)
					{
						Debug.DrawLine(i, i + Vector3.up);
					}
				}
			}
		}
	}
}
