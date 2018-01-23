using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterMap : MonoBehaviour {
	MapReader.Map mapData;
	MapRenderer mapRenderer;

	// These are assigned from the actual loaded map data.
	public int pixelTileWidth;
	public int pixelTileHeight;
	public int pixelHalfTileWidth;
	public int pixelHalfTileHeight;

	public void Init(MapReader.Map mapData, MapRenderer mapRenderer)
	{
		this.mapData = mapData;
		this.mapRenderer = mapRenderer;

		pixelTileWidth = mapData.TileWidth;
		pixelHalfTileWidth = pixelTileWidth / 2;
		pixelTileHeight = mapData.TileHeight;
		pixelHalfTileHeight = pixelTileHeight / 2;
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (mapRenderer != null && mapRenderer.goMap.activeSelf)
		{
			float scale = mapRenderer.goMap.transform.localScale.x;
			int mapWidth = mapData.Layers[0].Width * pixelTileWidth;
			int mapHeight = mapData.Layers[0].Height * pixelTileHeight;

			FaceCards.xMapUL =(int)( (-mapWidth / 2) * scale);
			FaceCards.yMapUL = Screen.height/2;// (int)((mapHeight / 2) * scale);// + FaceCards.ControlBarHeight); 

			mapRenderer.goMap.transform.position = new Vector3(FaceCards.xMapUL + pixelHalfTileWidth*scale, FaceCards.yMapUL - pixelHalfTileHeight*scale, 0);
		}
	}
}
