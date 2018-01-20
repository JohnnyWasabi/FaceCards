using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRenderer {




	//public class Map
	//{
	//    public string Version;
	//    public string Orientation;
	//    public string RenderOrder;
	//    public int Width, Height;
	//    public int TileWidth, TileHeight;
	//    public int NextObjectID;
	//    public List<Tileset> Tilesets = new List<Tileset>();
	//    public List<Layer> Layers = new List<Layer>();
	//    public List<Entity> Entities = new List<Entity>();
	//}
	//
	//public class Tileset
	//{
	//    public int FirstGridID;
	//    public string Name;
	//    public int TileWidth, TileHeight;
	//    public List<TilesetImage> Images = new List<TilesetImage>();
	//}
	//
	//public class TilesetImage
	//{
	//    public string Source;
	//    public string TransparentCol;
	//    public int Width, Height;
	//}
	//
	//public class Layer
	//{
	//    public string Name;
	//    public int Width, Height;
	//    public int[] Data;
	//}

	public LayerRenderer[] layers;
	public GameObject goMap;

	public static float Game_depthDivision = 1f / 64f; //1 / 64...
	public static float Game_bottomMapLayerDepth = 100;


	public static MapRenderer CreateMapRenderer(MapReader.Map mapData, string mapName, int fringeSize = 0)
	{
		MapRenderer mapRenderer = new MapRenderer();
		mapRenderer.goMap = new GameObject("Map_"+mapName);
		mapRenderer.layers = new LayerRenderer[mapData.Layers.Count];
		for (int i = 0; i < mapData.Layers.Count; i++)
		{
			mapRenderer.layers[i] = MapRenderer.LayerRenderer.CreateRenderedLayerObject(mapRenderer.goMap, mapData, mapData.Layers[i], fringeSize);
		}
		return mapRenderer;
	}

	public class LayerRenderer
	{
		public TileImage[] tileImages;
		public GameObject goLayer;
		MapReader.Layer layer;
		public int width { get; private set; }
		public int height { get; private set; }

		public static LayerRenderer CreateRenderedLayerObject(GameObject mapObject, MapReader.Map mapData, MapReader.Layer layer, int fringeSize = 0)
		{
			LayerRenderer renderedLayer = new LayerRenderer();
			renderedLayer.goLayer = new GameObject("Layer " + layer.Name);
			renderedLayer.goLayer.transform.SetParent(mapObject.transform);
			renderedLayer.tileImages = new TileImage[layer.Data.Length];
			for (int i = 0; i < layer.Data.Length; i++)
			{
				renderedLayer.tileImages[i] = TileImage.CreateTileImage(renderedLayer.goLayer, mapData, layer, i, fringeSize);
			}

			renderedLayer.layer = layer;
			renderedLayer.width = mapData.Width;
			renderedLayer.height = mapData.Height;
			return renderedLayer;
		}

		public void ChangeTile(int row, int col, int tileIDNew)
		{
			tileImages[row*width + col].ChangeTile(tileIDNew);
		}
		public int GetTileID(int row, int col)
		{
			return tileImages[row * width + col].tileID;
		}
		public TileImage GetTileImage(int row, int col)
		{
			return tileImages[row * width + col];
		}

		public void ChangeTileShader()
		{
			Shader shader = Shader.Find("Custom/HSVRangeShader");
			for (int i = 0; i < layer.Data.Length; i++)
			{
				tileImages[i].meshRenderer.material.shader = shader;
				//tileImages[i].meshRenderer.material.SetVector("_HSVAAdjust", new Vector4(color.h, color.s, color.v, 0.0f));
				tileImages[i].meshRenderer.material.SetVector("_HSVAAdjust", new Vector4(Random.Range(0.0f, 1.0f), 0, 0.0f));
				tileImages[i].meshRenderer.material.SetFloat("_HSVRangeMin", 0.8f);
				tileImages[i].meshRenderer.material.SetFloat("_HSVRangeMax", 1.0f);
			}

		}
	}



	public class TileImage : Image
    {
		float width;
		float height;
		MapReader.Tileset tileset;
		public int tileID { get; private set; }
		public int tileIndex { get; private set; }

		public static TileImage CreateTileImage(GameObject parentGameObject, MapReader.Map mapData, MapReader.Layer layer, int tileIndex, int fringeSize = 0)
		{
			int tileID = layer.Data[tileIndex];

			TileImage image = CreateTileImageOfTileID( parentGameObject, mapData, tileID, fringeSize);

			MapReader.Tileset tileset = GetTilesetFromTileID(tileID, mapData.Tilesets);
			image.tileIndex = tileIndex;

			int yTilePos = (tileIndex / mapData.Width) * tileset.TileHeight;
			int xTilePos = (tileIndex % mapData.Width) * tileset.TileWidth;

			Vector3 position = new Vector3(xTilePos, -yTilePos, Game_bottomMapLayerDepth);

			if (layer.Name.Split(',').Length > 1)
			{
				float.TryParse(layer.Name.Split(',')[1], out position.z);
				//position.z += Game_bottomMapLayerDepth;
			}


			//print("position z: " + position.z);

			image.transform.localPosition = new Vector3(position.x, (position.y), (position.y * Game_depthDivision) + position.z);

			return image;
		}


		public static TileImage CreateTileImageOfTileID(GameObject parentGameObject, MapReader.Map mapData, int tileID, int fringeSize = 0)
        {
            MapReader.Tileset tileset = GetTilesetFromTileID(tileID, mapData.Tilesets);
            int imageTileIndex = tileID - tileset.FirstGID;

            int imageWidthInTiles = tileset.Images[0].Width / tileset.TileWidth;
            int imageHeightInTiles = tileset.Images[0].Height / tileset.TileHeight;


            int yTileIndex = imageTileIndex / imageWidthInTiles;
            int xTileIndex = imageTileIndex % imageWidthInTiles;


            //print("TileIndex: " + yTileIndex + " " + xTileIndex);

            Texture2D texture2D = LoadTilePNG(tileset.Images[0].Source);

            //calc tile offset
			
            float u0 = (xTileIndex * tileset.TileWidth) - fringeSize;
            float v0 = (((imageHeightInTiles -1) -yTileIndex) * tileset.TileHeight) - fringeSize;// WHY

			int doubleFringe = fringeSize << 1;
            float u1 = (u0 + tileset.TileWidth) + doubleFringe;
            float v1 = (v0 + tileset.TileHeight) + doubleFringe;

            u0 /= tileset.Images[0].Width;
            v0 /= tileset.Images[0].Height;
            u1 /= tileset.Images[0].Width;
            v1 /= tileset.Images[0].Height;


			//print("tileset.Images[0]: " + tileset.Images[0].Width + " " + tileset.Images[0].Height);
			//print("tileset.TileWidth: " + tileset.TileWidth + " " + tileset.TileHeight);
			//print("UV: " + u0 + " " + v0 + " " + u1 + " " + v1);

			int fullWidth = tileset.TileWidth + doubleFringe;
			int fullHeight = tileset.TileHeight + doubleFringe;
			TileImage image = Image.CreateImage<TileImage>(parentGameObject.transform, texture2D, fullWidth, fullHeight, u0, v0, u1, v1);
			image.width = fullWidth;
			image.height = fullHeight;
			image.tileset = tileset;
			image.tileID = tileID;
            return image;
        }

		public void ChangeTile(int tileID)
		{
			int imageTileIndex = tileID - tileset.FirstGID;
			int imageWidthInTiles = tileset.Images[0].Width / tileset.TileWidth;
			int imageHeightInTiles = tileset.Images[0].Height / tileset.TileHeight;
			int yTileIndex = imageTileIndex / imageWidthInTiles;
			int xTileIndex = imageTileIndex % imageWidthInTiles;

			//calc tile offset

			float u0 = (xTileIndex * tileset.TileWidth);
			float v0 = (((imageHeightInTiles - 1) - yTileIndex) * tileset.TileHeight);// WHY

			float u1 = (u0 + tileset.TileWidth);
			float v1 = (v0 + tileset.TileHeight);

			u0 /= tileset.Images[0].Width;
			v0 /= tileset.Images[0].Height;
			u1 /= tileset.Images[0].Width;
			v1 /= tileset.Images[0].Height;


			Mesh mesh = meshFilter.mesh;
			mesh.uv = new Vector2[]
			{
				new Vector2(u0, v1),
				new Vector2(u1, v1),
				new Vector2(u1, v0),
				new Vector2(u0, v0)
			};

		}

		public static MapReader.Tileset GetTilesetFromTileID(int tileID, List<MapReader.Tileset> tilesets)
        {
            MapReader.Tileset tilesetResult = null;

            foreach (MapReader.Tileset tileset in tilesets)
            {
                if (tileID < tileset.FirstGID)
                    break;
                tilesetResult = tileset;
            }

            return tilesetResult; 
        }


        public static Texture2D LoadTilePNG(string imageName)
        {
            string imagePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Maps");
            imagePath = System.IO.Path.Combine(imagePath, imageName);

            return Image.LoadPNG(imagePath);

        }

    }

}
