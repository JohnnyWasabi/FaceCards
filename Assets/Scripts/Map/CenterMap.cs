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
			FaceCards.xMapUL =(int)( (-mapData.Layers[0].Width / 2 * pixelTileWidth + pixelHalfTileWidth) * scale);
			FaceCards.yMapUL =(int)( (mapData.Layers[0].Height / 2 * pixelTileHeight - pixelHalfTileHeight) * scale + FaceCards.ControlBarHeight); 

			mapRenderer.goMap.transform.position = new Vector3(FaceCards.xMapUL, FaceCards.yMapUL, 0);
		}
	}
}
