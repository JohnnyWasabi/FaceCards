using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FaceCards : MonoBehaviour {

	const float btnHeight = 24;
	const float btnVSpacing = 4;
	const float btnHeightSpaced = (btnHeight + btnVSpacing);
	const float btnWidth = 64;
	const float btnHSpacing = 4;
	const float btnWidthSpaced = (btnWidth + btnHSpacing);

	public ColorPicker bgColorPicker;	// Background color picker

	List<string> roles;
	int iDeptFilter = 0;    // 0 = all departments included. > 0 means a single department roles[iDeptFilter-1] is included.

	GUIContent[] comboBoxList;
	private ComboBox comboBoxControl;// = new ComboBox();
	private GUIStyle listStyle = new GUIStyle();


	GUIContent[] comboBoxListName;
	private ComboBox comboBoxControlName;// = new ComboBox();

	public int debugMaxCards = 0;	// greater than 0 the set limit on number of cards.
	public GameObject faceCardPrefab;
	public List<FaceSprite> faceSprites;
	public List<FaceSprite> faceSpritesFiltered;
	public GameObject cubeBG;
	public GUIText guiTextName;
	public GUIText guiTextNofM;
	public GUIText guiTextBadChar;
    public GUIText guiTextRole;
	public GameObject goHangMan;
	public GUIText guiTextTheMan;
	public GUIText guiTextGallows;

    int iGuessnamePrevious = 0;

    const float timeTransitionShowFace = 0.5f;

	int typedGood;
	int typedBad;
	int typedGoodWrongCase;
	float timeGameStarted;
	float timeGameEnded;
	bool doneLoading;
	FaceSprite faceSpriteCrnt = null;
	int iFaceSprite;
	public float faceHeightAsScreenHeightPercent = 128; //0.5f * 0.75f;
	public float heightFaceDealStartDisplay = 128;
	public float heightFaceGuessDislplay = 128;
	public Color colorCorrect = new Color(0, 1, 0, 1);
    private GUIStyle guiStyleStats = new GUIStyle(); //create a new variable
	float secondsCursorOn = 1f;
    float secondsCursorOff = 1f;
    float secondsBlinkCycle = 0;
    public Color cursorColor = new Color(182f / 255f, 1f, 1f, 1f);

	private GUIStyle guiStyleVersion = new GUIStyle(); //create a new variable

	float yDelta;

	// For doing key repeating
	float timeKeyDown;
	public float secondsKeyRepeatStartDelay = 0.6f;
	public float secondsKeyRepeatInterval = 0.03333f;
	float keyRepeatDelay;  // initial delay

	bool showAllFaces;
	bool caseSensitive;

	public int widthCardSlot = 74;
	public int heightPaddingCardSlot = 8;

	public int widthYearbookNameLabel = 140;
	public int heightYearBookNameLabel = 32;
	bool isYearBookMode = false;

	// Use this for initialization
	IEnumerator Start()
	{
		roles = new List<string>();
		caseSensitive = true;

		guiTextGallows.enabled = false;

		keyRepeatDelay = secondsKeyRepeatStartDelay;

		guiStyleStats.font = guiTextName.font;
        guiStyleStats.fontSize = guiTextName.fontSize;
        guiStyleStats.normal.textColor = cursorColor;
        guiTextName.text = "Loading ...";
		guiTextBadChar.text = "";

		guiStyleVersion.font = guiTextName.font;
		guiStyleVersion.fontSize = 12;
		guiStyleVersion.normal.textColor = Color.gray;

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

		YearBook.Init(FaceSprite.spriteCardBack.texture.width, FaceSprite.spriteCardBack.texture.height, widthCardSlot, heightPaddingCardSlot);
		
		faceSprites = new List<FaceSprite>();
		faceSpritesFiltered = new List<FaceSprite>();
        doneLoading = false;
        yield return StartCoroutine(LoadFaces());
		SetupRolesComboBox();


		guiTextName.text = "Type Full Name Here";
		guiTextNofM.text = "";

		if (faceSprites.Count > 0)
		{
			Invoke("Randomize", 1.00f);
			Invoke("StartGame", 1.50f);
		}
		//Debug.Log("Num cards Collected = " + FaceSprite.GetNumCollected());
	}

	public bool AreAllCollected() { return FaceSprite.GetNumCollected()  == faceSprites.Count; }

	void SetupRolesComboBox()
	{
		comboBoxList = new GUIContent[roles.Count+1];
		comboBoxList[0] = new GUIContent("All Depts");
		for (int i = 0; i < roles.Count; i++)
		{
			comboBoxList[i + 1] = new GUIContent(roles[i]);
		}

		listStyle.normal.textColor = Color.white;
		listStyle.onHover.background =
		listStyle.hover.background = new Texture2D(2, 2);
		listStyle.padding.left =
		listStyle.padding.right =
		listStyle.padding.top =
		listStyle.padding.bottom = 4;

		comboBoxControl = new ComboBox(new Rect(btnWidthSpaced + btnHSpacing*2, Screen.height - btnHeightSpaced, 100, btnHeight), comboBoxList[0], comboBoxList, "button", "box", listStyle);
	
			
		// What part of name they need to enter
		comboBoxListName = new GUIContent[4];
		comboBoxListName[0] = new GUIContent("First & Last");
		comboBoxListName[1] = new GUIContent("First Name");
		comboBoxListName[2] = new GUIContent("Last Name");
		comboBoxListName[3] = new GUIContent("Department");
		comboBoxControlName = new ComboBox(new Rect(btnWidthSpaced + btnHSpacing * 2 + 100 + btnHSpacing * 2, Screen.height - btnHeightSpaced, 100, btnHeight), comboBoxListName[0], comboBoxListName, "button", "box", listStyle);

	}

	void StartGame()
	{
		iFaceSprite = faceSprites.Count-1;
		ShowNextFace();//		DisplayFaceSprite();
		doneLoading = true;
		timeGameStarted = Time.time + timeTransitionShowFace;
		typedGood = 0;
		typedBad = 0;
		typedGoodWrongCase = 0;
	}

	void UpdateGUITextPositions()
    {
		transform.position = new Vector3(0, Screen.height *-0.5f + 90, 0);
		cubeBG.transform.localScale = new Vector3(Screen.width, cubeBG.transform.localScale.y, 1);
		Camera.main.orthographicSize = Screen.height * 0.5f;

		guiTextName.pixelOffset = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f + transform.position.y);  
		guiTextRole.pixelOffset = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f + transform.position.y - 32); 
		guiTextNofM.pixelOffset = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f + transform.position.y - 64); 
		guiTextTheMan.pixelOffset = new Vector2(Screen.width * 0.64f, Screen.height * 0.5f + transform.position.y-16);
		guiTextGallows.pixelOffset = new Vector2(guiTextTheMan.pixelOffset.x + 2, guiTextTheMan.pixelOffset.y);

		if (comboBoxControl != null)
		{
			const int xCombo = 128;
			comboBoxControl.Reposition(new Rect(xCombo, Screen.height - btnHeightSpaced, 100, btnHeight));
			comboBoxControlName.Reposition(new Rect(xCombo + 100 + btnHSpacing * 2, Screen.height - btnHeightSpaced, 100, btnHeight));
		}

#if false
		bgColorPicker.startPos.x = (Screen.width - bgColorPicker.sizeFull) * 0.5f;
		bgColorPicker.startPos.y = Screen.height * 0.5f - transform.position.y - bgColorPicker.sizeFull; // - ColorPicker.alphaGradientHeight;
#else
		bgColorPicker.originPos.x = (Screen.width) * 0.5f;
		bgColorPicker.originPos.y = Screen.height * 0.5f - transform.position.y -1; // - ColorPicker.alphaGradientHeight;
#endif


	}
	void DisplayFaceSprite()
	{
		faceSpriteCrnt = faceSprites[iFaceSprite];
		guiTextName.text = (faceSpriteCrnt.collected || showAllFaces) ? faceSpriteCrnt.guessName : "";
		guiTextNofM.text = FaceSprite.GetNumCollected() + "/" + faceSprites.Count.ToString();
		guiTextBadChar.text = "";

		if (!faceSpriteCrnt.collected)
		{
			faceSpriteCrnt.countRevealed = 0;
			faceSpriteCrnt.countWrongChars = 0;
		}
		faceSpriteCrnt.card.MoveTo(transform.position, YearBook.aspectCorrectHeight1 * heightFaceGuessDislplay, timeTransitionShowFace);
		faceSpriteCrnt.card.FlipShowFront();
		
		guiTextName.color = new Color(1, 1, 1, 1);
        //guiTextRole.text = (faceSpriteCrnt.collected || showAllFaces) ? (FaceSprite.iGuessNameIndex == 3 ? faceSpriteCrnt.fullName : faceSpriteCrnt.role) :  "";
        guiTextRole.text = (FaceSprite.iGuessNameIndex == 3 ? faceSpriteCrnt.fullName : faceSpriteCrnt.role);

        //Invoke("ShowGallows", timeTransitionShowFace);

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
		if (!AreAllCollected())
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
			faceSprites[i].card.ArrangeOnYearbook();
		}
	}
	public void RestartGame(bool clearCollected = true)
	{
		guiTextBadChar.text = "";
		guiTextName.text = "";
		guiTextNofM.text = "";
		ReturnFaceToYearbook(faceSpriteCrnt);
		//		faceSpriteCrnt.card.ArrangeOnYearbook();
		if (clearCollected)
		{
			Randomize();
			showAllFaces = false;
			for (int i = 0; i < faceSprites.Count; i++)
			{
				faceSprites[i].card.FlipShowBack();
				faceSprites[i].collected = false;
				faceSprites[i].card.uiTextName.gameObject.SetActive(false);
			}
		}
		if (!AreAllCollected())
		{
			if (clearCollected)
			{ 
				Invoke("StartGame", 0.6f);
				timeGameStarted = Time.time + 10; // Just put it way in the future so display will show 0 until game actually starts (in StartGame()).
			}
			else
			{
				iFaceSprite = faceSprites.Count - 1;
				ShowNextFace();
			}
		}
	}

	void FilterByDepartment(int ifilter)
	{
		iDeptFilter = ifilter;
		if (iDeptFilter > 0)
		{
			// Remove excluded faces from the active list.
			for (int i = faceSprites.Count - 1; i >= 0; i--)
			{
				FaceSprite fs = faceSprites[i];
				if (roles[iDeptFilter-1] != fs.role)
				{
					faceSprites.Remove(fs);
					faceSpritesFiltered.Add(fs);
					fs.collected = false;
					fs.card.MoveTo(transform.position, Vector2.zero, 0.5f);
				}
			}
		}
		// Bring back ones from inactive list if match current dept selected.
		for (int i = faceSpritesFiltered.Count - 1; i >= 0; i--)
		{
			FaceSprite fs = faceSpritesFiltered[i];
			if (iDeptFilter == 0 || roles[iDeptFilter-1] == fs.role)
			{
				faceSpritesFiltered.Remove(fs);
				faceSprites.Add(fs);
			}
		}

		// Recalc order indices for active list and move all faces to proper place in yearbook layout
		for (int i = 0; i <  faceSprites.Count; i++)
		{
			faceSprites[i].card.indexOrder = i; ;
			ReturnFaceToYearbook(faceSprites[i]);
		}
		
		iFaceSprite = faceSprites.Count - 1;
		ShowNextFace();
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
						if (!roles.Contains(nameParts[2]))
							roles.Add(nameParts[2]);

						GameObject goFaceCard = Instantiate(faceCardPrefab);
						faceSprite.card = goFaceCard.GetComponent<Card>();
						faceSprite.card.indexOrder = indexOrder;
						faceSprite.card.Init(sprite, FaceSprite.spriteCardBack, FaceSprite.spriteCardFrontBG, FaceSprite.spriteCardFrontFrame, Vector3.zero);
						faceSprite.card.SetHeight(heightFaceDealStartDisplay);
						faceSprite.card.SetPos(transform.position);
						faceSprite.card.ArrangeOnYearbook();
						faceSprite.card.FlipShowBack(1.0f);
						faceSprite.card.uiTextName.gameObject.SetActive(false);

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
	float oldScreenWidth = 0;
	float oldScreenHeight = 0;
	bool needScreenLayoutUpdate = false;
	// Update is called once per frame
	void Update() {
		if (doneLoading)
		{
			if (!bgColorPicker.IsActive())
			{
				if (Input.inputString.Length > 0 && !AreAllCollected())
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
						else if (guiTextName.text.Length < faceSpriteCrnt.guessName.Length)
						{
							char nextNameChar = faceSpriteCrnt.guessName[guiTextName.text.Length];
							if (nextNameChar == c)
							{
								AddChar();
								++typedGood;
							}
							else if (!caseSensitive && (nextNameChar == char.ToUpper(c) || nextNameChar == char.ToLower(c)))
							{
								AddChar();
								guiTextBadChar.text += c.ToString();
								++typedGoodWrongCase;
							}
							else
							{
								guiTextBadChar.text += c.ToString();
								faceSpriteCrnt.countWrongChars++;
								++typedBad;
								guiTextName.color = IsHangManDead() ? Color.yellow : Color.white;
							}
						}
					}
				}
				if (Input.GetKeyDown(KeyCode.Return))
				{
					if (guiTextName.text == faceSpriteCrnt.guessName)
					{
						if (IsHangManDead() || showAllFaces)
						{
							//faceSpriteCrnt.card.FlipShowBack();

						}
						else
						{
							faceSpriteCrnt.collected = true;
							guiTextNofM.text = FaceSprite.GetNumCollected() + "/" + faceSprites.Count.ToString();
						}
						//faceSpriteCrnt.card.ArrangeOnYearbook();
						ReturnFaceToYearbook(faceSpriteCrnt);
						if (!AreAllCollected())
							ShowNextFace();
						else
						{
							guiTextName.text = "";
							guiTextRole.text = "YOU WON!";
							//guiTextNofM.text = "";
						}
					}
					else if (!AreAllCollected())
					{
						faceSpriteCrnt.countRevealed += faceSpriteCrnt.guessName.Length - guiTextName.text.Length;
						guiTextName.text = faceSpriteCrnt.guessName;
						guiTextName.color = IsHangManDead() ? Color.yellow : colorCorrect;
						guiTextRole.text = (FaceSprite.iGuessNameIndex == 3 ? faceSpriteCrnt.fullName : faceSpriteCrnt.role);
					}
				}
				else if (GetKeyRepeatable(KeyCode.RightArrow))
				{
					if (guiTextName.text.Length < faceSpriteCrnt.guessName.Length)
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
				else if (GetKeyRepeatable(KeyCode.PageDown))
				{
					ReturnFaceToYearbook(faceSpriteCrnt);
					ShowNextFace();
				}
				else if (GetKeyRepeatable(KeyCode.PageUp))
				{
					ReturnFaceToYearbook(faceSpriteCrnt);
					ShowPrevFace();
				}
				else if (Input.GetKeyDown(KeyCode.Escape))
				{
					Application.Quit();
				}
				else if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
				{
					// First check if clicked on Big version of pic
					Vector2 displayFaceSize = YearBook.aspectCorrectHeight1 * heightFaceGuessDislplay;
					float xMouseWorld = Input.mousePosition.x - Screen.width * 0.5f;
					float yMouseWorld = Input.mousePosition.y - Screen.height * 0.5f;
					bool clickedDisplayFace = (transform.position.x == faceSpriteCrnt.card.transform.position.x && transform.position.y == faceSpriteCrnt.card.transform.position.y)
						&& (xMouseWorld >= transform.position.x - displayFaceSize.x * 0.5f
							&& xMouseWorld <= transform.position.x + displayFaceSize.x * 0.5f
							&& yMouseWorld <= transform.position.y + displayFaceSize.y
							&& yMouseWorld >= transform.position.y
						);
					//Debug.Log("MouseWorld=" + xMouseWorld + ", " + yMouseWorld + "HitDisplayPic=" + clickedDisplayFace);
					int index = YearBook.IndexAtScreenXY((int)Input.mousePosition.x, (int)Input.mousePosition.y);
					if (clickedDisplayFace)
					{
						if (Input.GetMouseButtonDown(0))
							ReturnFaceToYearbook(faceSpriteCrnt);
					}
					else if (index >= 0 && index < faceSprites.Count && (index != iFaceSprite || (faceSpriteCrnt.card.transform.position.x != transform.position.x || faceSpriteCrnt.card.transform.position.y != transform.position.y))) // && index != iFaceSprite)
					{
						if (Input.GetMouseButtonDown(0))
						{
							ReturnFaceToYearbook(faceSpriteCrnt);
							iFaceSprite = index;
							DisplayFaceSprite();
						}
					}
					else if (yMouseWorld > transform.position.y)
					{
						if (Input.GetMouseButtonDown(1))
							bgColorPicker.Show();
					}
				}
				else if (Input.GetMouseButtonDown(1))
				{

				}
			}
			if (Screen.width != oldScreenWidth || Screen.height != oldScreenHeight)
			{
				needScreenLayoutUpdate = true;
				oldScreenWidth = Screen.width;
				oldScreenHeight = Screen.height;
			}

			UpdateGUITextPositions();
			if (needScreenLayoutUpdate)
			{
				if (!(Input.mousePosition.x == 0 || Input.mousePosition.y == 0 || Input.mousePosition.x == Screen.width-1 || Input.mousePosition.y == Screen.height-1))
				{
					//Mouse is inside the screen by 1 pixel or more, so can't be outside of window, therefore is over the game screen
					YearBook.Init(FaceSprite.spriteCardBack.texture.width, FaceSprite.spriteCardBack.texture.height, widthCardSlot, heightPaddingCardSlot); // make it update it's Screen-size based values.
					if (!isYearBookMode)
					{
						foreach (FaceSprite fs in faceSprites)
						{
							fs.card.ArrangeOnYearbook(0.25f);
						}
						//DisplayFaceSprite();
						faceSpriteCrnt.card.MoveTo(transform.position, YearBook.aspectCorrectHeight1 * heightFaceGuessDislplay, 0.25f);// timeTransitionShowFace);
					}
					else
					{
						ChangeYearBookMode(isYearBookMode);
					}
					needScreenLayoutUpdate = false;
				}
			}
		}

		if (!AreAllCollected())
			timeGameEnded = Time.time;

		UpdateHangMan();
	}


	void ChangeYearBookMode(bool isYearBook)
	{
		isYearBookMode = isYearBook;
		if (isYearBookMode)
		{
			YearBook.Init(FaceSprite.spriteCardBack.texture.width, FaceSprite.spriteCardBack.texture.height, widthYearbookNameLabel, heightYearBookNameLabel);
			foreach (FaceSprite fs in faceSprites)
			{
				fs.card.uiTextName.gameObject.SetActive(true);
				fs.card.uiTextName.text = fs.fullName + "\n" + fs.role;
				fs.collected = true;
				fs.card.FlipShowFront();
			}
			SortByGuessName();
			showAllFaces = true;
			RestartGame(false);
		}
		else
		{
			YearBook.Init(FaceSprite.spriteCardBack.texture.width, FaceSprite.spriteCardBack.texture.height, widthCardSlot, heightPaddingCardSlot);
			showAllFaces = false;
			RestartGame();
		}
	}
	public void SortByGuessName()
	{
		faceSprites.Sort(delegate (FaceSprite fs1, FaceSprite fs2)
		{
			switch (FaceSprite.iGuessNameIndex)
			{
			case 0: // Full Name
            case 1: // First Name
                return string.Compare(fs1.fullName, fs2.fullName);
			case 2: // Last Name
				return string.Compare(fs1.lastName+fs1.firstName, fs2.lastName+fs2.firstName);
			case 3: // Department 
                    // Sort secondarily by the previous sorting criteria.
                    switch(iGuessnamePrevious)
                    {
                        case 0: // Full Name
                        case 1: // First Name
                            return string.Compare(fs1.role + fs1.fullName, fs2.role + fs2.fullName);
                        case 2: // Last Name
                            return string.Compare(fs1.role + fs1.lastName + fs1.firstName, fs2.role + fs2.lastName + fs2.firstName);
                    }
                    return string.Compare(fs1.role, fs2.role); // This should actually never get reached.
            }
            return (fs1.card.indexOrder - fs2.card.indexOrder); // This should actually never get reached.
		});
		for (int i = 0; i < faceSprites.Count; i++)
		{
			faceSprites[i].card.indexOrder = i;
			if (faceSprites[i] == faceSpriteCrnt)
				iFaceSprite = i;
			faceSprites[i].card.ArrangeOnYearbook();
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
	void ReturnFaceToYearbook(FaceSprite fs)
	{
        guiTextName.text = "";
        guiTextRole.text = "";
		//if (fs.countRevealed > 0 || guiTextName.text != fs.guessName || !fs.collected)
		//if (!fs.collected)
		{
			if (!fs.collected && !showAllFaces) //!AreAllCollected() && !showAllFaces)
				fs.card.FlipShowBack();
			else
				fs.card.FlipShowFront();
		}
		fs.card.ArrangeOnYearbook();
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
		if (guiTextName.text.Length < faceSpriteCrnt.guessName.Length)
		{
			guiTextName.text += faceSpriteCrnt.guessName[guiTextName.text.Length];
			if (guiTextName.text.Length == faceSpriteCrnt.guessName.Length)
			{
				guiTextName.color = IsHangManDead() ? Color.yellow : colorCorrect;
                guiTextRole.text = (FaceSprite.iGuessNameIndex == 3 ? faceSpriteCrnt.fullName : faceSpriteCrnt.role);
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

	const int iHangmanDead = 7;
	int indexHangMan;
	float timeTwitchedHangManDeath;
	int countHangManAnim;
	int GetHangManLimbCount()
	{
		//return faceSpriteCrnt.collected ? 0 : (faceSpriteCrnt.countRevealed == 0 ? faceSpriteCrnt.countWrongChars : iHangmanDead);		// Lose for any reveal
		return faceSpriteCrnt.collected ? 0 : faceSpriteCrnt.countRevealed * 3 + faceSpriteCrnt.countWrongChars; // Reveal cost 3 body parts.
	}
	bool IsHangManDead() { return GetHangManLimbCount() >= iHangmanDead; }
	void UpdateHangMan()
	{
		indexHangMan = 0;
		if (doneLoading && faceSpriteCrnt != null)
		{
			indexHangMan = GetHangManLimbCount();
			indexHangMan = Mathf.Min(iHangmanDead, indexHangMan);
			if (indexHangMan < iHangmanDead)
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
				if (countHangManAnim < iHangmanDead)
					indexHangMan = iHangmanDead + (countHangManAnim & 1);
				else if (countHangManAnim < iHangmanDead+2)
					indexHangMan = iHangmanDead+2;
				else
					indexHangMan = iHangmanDead+3;
			}
		}

		guiTextTheMan.text = TheManBody[indexHangMan];
		guiTextGallows.enabled = (indexHangMan > 0);
	}

    void OnGUI()
	{
 		if (doneLoading)
		{

			// Department Selector
			int selectedItemIndex = comboBoxControl.Show();
			if (selectedItemIndex != iDeptFilter)
			{
				FilterByDepartment(selectedItemIndex);
				SortByGuessName();
			}

			// Name part to Guess selector
			selectedItemIndex = comboBoxControlName.Show();
			if (selectedItemIndex != FaceSprite.iGuessNameIndex)
			{
                iGuessnamePrevious = FaceSprite.iGuessNameIndex;
				FaceSprite.iGuessNameIndex = selectedItemIndex;
                if (!showAllFaces)
                {
                    Randomize();
                    RestartGame();
                }
                else
                {
                    ReturnFaceToYearbook(faceSpriteCrnt);
                    guiTextName.text = "";
                    guiTextRole.text = "";
                    if (!AreAllCollected())
                        DisplayFaceSprite();
                }
                if (showAllFaces || isYearBookMode)
				    SortByGuessName();
			}

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
            guiStyleStats.normal.textColor = cursorColor;
            string cursorStr = (secondsBlinkCycle > secondsCursorOn) ? " " : "|";
			if (!AreAllCollected() && timeGameStarted <= Time.time)
			{
				GUI.Label(new Rect(rectText.x + rectText.width, rectText.y, 16, 32), cursorStr, guiStyleStats);
				guiTextBadChar.pixelOffset = new Vector2(rectText.x + rectText.width + 16, Screen.height - rectText.y);
			}

			const float caseBtnWidth = 110;
			caseSensitive = GUI.Toggle(new Rect(btnHSpacing, Screen.height - btnHeightSpaced * 1, caseBtnWidth, btnHeight), caseSensitive, "Case-sensitive");

			bool yearBookModeNew = GUI.Toggle(new Rect(btnHSpacing, Screen.height - btnHeightSpaced * 2, caseBtnWidth, btnHeight), isYearBookMode, "Yearbook");
			if (yearBookModeNew != isYearBookMode)
				ChangeYearBookMode(yearBookModeNew);


			bool showAllFacesBefore = showAllFaces;
			showAllFaces = GUI.Toggle(new Rect(btnHSpacing, Screen.height - btnHeightSpaced*3, 80, btnHeight), showAllFaces, "Show All");
			if (showAllFaces != showAllFacesBefore)
			{
				if (showAllFaces)
				{
					ReturnFaceToYearbook(faceSpriteCrnt);
					foreach (FaceSprite facesprite in faceSprites)
						facesprite.card.FlipShowFront();
					DisplayFaceSprite();
				}
				else
				{
					guiTextName.text = "";
					guiTextRole.text = "";
					foreach (FaceSprite facesprite in faceSprites)
						if (!facesprite.collected && iFaceSprite != facesprite.card.indexOrder)
							facesprite.card.FlipShowBack();
				}

			}

			if (GUI.Button(new Rect(Screen.width - btnWidthSpaced, Screen.height - btnHeightSpaced * 3, 64, btnHeight), "Shuffle"))
			{
				Randomize();
				RestartGame(false);
			}
			if (GUI.Button(new Rect(Screen.width - btnWidthSpaced, Screen.height - btnHeightSpaced * 2, 64, btnHeight), "Restart"))
			{
				RestartGame();
			}

			if (GUI.Button(new Rect(Screen.width - btnWidthSpaced, Screen.height - btnHeightSpaced, btnWidth, btnHeight), "Exit"))
			{
				Application.Quit();
			}

			// Accuracy
			if (timeGameStarted <= Time.time)
			{
				int typedTotal = typedGood + typedBad + typedGoodWrongCase;
				//if (typedTotal > 0)
				{
					float accuracy = (float)typedGood / (float)typedTotal * 100.0f;
					string accuracyPercent = (typedGood == typedTotal) ? "100" : accuracy.ToString("00");
					if (typedTotal == 0)
						accuracyPercent = "__";
					GUI.Label(new Rect(Screen.width * 0.5f + 128, Screen.height - 32, 64, 32),  accuracyPercent + "%", guiStyleStats);
				}
			}

			// Timer
			if (timeGameStarted <= Time.time)
			{
				float secondsElapsed = timeGameEnded - timeGameStarted;
				if (secondsElapsed < 0) secondsElapsed = 0;
				string minutes = Mathf.Floor(secondsElapsed / 60).ToString("00");
				string seconds = Mathf.Floor(secondsElapsed % 60).ToString("00");

				GUI.Label(new Rect(Screen.width * 0.5f - 128 - 58, Screen.height - 32, 64, 32), minutes + ":" + seconds, guiStyleStats);
			}

			// Version
			GUI.Label(new Rect(Screen.width-40 - btnWidthSpaced, Screen.height-16, 48, 16), "V 1.4", guiStyleVersion);
		}
    }
}
