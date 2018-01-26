using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotManager : MonoBehaviour {
	public GameObject prefabSpot;
	public int iSpotEdit = 0;
	public string idSpotPlacing = "";
	GameObject goSpotCursor;
	NumberedSpot nspotCursor;
	public bool editSpots = false;

	List<NumberedSpot> listSpots;
	string[] rows;
	string headingsSeats;
	string filePath;
	static Dictionary<string, Vector2> dictSeatIdToPos = new Dictionary<string, Vector2>();
	static Dictionary<string, string> dictFaceIdToSeatID = new Dictionary<string, string>();
	static Dictionary<string, string> dictConfig = new Dictionary<string, string>();
	const string FaceIDReplacementToken = "<UNIQUE_ID>";
	static public string FaceIDTemplate = FaceIDReplacementToken; // Defaults to replacement token f none present in config file so that the unique portion from the filename becomes the whole ID.

	static int mapGridUpperLeftX;
	static int mapGridUpperLeftY;
	static int mapGridHeight;
	static int mapGridWidth;
	static int mapGridRows;
	static int mapGridCols;
	static float mapGridCellWidth;
	static float mapGridCellHeight;

	void Awake()
	{
		listSpots = new List<NumberedSpot>();

		if (editSpots)
		{
			goSpotCursor = Instantiate(prefabSpot, transform);
			nspotCursor = goSpotCursor.GetComponent<NumberedSpot>();
			nspotCursor.textMesh.text = "";
		}


		// Read in Config file
		{
			string filenameConfig = "Config.txt";
			string filePathConfig = System.IO.Path.Combine(Application.streamingAssetsPath, filenameConfig);
			string configFile = System.IO.File.ReadAllText(filePathConfig);
			string[] linesConfig = configFile.Split(new string[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);

			for (int iLine = 0; iLine < linesConfig.Length; iLine++)
			{
				if (linesConfig[iLine].StartsWith("//"))
					continue; // ignore comments
				string line = linesConfig[iLine].Trim();
				if (line.Length == 0)
					continue;
				string[] parts = line.Split(new char[] { '=' }, System.StringSplitOptions.None);
				if (parts.Length == 2)
				{
					dictConfig.Add(parts[0].Trim(), parts[1].Trim());
				}
			}
		}
		if (dictConfig.ContainsKey("FaceIDTemplate"))
			FaceIDTemplate = dictConfig["FaceIDTemplate"];

		mapGridUpperLeftX	= GetIntFromDictIfPresent(dictConfig, "MapGridUpperLeftX");
		mapGridUpperLeftY	= GetIntFromDictIfPresent(dictConfig, "MapGridUpperLeftY");
		mapGridHeight		= GetIntFromDictIfPresent(dictConfig, "MapGridHeight", 1);
		mapGridWidth		= GetIntFromDictIfPresent(dictConfig, "MapGridWidth", 1);
		mapGridRows			= GetIntFromDictIfPresent(dictConfig, "MapGridRows", 1);
		mapGridCols			= GetIntFromDictIfPresent(dictConfig, "MapGridCols", 1);
		mapGridCellWidth = (float)mapGridWidth / (float)mapGridCols;
		mapGridCellHeight = (float)mapGridHeight / (float)mapGridRows;												


		string[] seatCSVs = GetConfigValuesArray("Seats", dictConfig);
		string[] seatingCSVs = GetConfigValuesArray("Seating", dictConfig);
		AddHeadingsToDict("SeatsHeadings", dictConfig);
		AddHeadingsToDict("SeatingHeadings", dictConfig);

		// Read in the SeatID to map position mappings
		foreach (string seatFile in seatCSVs)
		{
			filePath = System.IO.Path.Combine(Application.streamingAssetsPath, seatFile);
			headingsSeats = AddKeyValuesFromCSVToDict(filePath, 
				(iRow, dictHeadingsToValues) =>
				{
					float xPos, yPos;
					float.TryParse(dictHeadingsToValues[dictConfig["SeatsHeadings.XPos"]], out xPos);
					float.TryParse(dictHeadingsToValues[dictConfig["SeatsHeadings.YPos"]], out yPos);
					string seatID = dictHeadingsToValues[dictConfig["SeatsHeadings.SeatID"]];
					dictSeatIdToPos[seatID] = new Vector2(xPos, yPos);
					if (editSpots)
					{
						xPos = FaceCards.xMapUL + xPos;
						yPos = FaceCards.yMapUL - yPos;
						GameObject goSpot = Instantiate(prefabSpot, transform);
						NumberedSpot nspot = goSpot.GetComponent<NumberedSpot>();
						listSpots.Add(nspot);
						nspot.textMesh.text = seatID;
						nspot.id = seatID;
						nspot.transform.localPosition = new Vector3(xPos, yPos, 0);
					}
				}
			);
		}
		// Read in the FaceID to SeatID mappings
		foreach (string seatingFile in seatingCSVs)
		{
			AddKeyValuesFromCSVToDict(System.IO.Path.Combine(Application.streamingAssetsPath, seatingFile),
				(iRow, dictHeadingsToValues) =>
				{
					dictFaceIdToSeatID[dictHeadingsToValues[dictConfig["SeatingHeadings.FaceID"]]] = dictHeadingsToValues[dictConfig["SeatingHeadings.SeatID"]];
				}
			);
		}


		if (editSpots)
			idSpotPlacing = listSpots[iSpotEdit].id;
		enabled = editSpots;

	}
	static public int GetIntFromDictIfPresent(Dictionary<string,string> dict, string key, int defaultVal = 0)
	{
		int val = defaultVal;
		if (dict.ContainsKey(key))
		{
			string valStr = dict[key];
			val = int.Parse(valStr);
		}
		return val;
	}
	static public string GetFullFaceID(string uniquePortion)
	{
		return FaceIDTemplate.Replace(FaceIDReplacementToken, uniquePortion);
	}
	public static Vector2 GetSpotPos(string faceId, float scale = 1.0f, bool raw = false)
	{
		string seatId;
		if (dictFaceIdToSeatID.TryGetValue(faceId, out seatId))
		{
			Vector2 pos;
			if (dictSeatIdToPos.TryGetValue(seatId, out pos))
			{
				pos *= scale;
				if (!raw)
				{
					pos.x += FaceCards.xMapUL;
					pos.y = FaceCards.yMapUL - pos.y;
				}
				return pos;
			}
		}
		return Vector2.zero;
	}

	// returns 0-based x (column) and y (row) grid cell index. Upper left grid is 0,0
	public static Vector2 GetGridXY(string faceId)
	{
		Vector2 spotPos = GetSpotPos(faceId, 1.0f, true);
		Vector2 gridPos = new Vector2(
			(spotPos.x - mapGridUpperLeftX) / mapGridCellWidth,
			(spotPos.y - mapGridUpperLeftY) / mapGridCellHeight
		);
		return gridPos;
	}

	// Action takes (int iRow, Dictionary<string, string> dictHeadingToVal), where iRow of 0 is the header.
	// returns headings row string
	static string AddKeyValuesFromCSVToDict(string filePath, Action<int, Dictionary<string, string>> action)
	{
		string csvFile = System.IO.File.ReadAllText(filePath);
		string[] rows = csvFile.Split(new string[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);
		Dictionary<string, string> dictHeadingToVal = new Dictionary<string, string>();
		string[] headings = rows[0].Split(new char[] { ',' }, System.StringSplitOptions.None);

		for (int iRow = 1; iRow < rows.Length; iRow++)
		{
			string[] cols = rows[iRow].Split(new char[] { ',' }, System.StringSplitOptions.None);
			if (cols.Length <= headings.Length)
			{
				for (int c = 0; c < cols.Length; c++)
				{
					dictHeadingToVal[headings[c]] = cols[c];
				}
				action(iRow, dictHeadingToVal);
			}
		}
		return rows[0];
	}

	static string[] GetConfigValuesArray(string key, Dictionary<string,string> dict, char delimiterChar = ',')
	{
		string valuesLine;
		string[] values = new string[] { };
		if (dict.TryGetValue(key, out valuesLine))
		{
			values = valuesLine.Split(new char[] { delimiterChar }, System.StringSplitOptions.None);
			for (int i = 0; i < values.Length; i++)
				values[i] = values[i].Trim();
		}
		return values;
	}

	static void AddHeadingsToDict(string headingsKey, Dictionary<string, string> dict, char delimHeadings = ',', char delimColumLabel = ':')
	{

		string[] colLabels = GetConfigValuesArray(headingsKey, dict, delimHeadings);
		foreach(string colLabel in colLabels)
		{
			string[] parts = colLabel.Split(new char[] { delimColumLabel }, System.StringSplitOptions.None);
			if (parts.Length == 2)
				dict.Add(headingsKey + "." + parts[0].Trim(), parts[1].Trim());
		}
	}

	public float camSpeed = 1f;

	// Update is called once per frame
	void Update () {
		if (editSpots)
		{
			goSpotCursor.transform.position = new Vector3(Camera.main.transform.position.x + Input.mousePosition.x - Screen.width * 0.5f,
					Camera.main.transform.position.y + Input.mousePosition.y - Screen.height * 0.5f, 0);

			if (Input.GetKey(KeyCode.W))
			{
				Camera.main.transform.position += new Vector3(0, camSpeed, 0);
			}
			if (Input.GetKey(KeyCode.A))
			{
				Camera.main.transform.position += new Vector3(-camSpeed, 0, 0);
			}
			if (Input.GetKey(KeyCode.S))
			{
				Camera.main.transform.position += new Vector3(0, -camSpeed, 0);
			}
			if (Input.GetKey(KeyCode.D))
			{
				Camera.main.transform.position += new Vector3(camSpeed, 0, 0);
			}
			if (Input.GetKeyDown(KeyCode.Backspace))
			{
				if (--iSpotEdit < 0)
					iSpotEdit = 0;
				idSpotPlacing = listSpots[iSpotEdit].id;
				Camera.main.transform.position += new Vector3(camSpeed, 0, 0);
			}

			if (Input.GetKeyDown(KeyCode.Space))
			{
				Vector3 mousePos = Input.mousePosition;
				if (0 <= iSpotEdit && iSpotEdit < listSpots.Count)
				{
					Vector3 newPos = new Vector3(Camera.main.transform.position.x + Input.mousePosition.x - Screen.width * 0.5f,
						Camera.main.transform.position.y + Input.mousePosition.y - Screen.height * 0.5f, 0);
					listSpots[iSpotEdit++].transform.position = newPos;
					idSpotPlacing = listSpots[iSpotEdit].id;
				}
			}
			if (Input.GetKeyDown(KeyCode.Return))
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				sb.Append(headingsSeats + "\n");
				foreach (NumberedSpot nspot in listSpots)
				{
					sb.Append(nspot.id); sb.Append(",");
					float xOut = nspot.transform.position.x - FaceCards.xMapUL;
					float yOut = FaceCards.yMapUL - nspot.transform.position.y;
					sb.Append(xOut.ToString()); sb.Append(",");
					sb.Append(yOut.ToString()); sb.Append("\n");
				}
				System.IO.File.WriteAllText(filePath, sb.ToString());
			}
		}
	}
}
