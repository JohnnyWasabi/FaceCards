using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FaceCards : MonoBehaviour {

	public int debugMaxCards = 0;	// greater than 0 the set limit on number of cards.
	public GameObject faceCardPrefab;
	public List<FaceSprite> faceSprites;
	public SpriteRenderer spriteRenderer;
	public GameObject cubeBG;
	public GUIText guiTextName;
	public GUIText guiTextNofM;
	public GUIText guiTextBadChar;
    public GUIText guiTextRole;
	public GameObject goHangMan;
	public GUIText guiTextTheMan;
	public GUIText guiTextGallows;

	const float timeTransitionShowFace = 0.5f;


	float timeGameStarted;
	float timeGameEnded;
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
		guiTextGallows.enabled = false;

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

		YearBook.Init(FaceSprite.spriteCardBack.texture.width, FaceSprite.spriteCardBack.texture.height);
		
		faceSprites = new List<FaceSprite>();
        doneLoading = false;
        yield return StartCoroutine(LoadFaces());
		guiTextName.text = "Type Full Name Here";
		guiTextNofM.text = "";

		if (faceSprites.Count > 0)
		{
			Invoke("Randomize", 1.00f);
			Invoke("StartGame", 1.50f);
		}
		Debug.Log("Num cards Collected = " + FaceSprite.GetNumCollected());
	}

	void StartGame()
	{
		iFaceSprite = faceSprites.Count-1;
		ShowNextFace();//		DisplayFaceSprite();
		doneLoading = true;
		timeGameStarted = Time.time + timeTransitionShowFace;
	}
    void UpdateGUITextPositions()
    {
		guiTextName.pixelOffset = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f + transform.position.y);  
		guiTextRole.pixelOffset = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f + transform.position.y - 32); 
		guiTextNofM.pixelOffset = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f + transform.position.y - 64); 
		guiTextTheMan.pixelOffset = new Vector2(Screen.width * 0.7f, Screen.height * 0.5f + transform.position.y-16);
		guiTextGallows.pixelOffset = guiTextTheMan.pixelOffset;
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
			faceSpriteCrnt.countWrongChars = 0;
		}
		faceSpriteCrnt.card.MoveTo(transform.position, YearBook.aspectCorrectHeight1 * 256f, timeTransitionShowFace);
		faceSpriteCrnt.card.FlipShowFront();
		
		guiTextName.color = new Color(1, 1, 1, 1);
        guiTextRole.text = faceSpriteCrnt.collected ? faceSpriteCrnt.role :  "";

		Invoke("ShowGallows", timeTransitionShowFace);
		
    }

	void ShowGallows()
	{
		guiTextGallows.enabled = true;
	}
    void ShowNextFace()
	{
		++iFaceSprite;
		if (iFaceSprite >= faceSprites.Count)
		{
			iFaceSprite = 0;
		}
		if (!FaceSprite.AreAllCollected())
		{
			while (faceSprites[iFaceSprite].collected)
			{
				if (++iFaceSprite >= faceSprites.Count)
				{
					iFaceSprite = 0;
				}
			}
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
			//if (faceSprites[i] != faceSpriteCrnt || FaceSprite.AreAllCollected())
				faceSprites[i].card.ArrangeOnYearbook();
		}
	}
	public void RestartGame(bool clearCollected = true)
	{
		ReturnFaceToYearbook();
		Randomize();
		//		faceSpriteCrnt.card.ArrangeOnYearbook();
		if (clearCollected)
		{
			for (int i = 0; i < faceSprites.Count; i++)
			{
				faceSprites[i].card.FlipShowBack();
				faceSprites[i].collected = false;
			}
		}
		if (!FaceSprite.AreAllCollected())
			Invoke("StartGame", 0.6f);
		timeGameStarted = Time.time + 10; // Just put it way in the future so display will show 0 until game actually starts (in StartGame()).
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
			if (debugMaxCards > 0 && faceSprites.Count >= debugMaxCards) break;
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
	void Update() {
		if (doneLoading)
		{
            if (Input.inputString.Length > 0 && !FaceSprite.AreAllCollected())
			{
				guiTextBadChar.text = "";
				foreach (char c in Input.inputString)
				{
					if (c == "\b"[0])
					{
						//RemoveChar();
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
							faceSpriteCrnt.countWrongChars++;
						}
					}
				}
			}
			if (Input.GetKeyDown(KeyCode.Return))
			{
				if (guiTextName.text == faceSpriteCrnt.fullName)
				{
					if (IsHangManDead())
					{
						faceSpriteCrnt.card.FlipShowBack();
					}
					else
					{
						faceSpriteCrnt.collected = true;
					}
					faceSpriteCrnt.card.ArrangeOnYearbook();
					if (!FaceSprite.AreAllCollected())
						ShowNextFace();
					else
					{
						guiTextName.text = "";
						guiTextRole.text = "YOU WON!";
						guiTextNofM.text = "";
					}
				}
				else if (!FaceSprite.AreAllCollected())
				{
					faceSpriteCrnt.countRevealed += faceSpriteCrnt.fullName.Length - guiTextName.text.Length;
					guiTextName.text = faceSpriteCrnt.fullName;
					guiTextName.color = Color.yellow;
                    guiTextRole.text = faceSpriteCrnt.role;
                }
            }
			else if (GetKeyRepeatable(KeyCode.RightArrow))
			{
				if (guiTextName.text.Length < faceSpriteCrnt.fullName.Length)
				{
					++faceSpriteCrnt.countRevealed;
					AddChar();
					if (IsHangManDead())
						guiTextName.color = Color.yellow;
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
			else if (Input.GetMouseButtonDown(0))
			{
				int index = YearBook.IndexAtScreenXY((int)Input.mousePosition.x, (int)Input.mousePosition.y);
				if (index >= 0 && index < faceSprites.Count && index != iFaceSprite)
				{
					ReturnFaceToYearbook();
					iFaceSprite = index;
					DisplayFaceSprite();
				}
			}
		}

		if (!FaceSprite.AreAllCollected())
			timeGameEnded = Time.time;

		UpdateHangMan();
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
			if (!FaceSprite.AreAllCollected())
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
	static string[] TheManBody =
   {
		@"" + "\n" +
		@"" + "\n" +
		@"" + "\n" +
		@"" ,

		@"" + "\n" +
		@" ☻" + "\n" +
		@"" + "\n" +
		@"" ,

		@"" + "\n" +
		@" ☻" + "\n" +
		@"  █" + "\n" +
		@"" ,

		@"" + "\n" +
		@" ☻" + "\n" +
		@"/ █" + "\n" +
		@"" ,

		@"" + "\n" +
		@" ☻" + "\n" +
		@"/ █ \" + "\n" +
		@"" ,

		@"" + "\n" +
		@" ☻" + "\n" +
		@"/ █ \" + "\n" +
		@" /" ,

		@"" + "\n" +
		@" ☻" + "\n" +
		@"/ █ \" + "\n" +
		@"  | |" ,

		@"" + "\n" +
		@"  ☻" + "\n" +
		@" |█|" + "\n" +
		@" /  \" ,

		@"" + "\n" +
		@"\☻ /" + "\n" +
		@"  █" + "\n" +
		@" /  \" ,

				@"" + "\n" +
		@"  ☻" + "\n" +
		@" |█|" + "\n" +
		@"  | |" ,
	};

	int indexHangMan;
	float timeTwitchedHangManDeath;
	int countHangManAnim;
	int GetHangManLimbCount()
	{
		//return faceSpriteCrnt.collected ? 0 : (faceSpriteCrnt.countRevealed == 0 ? faceSpriteCrnt.countWrongChars : 6);		// Lose for any reveal
		return faceSpriteCrnt.collected ? 0 : faceSpriteCrnt.countRevealed * 3 + faceSpriteCrnt.countWrongChars; // Reveal cost 3 body parts.
	}
	bool IsHangManDead() { return GetHangManLimbCount() >= 6; }
	void UpdateHangMan()
	{
		indexHangMan = 0;
		if (doneLoading && faceSpriteCrnt != null)
		{
			indexHangMan = GetHangManLimbCount();
			indexHangMan = Mathf.Min(6, indexHangMan);
			if (indexHangMan < 6)
			{
				countHangManAnim = -1;
			}
			else if (countHangManAnim == -1)
			{
				countHangManAnim = 0;
				timeTwitchedHangManDeath = Time.time;
			}
			else 
			{
				if (Time.time - timeTwitchedHangManDeath > 0.1f)
				{
					++countHangManAnim;
					timeTwitchedHangManDeath = Time.time;
				}
				if (countHangManAnim < 6)
					indexHangMan = 6 + (countHangManAnim & 1);
				else if (countHangManAnim < 8)
					indexHangMan = 8;
				else
					indexHangMan = 9;
			}
		}

		guiTextTheMan.text = TheManBody[indexHangMan];
	}

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
			if (!FaceSprite.AreAllCollected())
			{
				GUI.Label(new Rect(rectText.x + rectText.width, rectText.y, 16, 32), cursorStr, guiStyleName);
				guiTextBadChar.pixelOffset = new Vector2(rectText.x + rectText.width + 16, Screen.height - rectText.y);
			}

			if (GUI.Button(new Rect(0, Screen.height - 32, 64, 32), "Shuffle"))
			{
				RestartGame(false);
			}
			if (GUI.Button(new Rect(72, Screen.height -32, 64, 32), "Restart"))
			{
				RestartGame();
			}
			if (GUI.Button(new Rect(Screen.width-64, Screen.height - 32, 64, 32), "Exit"))
			{
				Application.Quit();
			}
			{
				float secondsElapsed = timeGameEnded - timeGameStarted;
				if (secondsElapsed < 0) secondsElapsed = 0;
				string minutes = Mathf.Floor(secondsElapsed / 60).ToString("00");
				string seconds = Mathf.Floor(secondsElapsed % 60).ToString("00");

				GUI.Label(new Rect(/*72+64+8*/ Screen.width*0.25f, Screen.height - 32, 64, 32), minutes + ":" + seconds, guiStyleName);
			}
		}
    }
}
