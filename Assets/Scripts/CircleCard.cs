﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class CircleCard : MonoBehaviour
{
	// The more verts, the more 'round' the circle appears  
	// It's hard coded here but it would better if you could pass it in as an argument to this function  
	public int numCircleVerts = 41;
	public float radiusInner = 40;
	public float radiusOuter = 60;
	Mesh mesh;

	// Use this for initialization
	void Awake ()
	{
		if (radiusInner > 0)
			CreateDonutMesh(radiusInner, radiusOuter, numCircleVerts);
		else
			CreateCircleMesh(radiusOuter, numCircleVerts);

		//SetDonutMeshColors(new Color32[] { new Color32(255, 255, 0, 255), new Color32(0, 255, 0, 255), new Color32(0,0,255,255)  });
	}


	// Update is called once per frame
	void Update () {
		
	}

	void CreateCircleMesh(float radius, int numVerts)
	{
		mesh = new Mesh();
		Vector3[] verts = new Vector3[numVerts];
		Vector2[] uvs = new Vector2[numVerts];
		int[] tris = new int[(numVerts * 3)];

		//In the beginning we set up for everything we’ll need later.We get an array of Vector3(3 floats) to use for every point as well as arrays for uv coordinates and triangles.
		// The first vert is in the center of the triangle  
		verts[0] = Vector3.zero;
		uvs[0] = new Vector2(0.5f, 0.5f);
		float angle = 360.0f / (float)(numVerts - 1);


		for (int i = 1; i < numVerts; ++i)
		{
			verts[i] = Quaternion.AngleAxis(angle * (float)(i - 1), Vector3.back) * Vector3.up * radius;
			float normedHorizontal = (verts[i].x + 1.0f) * 0.5f;
			float normedVertical = (verts[i].x + 1.0f) * 0.5f;
			uvs[i] = new Vector2(normedHorizontal, normedVertical);
		}

		for (int i = 0; i + 2 < numVerts; ++i)
		{
			int index = i * 3;
			tris[index + 0] = 0;
			tris[index + 1] = i + 1;
			tris[index + 2] = i + 2;
		}

		// The last triangle has to wrap around to the first vert so we do this last and outside the lop  
		int lastTriangleIndex = tris.Length - 3;
		tris[lastTriangleIndex + 0] = 0;
		tris[lastTriangleIndex + 1] = numVerts - 1;
		tris[lastTriangleIndex + 2] = 1;

		mesh.vertices = verts;
		mesh.SetUVs(0, new List<Vector2>(uvs));
		mesh.SetTriangles(tris, 0);
		mesh.RecalculateNormals();

		MeshFilter meshFilter = GetComponent<MeshFilter>();
		meshFilter.mesh = mesh;
	}
	public void SetDonutMeshColors(Color32[] colorsNew)
	{
		int totalVerts = mesh.vertices.Length;
		Color32[] colors = new Color32[totalVerts];

		int vertsPerColor = totalVerts / colorsNew.Length;

		int indexColor = 0;
		int count = 0 - totalVerts % colorsNew.Length; // add the extra verts that don't divide evenly to the first color by backing up the initial count negative.
		for (int i = 0; i < totalVerts; i++)
		{
			colors[i] = colorsNew[indexColor];
			if (++count >= vertsPerColor)
			{
				count = 0;
				++indexColor;
			}
		}
		mesh.colors32 = colors;
	}
	void CreateDonutMesh(float radiusInner, float radiusOuter, int numOuterVerts)
	{
		mesh = new Mesh();
		int totalVerts = numOuterVerts * 2;
		Vector3[] verts = new Vector3[totalVerts];
		Vector2[] uvs = new Vector2[totalVerts];
		Color32[] colors = new Color32[totalVerts];
		int[] tris = new int[(totalVerts * 3)];

		//In the beginning we set up for everything we’ll need later.We get an array of Vector3(3 floats) to use for every point as well as arrays for uv coordinates and triangles.
		// The first vert is in the center of the triangle  
		float angle = 360.0f / (float)(numOuterVerts);


		for (int i = 0; i < numOuterVerts; ++i)
		{
			Vector3 radial = Quaternion.AngleAxis(angle * (float)(i), Vector3.back) * Vector3.up;
			verts[i * 2] = radial * radiusInner;
			verts[i * 2 + 1] = radial * radiusOuter;
			float normedHorizontal = (verts[i].x + 1.0f) * 0.5f;
			float normedVertical = (verts[i].x + 1.0f) * 0.5f;
			uvs[i*2] = new Vector2(normedHorizontal, normedVertical);
			uvs[i*2+1] = new Vector2(normedHorizontal, normedVertical);
		}

		for (int i = 0; i+1 < numOuterVerts; ++i)
		{
			int index = i * 2 * 3;
			int iv = i * 2;
			tris[index + 0] = iv;
			tris[index + 1] = iv + 1;
			tris[index + 2] = iv + 2;

			tris[index + 3] = iv + 2;
			tris[index + 4] = iv + 1;
			tris[index + 5] = iv + 3;
		}

		byte blue = (byte)Random.Range(0, 256);
		for (int i = 0; i < verts.Length; i++)
		{
			if (i < verts.Length/2)
				colors[i] = new Color32(255, 0, 0, 255);
			else
				colors[i] = new Color32(0, 0, 255, 255);
		}
		// The last triangle has to wrap around to the first vert so we do this last and outside the lop  
		int lastTriangleIndex = tris.Length - 2*3;
		int iVert = verts.Length - 2;
		tris[lastTriangleIndex + 0] = iVert;
		tris[lastTriangleIndex + 1] = iVert + 1;
		tris[lastTriangleIndex + 2] = 0;

		tris[lastTriangleIndex + 3] = 0;
		tris[lastTriangleIndex + 4] = iVert + 1;
		tris[lastTriangleIndex + 5] = 1;

		mesh.vertices = verts;
		mesh.SetUVs(0, new List<Vector2>(uvs));
		mesh.colors32 = colors;
		mesh.SetTriangles(tris, 0);
		mesh.RecalculateNormals();

		MeshFilter meshFilter = GetComponent<MeshFilter>();
		meshFilter.mesh = mesh;
	}

}
