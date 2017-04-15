using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FaceCards : MonoBehaviour {

	public GameObject faceCardPrefab;
	public List<FaceSprite> faceSprites;
	public SpriteRenderer spriteRenderer;
	public GameObject cubeBG;
	public GUIText guiTextName;
	public GUIText guiTextNofM;
	public GUIText guiTextBadChar;
    public GUIText guiTextRole;
	bool doneLoading;
	FaceSprite faceSpriteCrnt = null;
	int iFaceSprite;
	public float faceHeightAsScreenHeightPercent = 0.5f * 0.75f;
	float heightFaceDisplay;
	public Color colorCorrect = new Color(0, 1, 0, 1);
    private GUIStyle guiStyleName = new GUIStyle(); //create a new variable
    float secondsCursorOn = 1f;
    float secondsCursorOff = 1f;
    float secondsBlinkCycle = 0;
    public Color cursorColor = new Color(182f / 255f, 1f, 1f, 1f);

	int TotalNameCharacters;
	float yDelta;

	// For doing key repeating
	float timeKeyDown;
	public float secondsKeyRepeatStartDelay = 0.6f;
	public float secondsKeyRepeatInterval = 0.03333f;
	float keyRepeatDelay;  // initial delay

	// Use this for initialization
	IEnumerator Start()
	{
		YearBook.Init();

		keyRepeatDelay = secondsKeyRepeatStartDelay;
		float yInitial = cubeBG.transform.position.y;
		float yDesired = Camera.main.transform.position.y - Screen.height * 0.5f + cubeBG.transform.localScale.y * 0.5f;
		yDelta = (yDesired - yInitial);
		transform.position = new Vector3(0, transform.position.y + yDelta, 0);
		cubeBG.transform.localScale = new Vector3(Screen.width, cubeBG.transform.localScale.y, 1);
		heightFaceDisplay = Screen.height * faceHeightAsScreenHeightPercent;
		Camera.main.orthographicSize = Screen.height * 0.5f;

        guiStyleName.font = guiTextName.font;
        guiStyleName.fontSize = guiTextName.fontSize;
        guiStyleName.normal.textColor = cursorColor;
        guiTextName.text = "Loading ...";
		guiTextBadChar.text = "";


        UpdateGUITextPositions();

		foreach (Sprite sprite in LoadSprite(System.IO.Path.Combine(Application.streamingAssetsPath, "CardBack.png")))
		{
			FaceSprite.spriteCardBack = sprite;
		}
		foreach (Sprite sprite in LoadSprite(System.IO.Path.Combine(Application.streamingAssetsPath, "CardFrontFrame.png")))
		{
			FaceSprite.spriteCardFrontFrame = sprite;
		}
		foreach (Sprite sprite in LoadSprite(System.IO.Path.Combine(Application.streamingAssetsPath, "CardFrontBG.png")))
		{
			FaceSprite.spriteCardFrontBG = sprite;
		}
		FaceSprite.spriteCardFrontFrame.texture.filterMode = FilterMode.Point;
		//FaceSprite.spriteCardFrontFrame.texture;

		faceSprites = new List<FaceSprite>();
        doneLoading = false;
        yield return StartCoroutine(LoadFaces());
		if (faceSprites.Count > 0)
		{
			Invoke("Randomize", 1.25f);
			Invoke("StartGame", 1.75f);
		}
	}

	void StartGame()
	{
		iFaceSprite = 0;
		DisplayFaceSprite();
		doneLoading = true;
	}
    void UpdateGUITextPositions()
    {
		guiTextName.pixelOffset = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f + transform.position.y);  
		guiTextRole.pixelOffset = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f + transform.position.y - 32); 
		guiTextNofM.pixelOffset = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f + transform.position.y - 64); 
    }
    void DisplayFaceSprite()
	{
		faceSpriteCrnt = faceSprites[iFaceSprite];
		spriteRenderer.sprite = faceSpriteCrnt.sprite;
		guiTextName.text = faceSpriteCrnt.collected ? faceSpriteCrnt.fullName : "";
		guiTextNofM.text = (iFaceSprite + 1).ToString() + "/" + faceSprites.Count.ToString();
		guiTextBadChar.text = "";
		float scale = heightFaceDisplay / (float)faceSpriteCrnt.texture.height; // widthFace / (float)faceSpriteCrnt.texture.width;
		spriteRenderer.transform.localScale = new Vector3(scale, scale, 1);

		if (!faceSpriteCrnt.collected)
		{
			faceSpriteCrnt.countRevealed = 0;
		}
		faceSpriteCrnt.card.MoveTo(transform.position, YearBook.aspectCardVert1 * 256f, 0.5f);
		faceSpriteCrnt.card.FlipShowFront();
		
		guiTextName.color = new Color(1, 1, 1, 1);
        guiTextRole.text = "";
    }

    void ShowNextFace()
	{
		if (++iFaceSprite >= faceSprites.Count)
		{
			iFaceSprite = 0;
		}
		DisplayFaceSprite();
	}

	public void Randomize()
	{
		for (int i = 0; i < faceSprites.Count; i++)
			faceSprites[i].RandomizeOrder();
		faceSprites.Sort();
		for (int i = 0; i < faceSprites.Count; i++)
		{
			faceSprites[i].card.indexOrder = i;
			if (faceSprites[i] == faceSpriteCrnt)
				iFaceSprite = i;
		}

		for (int i = 0; i < faceSprites.Count; i++)
		{
			if (faceSprites[i] != faceSpriteCrnt)
				faceSprites[i].card.ArrangeOnYearbook();
		}
	}
	public void RestartGame()
	{
		Randomize();
		faceSpriteCrnt.card.ArrangeOnYearbook();
		for (int i = 0; i < faceSprites.Count; i++)
		{
			faceSprites[i].card.FlipShowBack();
			faceSprites[i].collected = false;
		}
		Invoke("StartGame", 0.6f);
	}

	void ShowPrevFace()
    {
        if (--iFaceSprite < 0)
        {
            iFaceSprite = faceSprites.Count-1;
        }
        DisplayFaceSprite();
    }
    public IEnumerator LoadFaces()
	{
		string path = System.IO.Path.Combine(Application.streamingAssetsPath, "Faces");
		DirectoryInfo dir = new DirectoryInfo(path);
		FileInfo[] info = dir.GetFiles("*");
		TotalNameCharacters = 0;
		foreach (FileInfo f in info)
		{
			if (f.FullName.EndsWith(".png") || f.FullName.EndsWith(".jpg"))
			{
				//Debug.Log("file: " + f.FullName);
				yield return LoadFaceSprite(f.FullName);
				guiTextNofM.text = faceSprites.Count.ToString();
			}
		}
	}

	public IEnumerator LoadFaceSprite(string absoluteImagePath)
	{
		string url =  @"file:///" + absoluteImagePath;
		WWW localFile = new WWW(url);

		yield return localFile;

		Texture2D texture = localFile.texture as Texture2D;
		if (texture != null)
		{
			Sprite sprite = Sprite.Create(texture as Texture2D, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0f), 1.0f);
			if (sprite != null)
			{
				string fileName = absoluteImagePath.Substring(LastSlash(absoluteImagePath) + 1);
				string nameCode = fileName.Substring(0, fileName.Length - 4);
				string[] nameParts = nameCode.Split('_');
				if (nameParts.Length == 3)
				{
					FaceSprite faceSprite = new FaceSprite(nameParts[0], nameParts[1], nameParts[2], sprite, texture);
					if (faceSprite != null)
					{
						int indexOrder = faceSprites.Count;
						faceSprites.Add(faceSprite);
						TotalNameCharacters += faceSprite.fullName.Length;

						GameObject goFaceCard = Instantiate(faceCardPrefab);
						faceSprite.card = goFaceCard.GetComponent<Card>();
						faceSprite.card.indexOrder = indexOrder;
						faceSprite.card.Init(sprite, FaceSprite.spriteCardBack, FaceSprite.spriteCardFrontBG, FaceSprite.spriteCardFrontFrame, Vector3.zero);
						faceSprite.card.SetHeight(heightFaceDisplay);
						faceSprite.card.SetPos(transform.position);
						faceSprite.card.ArrangeOnYearbook();
						faceSprite.card.FlipShowBack(1.0f);
					}
				}
				else Debug.LogError("Filename missing all three parts separated by underscore. E.g. FirstName_LastName_Role");
			}
		}
	}


	public IEnumerable<Sprite> LoadSprite(string absoluteImagePath)
	{
		Sprite sprite = null;
		string url = @"file:///" + absoluteImagePath;
		WWW localFile = new WWW(url);

		while (!localFile.isDone)
			yield return null;

		//yield return localFile as Sprite;

		Texture2D texture = localFile.texture as Texture2D;
		if (texture != null)
		{
			sprite = Sprite.Create(texture as Texture2D, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0f), 1.0f);
		}
		yield return sprite;
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

	Rect rectText;

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
					if (faceSpriteCrnt.countRevealed > 0)
					{
						faceSpriteCrnt.card.FlipShowBack();
					}
					else
					{
						faceSpriteCrnt.collected = true;
					}
					faceSpriteCrnt.card.ArrangeOnYearbook();
					ShowNextFace();
				}
				else
				{
					faceSpriteCrnt.countRevealed += faceSpriteCrnt.fullName.Length - guiTextName.text.Length;
					guiTextName.text = faceSpriteCrnt.fullName;
					guiTextName.color = colorCorrect;
                    guiTextRole.text = faceSpriteCrnt.role;
                }
            }
			else if (GetKeyRepeatable(KeyCode.RightArrow))
			{
				if (guiTextName.text.Length < faceSpriteCrnt.fullName.Length)
				{
					++faceSpriteCrnt.countRevealed;
					AddChar();
				}
			}
			else if (GetKeyRepeatable(KeyCode.LeftArrow))
			{
				RemoveChar();
			}
            else if (GetKeyRepeatable(KeyCode.PageDown) )
            {
				ReturnFaceToYearbook();
                ShowNextFace();
            }
            else if (GetKeyRepeatable(KeyCode.PageUp))
            {
				ReturnFaceToYearbook();
				ShowPrevFace();
            }
			else if (Input.GetKeyDown(KeyCode.Escape))
			{
				Application.Quit();
			}
		}
	}

	public bool GetKeyRepeatable(KeyCode keyCode)
	{
		bool keyDown = Input.GetKeyDown(keyCode);
		if (keyDown || (Input.GetKey(keyCode) && Time.time - timeKeyDown >= keyRepeatDelay))
		{
			keyRepeatDelay = (keyDown) ? secondsKeyRepeatStartDelay : secondsKeyRepeatInterval;
			timeKeyDown = Time.time;
			return true;
		}
		return false;
	}
	void ReturnFaceToYearbook()
	{
		if (faceSpriteCrnt.countRevealed > 0 || guiTextName.text != faceSpriteCrnt.fullName || !faceSpriteCrnt.collected)
		{
			faceSpriteCrnt.card.FlipShowBack();
		}
		faceSpriteCrnt.card.ArrangeOnYearbook();
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
                guiTextRole.text = faceSpriteCrnt.role;
            }
        }
	}
    /*
     *    ######
     *    |    #
     *    O    #
     *   /|\   #
     *   / \   #
     *      ########
     *   addLmN
     *   
     */
    void OnGUI()
	{
 		if (doneLoading)
		{
			rectText = guiTextName.GetScreenRect(Camera.main);
			if (guiTextName.text.Length == 0)
			{
				rectText.x = guiTextName.pixelOffset.x;
			}
			rectText.y = Screen.height - guiTextName.pixelOffset.y;

			secondsBlinkCycle += Time.deltaTime;
            if (secondsBlinkCycle > secondsCursorOn + secondsCursorOff)
            {
                secondsBlinkCycle -= (secondsCursorOn + secondsCursorOff);
            }
            guiStyleName.normal.textColor = cursorColor;
            string cursorStr = (secondsBlinkCycle > secondsCursorOn) ? " " : "|";
            GUI.Label(new Rect(rectText.x + rectText.width, rectText.y , 16, 32), cursorStr, guiStyleName);
            guiTextBadChar.pixelOffset = new Vector2(rectText.x + rectText.width + 16, Screen.height- rectText.y );

			if (GUI.Button(new Rect(0, Screen.height - 32, 64, 32), "Shuffle"))
			{
				Randomize();
			}
			if (GUI.Button(new Rect(72, Screen.height -32, 64, 32), "Restart"))
			{
				RestartGame();
			}
			if (GUI.Button(new Rect(Screen.width - 64, Screen.height - 32, 64, 32), "Exit"))
			{
				Application.Quit();
			}
		}
    }
}
