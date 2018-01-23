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
	string filePath;
	static Dictionary<string, Vector2> dictSeatIdToPos;

	void Awake()
	{
		dictSeatIdToPos = new Dictionary<string, Vector2>();
		listSpots = new List<NumberedSpot>();

		if (editSpots)
		{
			goSpotCursor = Instantiate(prefabSpot, transform);
			nspotCursor = goSpotCursor.GetComponent<NumberedSpot>();
			nspotCursor.textMesh.text = "";
		}

	}
	public static Vector2 GetSpotPos(string id, float scale)
	{
		Vector2 pos;
		if (dictSeatIdToPos.TryGetValue(id, out pos))
		{
			pos *= scale;
			pos.x += FaceCards.xMapUL;
			pos.y = FaceCards.yMapUL - pos.y;
			return pos;
		}
		return Vector2.zero;
	}
	// Use this for initialization
	void Start () {
		// Read in the spot values
		string filename = "Maps\\Seats.csv";
		filePath = System.IO.Path.Combine(Application.streamingAssetsPath, filename);
		string seatsFile = System.IO.File.ReadAllText(filePath);
		//Debug.Log("Puzzle=" + puzzleFile);
		rows = seatsFile.Split(new string[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);
		for (int iRow = 1; iRow < rows.Length; iRow++)
		{
			string[] cols = rows[iRow].Split(new char[] { ',' }, System.StringSplitOptions.None);
			if (cols.Length >= 3)
			{

				float xPos;
				float yPos;
				float.TryParse(cols[1], out xPos);
				float.TryParse(cols[2], out yPos);
				dictSeatIdToPos.Add(cols[0], new Vector2(xPos, yPos));
				xPos = FaceCards.xMapUL + xPos;
				yPos = FaceCards.yMapUL - yPos;

				if (editSpots)
				{
					GameObject goSpot = Instantiate(prefabSpot, transform);
					NumberedSpot nspot = goSpot.GetComponent<NumberedSpot>();
					listSpots.Add(nspot);
					nspot.textMesh.text = cols[0];
					nspot.id = cols[0];
					nspot.transform.localPosition = new Vector3(xPos, yPos, 0);
				}
			}
		}
		if (editSpots)
			idSpotPlacing = listSpots[iSpotEdit].id;
		enabled = editSpots;
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
				sb.Append(rows[0] + "\n");
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
