using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
[ExecuteInEditMode]
public class CircleText : MonoBehaviour {
	public float radius = 45;
	private TextMeshPro m_TextMeshPro;
	private TextContainer m_TextContainer;

	void Awake()
	{
		m_TextMeshPro = GetComponent<TextMeshPro>();
		m_TextContainer = GetComponent <TextContainer>();
	//	m_TextMeshPro.ForceMeshUpdate();
	}
	// Use this for initialization
	void Start () {
		//StartCoroutine(CircleText());
		EncircleText();
	}
	
	void EncircleText()
	{
		m_TextMeshPro.ForceMeshUpdate();
		int characterCount = m_TextMeshPro.textInfo.characterCount;
		float circumference = 2 * Mathf.PI * radius;
		float arcLenToAngle = 360 / circumference;
		for (int i = 0; i < characterCount; i++)
		{
			TMP_CharacterInfo charInfo = m_TextMeshPro.textInfo.characterInfo[i];
			if (!charInfo.isVisible)
				continue;
			int vertexIndex = charInfo.vertexIndex;
			int meshIndex = charInfo.materialReferenceIndex;
			Vector3[] vertices = m_TextMeshPro.textInfo.meshInfo[meshIndex].vertices;
			Vector2 charMidBaseLine = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) * 0.5f, charInfo.bottomRight.y);
			Vector3 offset = charMidBaseLine;
			vertices[vertexIndex + 0] += -offset;
			vertices[vertexIndex + 1] += -offset;
			vertices[vertexIndex + 2] += -offset;
			vertices[vertexIndex + 3] += -offset;

			float arclen = (charMidBaseLine.x - transform.position.x);
			float angle = arclen * arcLenToAngle; 
			float dy = Mathf.Cos(angle * Mathf.Deg2Rad) * -radius;
			float dx = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
			Matrix4x4 matrix = Matrix4x4.TRS(new Vector3(dx, dy, 0), Quaternion.Euler(0, 0, angle), Vector3.one);

			vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
			vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
			vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
			vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);
#if false
			vertices[vertexIndex + 0] += offset;
			vertices[vertexIndex + 1] += offset;
			vertices[vertexIndex + 2] += offset;
			vertices[vertexIndex + 3] += offset;
#endif

			
		}
		m_TextMeshPro.UpdateVertexData();
	}
	// Update is called once per frame
	void Update () {
		//EncircleText();
	}
}
