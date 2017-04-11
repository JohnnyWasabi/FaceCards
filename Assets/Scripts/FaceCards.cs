using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FaceCards : MonoBehaviour {

	public class FaceSprite: System.IComparable
	{
		public FaceSprite(string first, string last, string role, Sprite sprite, Texture2D texture)
		{
			firstName = first;
			lastName = last;
			fullName = first + " " + last;
			this.role = role;
			this.sprite = sprite;
			this.texture = texture;
			RandomizeOrder();
		}
		public string firstName;
		public string lastName;
		public string fullName;
		public string role;
		public Sprite sprite;
		public Texture2D texture;
		int randSortOrder;
		
		public int CompareTo(object obj)
		{
			if (obj == null) return 1;
			FaceSprite faceSprite = obj as FaceSprite;
			if (faceSprite != null)
				return this.randSortOrder.CompareTo(faceSprite.randSortOrder);
			else
				throw new System.ArgumentException("Object is not a FaceSprite");
		}
		public void RandomizeOrder()
		{
			randSortOrder = Random.Range(0, 1000);
		}
	}
	public List<FaceSprite> faceSprites;
	public SpriteRenderer spriteRenderer;
	public GUIText guiTextName;
	public GUIText guiTextNofM;
	public GUIText guiTextBadChar;
	bool doneLoading;
	FaceSprite faceSpriteCrnt = null;
	int iFaceSprite;
	public float widthFace = 256.0f;
	public Color colorCorrect = new Color(0, 1, 0, 1);

	// Use this for initialization
	IEnumerator Start()
	{
		faceSprites = new List<FaceSprite>();
		doneLoading = false;
		yield return StartCoroutine(LoadFaces());
		guiTextName.pixelOffset = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
		guiTextNofM.pixelOffset = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f - 32);
		guiTextBadChar.pixelOffset = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f - 64);
		guiTextBadChar.text = "";
		if (faceSprites.Count > 0)
		{
			faceSprites.Sort();
			iFaceSprite = 0;
			DisplayFaceSprite();
			doneLoading = true;
		}
	}

	void DisplayFaceSprite()
	{
		faceSpriteCrnt = faceSprites[iFaceSprite];
		spriteRenderer.sprite = faceSpriteCrnt.sprite;
		guiTextName.text = "";
		guiTextNofM.text = (iFaceSprite + 1).ToString() + "/" + faceSprites.Count.ToString();
		guiTextBadChar.text = "";
		float scale = widthFace / (float)faceSpriteCrnt.texture.width;
		transform.localScale = new Vector3(scale, scale, 1);
		guiTextName.color = new Color(1, 1, 1, 1);
	}

	void ShowNextFace()
	{
		if (++iFaceSprite >= faceSprites.Count)
		{
			for (int i = 0; i < faceSprites.Count; i++)
				faceSprites[i].RandomizeOrder();

			faceSprites.Sort();
			iFaceSprite = 0;
		}
		DisplayFaceSprite();
	}

	public IEnumerator LoadFaces()
	{
		string path = System.IO.Path.Combine(Application.streamingAssetsPath, "Faces");
		DirectoryInfo dir = new DirectoryInfo(path);
		FileInfo[] info = dir.GetFiles("*");
		foreach (FileInfo f in info)
		{
			if (f.FullName.EndsWith(".png") || f.FullName.EndsWith(".jpg"))
			{
				Debug.Log("file: " + f.FullName);
				yield return LoadSprite(f.FullName);
				guiTextNofM.text = faceSprites.Count.ToString();
			}
		}
	}

	public IEnumerator LoadSprite(string absoluteImagePath)
	{
		string url =  @"file:///" + absoluteImagePath;
		WWW localFile = new WWW(url);

		yield return localFile;

		Texture2D texture = localFile.texture as Texture2D;
		if (texture != null)
		{
			Sprite sprite = Sprite.Create(texture as Texture2D, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0f));
			if (sprite != null)
			{
				string fileName = absoluteImagePath.Substring(LastSlash(absoluteImagePath) + 1);
				string nameCode = fileName.Substring(0, fileName.Length - 4);
				string[] nameParts = nameCode.Split('_');
				if (nameParts.Length == 3)
				{
					faceSprites.Add(new FaceSprite(nameParts[0], nameParts[1], nameParts[2], sprite, texture));
					//spriteRenderer.sprite = sprite;
				}
				else Debug.LogError("Filename missing all three parts separated by underscore. E.g. FirstName_LastName_Role");
			}
		}
	}

	// Returns position of last path separator slash (/ or \). Returns -1 if no slashes.
	public int LastSlash(string str)
	{
		int i = 0;
		for (i = str.Length-1; i >= 0; i--)
		{
			if (str[i] == '/' || str[i] == '\\')
			{
				break;
			}
		}
		return i;
	}


	// Update is called once per frame
	void Update () {
		if (doneLoading)
		{
			if (Input.inputString.Length > 0)
			{
				guiTextBadChar.text = "";
				foreach (char c in Input.inputString)
				{
					if (c == "\b"[0])
					{
						RemoveChar();
					}
					else if (c == "\n"[0] || c == "\r"[0])
					{

					}
					else if (guiTextName.text.Length < faceSpriteCrnt.fullName.Length)
					{
						if (faceSpriteCrnt.fullName[guiTextName.text.Length] == c)
						{
							AddChar();
						}
						else
						{
							guiTextBadChar.text += c.ToString();
						}
					}
				}
			}
			if (Input.GetKeyDown(KeyCode.Return))
			{
				if (guiTextName.text == faceSpriteCrnt.fullName)
				{
					ShowNextFace();
				}
				else
				{
					guiTextName.text = faceSpriteCrnt.fullName;
					guiTextName.color = colorCorrect;
				}
			}
			else if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				AddChar();
			}
			else if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				RemoveChar();
			}
		}
	}

	void RemoveChar()
	{
		if (guiTextName.text.Length != 0)
		{
			guiTextName.text = guiTextName.text.Substring(0, guiTextName.text.Length - 1);
			guiTextName.color = new Color(1, 1, 1, 1);
		}
	}
	void AddChar()
	{
		if (guiTextName.text.Length < faceSpriteCrnt.fullName.Length)
		{
			guiTextName.text += faceSpriteCrnt.fullName[guiTextName.text.Length];
			if (guiTextName.text.Length == faceSpriteCrnt.fullName.Length)
			{
				guiTextName.color = colorCorrect;
			}
		}
	}

	void OnGUI()
	{
		if (doneLoading)
		{
	
		}
	}
}
