using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

//By UnknownShadow200 (Thank you!!)

public static class XmlExtensions
{
    public static int IntValue(this XAttribute attr) { return int.Parse(attr.Value); }
}

public static class MapReader
{

    public static Map GetMapFromFile(string mapName)
    {
        string mapPath = Path.Combine(Application.streamingAssetsPath, "Maps");
        string filePath = Path.Combine(mapPath, mapName);

        using (StreamReader reader = new StreamReader(filePath))
        {
            return Deserialize(reader);
        }
    }

    public class Map
    {
        public string Version;
        public string Orientation;
        public string RenderOrder;
        public int Width, Height;
        public int TileWidth, TileHeight;
        public int NextObjectID;
        public List<Tileset> Tilesets = new List<Tileset>();
        public List<Layer> Layers = new List<Layer>();
        public List<Entity> Entities = new List<Entity>();
    }

    public class Tileset
    {
        public int numTiles;
        public int FirstGID;
        public string Name;
        public int TileWidth, TileHeight;
        public List<TilesetImage> Images = new List<TilesetImage>();
    }

    public class TilesetImage
    {
        public string Source;
        public string TransparentCol;
        public int Width, Height;
    }

    public class Layer
    {
        public string Name;
        public int Width, Height;
        public int[] Data;
    }

    public class Entity
    {
        public int ID;
        public string Name;
        public int GridID;
        public int X, Y;
        public Dictionary<string, string> Properties = new Dictionary<string, string>();
    }

    static Map Deserialize(TextReader source)
    {
        XDocument doc = XDocument.Load(source);
        // Validate document
        XElement mapEl = doc.Root;
        if (mapEl.Name != "map")
        {
            Debug.Log("XML must have 'map' root node");
            //throw new InvalidDataException("XML must have 'map' root node");
        }
        Map map = new Map();

        // Read attributes of map node
        map.Version = mapEl.Attribute("version").Value;
        map.Orientation = mapEl.Attribute("orientation").Value;
        map.RenderOrder = mapEl.Attribute("renderorder").Value;
        map.Width = mapEl.Attribute("width").IntValue();
        map.Height = mapEl.Attribute("height").IntValue();
        map.TileWidth = mapEl.Attribute("tilewidth").IntValue();
        map.TileHeight = mapEl.Attribute("tileheight").IntValue();
        map.NextObjectID = mapEl.Attribute("nextobjectid").IntValue();

        // Read all tileset child nodes of map node
        foreach (XElement tilesetEl in mapEl.Elements("tileset"))
        {
            Tileset tileset = new Tileset();
            tileset.FirstGID = tilesetEl.Attribute("firstgid").IntValue();
            tileset.Name = tilesetEl.Attribute("name").Value;
            tileset.TileWidth = tilesetEl.Attribute("tilewidth").IntValue();
            tileset.TileHeight = tilesetEl.Attribute("tileheight").IntValue();


            // Read all image child nodes of tileset node
            foreach (XElement imageEl in tilesetEl.Elements("image"))
            {
                TilesetImage image = new TilesetImage();
                image.Source = imageEl.Attribute("source").Value;
 //               image.TransparentCol = imageEl.Attribute("trans").Value;
                image.Width = imageEl.Attribute("width").IntValue();
                image.Height = imageEl.Attribute("height").IntValue();
                tileset.Images.Add(image);
            }
            map.Tilesets.Add(tileset);
        }

        // Read all layer child nodes of map node
        foreach (XElement layerEl in mapEl.Elements("layer"))
        {
            Layer layer = new Layer();
            layer.Name = layerEl.Attribute("name").Value;
            layer.Width = layerEl.Attribute("width").IntValue();
            layer.Height = layerEl.Attribute("height").IntValue();

            // Read data child node of layer node
            XElement dataEl = layerEl.Element("data");
            string encoding = dataEl.Attribute("encoding").Value;
            if (encoding != "csv")
                throw new NotSupportedException("Only CSV encoding of data supported");

            // Convert CSV data value of node into byte array
            string[] csvElements = dataEl.Value.Split(',');
            if (csvElements.Length != (layer.Width * layer.Height))
            {
                Debug.Log("CSV data length doesn't match layer data size");
                //throw new InvalidDataException("CSV data length doesn't match layer data size");
            }
            layer.Data = new int[csvElements.Length];
            for (int i = 0; i < layer.Data.Length; i++)
                layer.Data[i] = int.Parse(csvElements[i]);

            map.Layers.Add(layer);
        }

        // Read all object group child nodes of map node
        foreach (XElement objGroupEl in mapEl.Elements("objectgroup"))
        {
            string objGroupName = objGroupEl.Attribute("name").Value;
            // Skip object group nodes that are not 'Entities' group
            if (objGroupName != "Entities") continue;

            // Read all object child nodes of object group node
            foreach (XElement objEl in objGroupEl.Elements())
            {
                Entity entity = new Entity();
                entity.ID = objEl.Attribute("id").IntValue();
                entity.GridID = objEl.Attribute("gid").IntValue();
                entity.X = objEl.Attribute("x").IntValue();
                entity.Y = objEl.Attribute("y").IntValue();
                // Note: name attribute is optional
                if (objEl.Attribute("name") != null)
                {
                    entity.Name = objEl.Attribute("name").Value;
                }

                // Read all property nodes of object node (if they exist)
                XElement propertiesEl = objEl.Element("properties");
                if (propertiesEl != null)
                {
                    foreach (XElement propEl in propertiesEl.Elements("property"))
                    {
                        string propName = propEl.Attribute("name").Value;
                        string propValue = propEl.Attribute("value").Value;
                        entity.Properties.Add(propName, propValue);
                    }
                }
                map.Entities.Add(entity);
            }
        }
        return map;
    }

}