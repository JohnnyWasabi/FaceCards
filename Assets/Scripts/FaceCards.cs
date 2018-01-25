using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FaceCards : StateMachine {

	const float btnHeight = 24;
	const float btnVSpacing = 4;
	const float btnHeightSpaced = (btnHeight + btnVSpacing);
	const float btnWidth = 64;
	const float btnHSpacing = 4;
	const float btnWidthSpaced = (btnWidth + btnHSpacing);
    const float comboButtonWidth = 120;
	const float comboSpacing = comboButtonWidth + btnHSpacing * 2;
	const float xStartComboBoxes = btnHSpacing;
	const string msgVictory = "YOU WON!";

	public ColorPicker bgColorPicker;	// Background color picker

	List<string> roles;
	int iDeptFilter = 0;    // 0 = all departments included. > 0 means a single department roles[iDeptFilter-1] is included.
	int countTenureMembers = 1;  // 1..TotalFaces; Minimum number of people to include in filter. This determines the date to use (to get that many people) and in turn determines the actual number shown (because some people were hired on the same date).
	int _countDeptTotal = 0;     // How many people are included by current Department filter setting.
	int countDeptTotal { get { return _countDeptTotal; } set { _countDeptTotal = value; countTenureMembers = (int)Mathf.Clamp(countDeptTotal * valTenureSlider, 1, countDeptTotal); } }     // How many people are included by current Department filter setting.

	public enum GameMode
	{
		memoryGame,
		flashCards,
		yearBook,
		map
	}
	GameMode gameMode = GameMode.memoryGame;
	int iTenureFilter = 0;
	float _valTenureSlider = 0;
	float valTenureSlider { get { return _valTenureSlider; } set { _valTenureSlider = value; countTenureMembers = (int)Mathf.Clamp(countDeptTotal * valTenureSlider, 1, countDeptTotal); } }	  // 0.0 .. 1.0  slider value that determines countTenureMembers.
	float valTenureSliderLastRelease = 0;   // Keeps track of valTenureSlider on mouse Up so we only update the game when the player releases the slider.

	float _valMapScaleSlider = 1;
	float valMapScaleSlider {  get { return _valMapScaleSlider; } set { _valMapScaleSlider = value;  } }
	float valMapScaleSliderLastRelease = 0;   // Keeps track of valTenureSlider on mouse Up so we only update the game when the player releases the slider.


	float score;

	// Departments
	GUIContent[] comboBoxList;
	private ComboBox comboBoxControl;// = new ComboBox();
	private GUIStyle listStyle = new GUIStyle();

	// Guess Name
	GUIContent[] comboBoxListNameGuess;
	GUIContent[] comboBoxListNameSort;
	private ComboBox comboBoxControlName;// = new ComboBox();

	// Game Mode
	GUIContent[] comboBoxListMode;
	private ComboBox comboBoxControlMode;// = new ComboBox();

	// Tenure Filter mode:
	GUIContent[] comboBoxListTenure;
	private ComboBox comboBoxControlTenure;

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
	public float heightFaceDealStartDisplay = 144;
	public float heightFaceGuessDislplay = 144;
	public Color colorCorrect = new Color(0, 1, 0, 1);
	private GUIStyle guiStyleStats = new GUIStyle(); //create a new variable
	private GUIStyle guiStyleScore = new GUIStyle(); //create a new variable
	float secondsCursorOn = 1f;
    float secondsCursorOff = 1f;
    float secondsBlinkCycle = 0;
    public Color cursorColor = new Color(182f / 255f, 1f, 1f, 1f);

	private GUIStyle guiStyleVersion = new GUIStyle(); //create a new variable

	// For doing key repeating
	float timeKeyDown;
	public float secondsKeyRepeatStartDelay = 0.6f;
	public float secondsKeyRepeatInterval = 0.03333f;
	float keyRepeatDelay;  // initial delay

	bool isFlashcardsMode {  get { return gameMode == GameMode.flashCards; } }
	bool isYearBookMode { get { return gameMode == GameMode.yearBook; } }
	bool caseSensitive;

	public int widthCardSlot = 74;
	public int heightPaddingCardSlot = 8;

	public int widthYearbookNameLabel = 140;
	public int heightYearBookNameLabel = 32;
	public int topMargin = 12;  // margin space at top of screen above cards
	public int sideMargin = 12;


	float totalGuessNameChars;  // Total chars in the all the names to be guessed.

	FaceSprite fsCurrentUser = null;

	public MapReader.Map mapDataMap;
	public MapRenderer mapRendererMap;
	public float scaleMap { get { return mapRendererMap.goMap.transform.localScale.x; } set { mapRendererMap.goMap.transform.localScale = new Vector3(value, value, 1); } }
	// Upper left corner of map in world coordinates
	public static int xMapUL { get; set; }
	public static int yMapUL { get; set; }
	public static int xMapBR { get; private set; }
	public static int yMapBR { get; private set; }

	public static int xScreenMin { get; private set; }  // screen left edge
	public static int yScreenMax { get; private set; }  // screen top edge
	public static int xScreenMax { get; private set; }  // screen right edge
	public static int yScreenMin { get; private set; }  // screen bottom edge

	public static int ScreenPlayAreaWidth;
	public static int ScreenPlayAreaHeight;
	public static int ControlBarHeight;

	float scaleMapMax = 1.0f;
	float scaleMapMin = 0.5f;	// Typically set to fit-to-screen scale

	// These are assigned from the actual loaded map data.
	static public int pixelTileWidth = 24;
	static public int pixelTileHeight = 24;
	static public int pixelHalfTileWidth = (pixelTileWidth / 2);
	static public int pixelHalfTileHeight = (pixelTileHeight / 2);
	public int mapWidth;
	public int mapHeight;

	void Awake()
	{
		ScreenPlayAreaWidth = Screen.width;
		ScreenPlayAreaHeight = Screen.height - (int)cubeBG.transform.localScale.y;
		ControlBarHeight = (int)cubeBG.transform.localScale.y;

		mapDataMap = MapReader.GetMapFromFile("Floorplan.tmx");   // Dirt layer
		pixelTileWidth = mapDataMap.TileWidth;
		pixelHalfTileWidth = pixelTileWidth / 2;
		pixelTileHeight = mapDataMap.TileHeight;
		pixelHalfTileHeight = pixelTileHeight / 2;

		mapWidth = mapDataMap.Layers[0].Width * pixelTileWidth;
		mapHeight = mapDataMap.Layers[0].Height * pixelTileHeight;
		xMapUL = -(mapWidth / 2);// + (int)LayoutSet.layout.transform.localPosition.x;
		yMapUL = Screen.height/2; // mapHeight / 2;// + ControlBarHeight; // + (int)LayoutSet.layout.transform.localPosition.y;
		xMapBR = xMapUL + (mapDataMap.Width - 1) * pixelTileWidth;
		yMapBR = yMapUL - (mapDataMap.Height - 1) * pixelTileHeight;

		mapRendererMap = MapRenderer.CreateMapRenderer(mapDataMap, "Floorplan");
		mapRendererMap.goMap.transform.position = new Vector3(xMapUL + pixelHalfTileWidth, yMapUL - pixelHalfTileHeight, 0);
		mapRendererMap.goMap.SetActive(false);
		CenterMap centerMap = mapRendererMap.goMap.AddComponent<CenterMap>();
		centerMap.Init(mapDataMap, mapRendererMap);

		CalcMapScaleLimits();

		// Setup app states
		stateLoading = new State("Loading", Loading_Enter, Loading_Update, Loading_Exit);
		stateMemoryGame = new State("MemoryGame", MemoryGame_Enter, MemoryGame_Update, MemoryGame_Exit);
		stateFlashCards = new State("FlashCards", FlashCards_Enter, FlashCards_Update, FlashCards_Exit);
		stateYearbook = new State("Yearbook", Yearbook_Enter, Yearbook_Update, Yearbook_Exit);
		stateMapView = new State("MapView", MapView_Enter, MapView_Update, MapView_Exit);
		stateColorPicking = new State("ColorPicking", ColorPicking_Enter, ColorPicking_Update, ColorPicking_Exit);

		TransitionToState(stateLoading);

		oldScreenWidth = Screen.width;
		oldScreenHeight = Screen.height;

	}

	void CalcMapScaleLimits()
	{
		ScreenPlayAreaWidth = Screen.width;
		ScreenPlayAreaHeight = Screen.height - (int)cubeBG.transform.localScale.y;
		float scaleMapMinX = (float)ScreenPlayAreaWidth / (float)mapWidth;
		float scaleMapMinY = (float)ScreenPlayAreaHeight / (float)mapHeight;
		scaleMapMin = Mathf.Min(scaleMapMinY, scaleMapMinX);
		if (scaleMap < scaleMapMin)
		{
			scaleMap = scaleMapMin;
			valMapScaleSlider = scaleMap;
		}
	}
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

		guiStyleScore.font = guiStyleStats.font;
		guiStyleScore.fontSize = guiStyleStats.fontSize;
		guiStyleScore.normal.textColor = guiStyleStats.normal.textColor;
		guiStyleScore.alignment = TextAnchor.UpperCenter;

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

		YearBook.Init(FaceSprite.spriteCardBack.texture.width, FaceSprite.spriteCardBack.texture.height, widthCardSlot, heightPaddingCardSlot, topMargin, sideMargin);
		
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


		// Identify picture of current user.
		Debug.Log("Local user name: " + System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments));
		Debug.Log("Local user name: " + System.Environment.UserName);
		string envUserName = System.Environment.UserName;
		if (!string.IsNullOrEmpty(envUserName) && envUserName.Length > 1)
		{
			char firstInitial = char.ToLower(envUserName[0]);
			string lastName = envUserName.Substring(1).ToLower(); ;
			foreach (FaceSprite fs in faceSprites)
			{
				if (fs.lastName.ToLower() == lastName && char.ToLower(fs.firstName[0]) == firstInitial)
				{
					fsCurrentUser = fs;
					Debug.Log("Found current user picture: " + fs.firstName + " " + fs.lastName);
					break;
				}
			}
		}

	}

	#region STATES
	/************************************************************************************************************************************/
	/************************************************************************************************************************************/

	#region STATE_Loading
	/************************************************************************************************************************************/
	State stateLoading;
	public void Loading_Enter(State prevState)	{ doneLoading = false; }
	public void Loading_Exit(State nextState) { }
	public State Loading_Update()
	{
		if (doneLoading)
			return stateMemoryGame;

		return null;
	}
	#endregion STATE_Loading

	#region STATE_MemoryGame
	/************************************************************************************************************************************/
	State stateMemoryGame;
	public void MemoryGame_Enter(State prevState) {  }
	public void MemoryGame_Exit(State nextState) { }
	public State MemoryGame_Update()
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
				if (!IsHangManDead())
				{
					faceSpriteCrnt.collected = true;
					guiTextNofM.text = FaceSprite.GetNumCollected() + "/" + faceSprites.Count.ToString();
				}
				ReturnFaceToYearbook(faceSpriteCrnt);
				if (!AreAllCollected())
					ShowNextFace();
				else
				{
					guiTextName.color = Color.white;
					guiTextName.text = msgVictory;
					guiTextRole.text = "";
					if (fsCurrentUser != null)
					{
						iFaceSprite = fsCurrentUser.card.indexOrder;
						DisplayFaceSprite(fsCurrentUser, false);
					}
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
				if (Input.GetMouseButtonDown(0) && AreAllCollected())
					ReturnFaceToYearbook(faceSpriteCrnt);
			}
			else if (index >= 0 && index < faceSprites.Count && (index != iFaceSprite || (faceSpriteCrnt.card.transform.position.x != transform.position.x || faceSpriteCrnt.card.transform.position.y != transform.position.y)))
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
					return stateColorPicking;
			}
		}

		return null;
	}
	#endregion STATE_MemoryGame

	#region STATE_FlashCards
	/************************************************************************************************************************************/
	State stateFlashCards;
	public void FlashCards_Enter(State prevState) { }
	public void FlashCards_Exit(State nextState) { }
	public State FlashCards_Update()
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
				if (!IsHangManDead() && gameMode == GameMode.memoryGame) //!isFlashcardsMode)
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
					guiTextName.color = Color.white;
					guiTextName.text = msgVictory;
					guiTextRole.text = "";
					if (fsCurrentUser != null)
					{
						iFaceSprite = fsCurrentUser.card.indexOrder;
						DisplayFaceSprite(fsCurrentUser, false);
					}
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
				if (Input.GetMouseButtonDown(0) && (gameMode != GameMode.memoryGame || AreAllCollected())) //showAllFaces || isYearBookMode ||
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
		return null;
	}
	#endregion STATE_FlashCards

	#region STATE_Yearbook
	/************************************************************************************************************************************/
	State stateYearbook;
	public void Yearbook_Enter(State prevState) { }
	public void Yearbook_Exit(State nextState) { }
	public State Yearbook_Update()
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
				if (!IsHangManDead() && gameMode == GameMode.memoryGame) //!isFlashcardsMode)
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
					guiTextName.color = Color.white;
					guiTextName.text = msgVictory;
					guiTextRole.text = "";
					if (fsCurrentUser != null)
					{
						iFaceSprite = fsCurrentUser.card.indexOrder;
						DisplayFaceSprite(fsCurrentUser, false);
					}
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
				if (Input.GetMouseButtonDown(0) && (gameMode != GameMode.memoryGame || AreAllCollected())) //showAllFaces || isYearBookMode ||
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
		return null;
	}
	#endregion

	#region STATE_MapView
	/************************************************************************************************************************************/
	State stateMapView;
	public void MapView_Enter(State prevState) { }
	public void MapView_Exit(State nextState) { }
	public State MapView_Update()
	{
		return null;
	}
	#endregion STATE_MapView


	#region STATE_ColorPicking
	/************************************************************************************************************************************/
	State stateColorPicking;
	State stateColorPickingReturnTo;
	public void ColorPicking_Enter(State prevState) {
		stateColorPickingReturnTo = prevState;
		bgColorPicker.Show();
	}
	public void ColorPicking_Exit(State nextState) { }
	public State ColorPicking_Update()
	{
		if (!bgColorPicker.IsActive())
			return stateColorPickingReturnTo;

		return null;
	}
	#endregion STATE_ColorPicking
	/************************************************************************************************************************************/
	/************************************************************************************************************************************/
	#endregion STATES

	public bool AreAllCollected() { return FaceSprite.GetNumCollected()  == faceSprites.Count; }

	void SetupRolesComboBox()
	{
		float xComboBox = xStartComboBoxes;

		comboBoxList = new GUIContent[roles.Count+1];
		comboBoxList[0] = new GUIContent("All");
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


		comboBoxControl = new ComboBox(new Rect(xComboBox, Screen.height - btnHeightSpaced, comboButtonWidth, btnHeight), comboBoxList[0], comboBoxList, "button", "box", listStyle, "Department:");

		// What part of name they need to enter
		xComboBox += comboSpacing;
		comboBoxListNameGuess = new GUIContent[4];
		comboBoxListNameGuess[0] = new GUIContent("First & Last");
		comboBoxListNameGuess[1] = new GUIContent("First Name");
		comboBoxListNameGuess[2] = new GUIContent("Last Name");
		comboBoxListNameGuess[3] = new GUIContent("Department");

		comboBoxListNameSort = new GUIContent[6];
		comboBoxListNameSort[0] = new GUIContent("First & Last");
		comboBoxListNameSort[1] = new GUIContent("First Name");
		comboBoxListNameSort[2] = new GUIContent("Last Name");
		comboBoxListNameSort[3] = new GUIContent("Department");
		comboBoxListNameSort[4] = new GUIContent("Tenure Most");
		comboBoxListNameSort[5] = new GUIContent("Tenure Least");
		comboBoxControlName = new ComboBox(new Rect(xComboBox, Screen.height - btnHeightSpaced, comboButtonWidth, btnHeight), comboBoxListNameGuess[0], comboBoxListNameGuess, "button", "box", listStyle, "Guess:");

		xComboBox += comboSpacing;
		comboBoxListMode = new GUIContent[4];
		comboBoxListMode[0] = new GUIContent("Memory Game");
		comboBoxListMode[1] = new GUIContent("Flash Cards*");
		comboBoxListMode[2] = new GUIContent("Yearbook*");
		comboBoxListMode[3] = new GUIContent("Map");
		comboBoxControlMode = new ComboBox(new Rect(xComboBox, Screen.height - btnHeightSpaced, comboButtonWidth, btnHeight), comboBoxListMode[0], comboBoxListMode, "button", "box", listStyle, "Mode:");

		xComboBox += comboSpacing;
		comboBoxListTenure = new GUIContent[5];
		comboBoxListTenure[0] = new GUIContent("All");
		comboBoxListTenure[1] = new GUIContent("Most");
		comboBoxListTenure[2] = new GUIContent("Least");
		comboBoxListTenure[3] = new GUIContent("OGs*");
		comboBoxListTenure[4] = new GUIContent("Newbies*");
		comboBoxControlTenure = new ComboBox(new Rect(xComboBox, Screen.height - btnHeightSpaced, comboButtonWidth, btnHeight), comboBoxListTenure[0], comboBoxListTenure, "button", "box", listStyle, "Tenure Filter:");

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

		guiTextName.pixelOffset = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f + transform.position.y - 4);  
		guiTextRole.pixelOffset = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f + transform.position.y - 32); 
		guiTextNofM.pixelOffset = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f + transform.position.y - 64); 
		guiTextTheMan.pixelOffset = new Vector2(Screen.width * 0.64f, Screen.height * 0.5f + transform.position.y-16);
		guiTextGallows.pixelOffset = new Vector2(guiTextTheMan.pixelOffset.x + 2, guiTextTheMan.pixelOffset.y);

		if (comboBoxControl != null)
		{
			float xCombo = xStartComboBoxes;
			comboBoxControlMode.Reposition(new Rect(xCombo, Screen.height - btnHeightSpaced, comboButtonWidth, btnHeight));
			xCombo += comboSpacing;
			comboBoxControlName.Reposition(new Rect(xCombo, Screen.height - btnHeightSpaced, comboButtonWidth, btnHeight));
			xCombo += comboSpacing;
			comboBoxControl.Reposition(new Rect(xCombo, Screen.height - btnHeightSpaced, comboButtonWidth, btnHeight));
			xCombo += comboSpacing;
			comboBoxControlTenure.Reposition(new Rect(xCombo, Screen.height - btnHeightSpaced, comboButtonWidth, btnHeight));
		}

#if false
		bgColorPicker.startPos.x = (Screen.width - bgColorPicker.sizeFull) * 0.5f;
		bgColorPicker.startPos.y = Screen.height * 0.5f - transform.position.y - bgColorPicker.sizeFull; // - ColorPicker.alphaGradientHeight;
#else
		bgColorPicker.originPos.x = (Screen.width) * 0.5f;
		bgColorPicker.originPos.y = Screen.height * 0.5f - transform.position.y -1; // - ColorPicker.alphaGradientHeight;
#endif


	}
	void DisplayFaceSprite(FaceSprite fsToUse = null, bool refreshText = true)
	{
        if (faceSprites.Count <= 0)
            return;

		faceSpriteCrnt = (fsToUse != null) ? fsToUse : faceSprites[iFaceSprite];
		if (refreshText)
		{
			if (FaceSprite.iGuessNameIndex >= 4)
			{
				guiTextName.text = faceSpriteCrnt.fullName;
				guiTextNofM.text = faceSpriteCrnt.dateHired; 
			}
			else
			{
				guiTextName.text = (faceSpriteCrnt.collected || gameMode != GameMode.memoryGame) ? faceSpriteCrnt.guessName : "";
				guiTextNofM.text = (gameMode != GameMode.memoryGame) ? faceSpriteCrnt.dateHired :  FaceSprite.GetNumCollected() + "/" + faceSprites.Count.ToString();
			}
			guiTextBadChar.text = "";

			if (!faceSpriteCrnt.collected)
			{
				faceSpriteCrnt.countRevealed = 0;
				faceSpriteCrnt.countWrongChars = 0;
			}
			guiTextName.color = new Color(1, 1, 1, 1);
			//guiTextRole.text = (faceSpriteCrnt.collected || showAllFaces) ? (FaceSprite.iGuessNameIndex == 3 ? faceSpriteCrnt.fullName : faceSpriteCrnt.role) :  "";
			guiTextRole.text = (FaceSprite.iGuessNameIndex == 3 ? faceSpriteCrnt.fullName : faceSpriteCrnt.role);

		}
		faceSpriteCrnt.card.MoveTo(transform.position, YearBook.aspectCorrectHeight1 * heightFaceGuessDislplay, timeTransitionShowFace);
		faceSpriteCrnt.card.FlipShowFront();
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
			for (int i = 0; i < faceSprites.Count; i++)
			{
				faceSprites[i].card.FlipShowBack();
				faceSprites[i].collected = false;
				faceSprites[i].card.uiTextName.gameObject.SetActive(false);
			}
		}
		if (!AreAllCollected() && gameMode == GameMode.memoryGame)  //!isYearBookMode)
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
		if (gameMode == GameMode.memoryGame && FaceSprite.iGuessNameIndex >= 4) // !isYearBookMode && !showAllFaces
		{
			FaceSprite.iGuessNameIndex = iGuessnamePrevious = 0;
			comboBoxControlName.SelectedItemIndex = iGuessnamePrevious;
		}
	}

    bool DeptMatchesRole(string dept, string role)
    {
        if (dept == role)
            return true;
        if (role.Contains(dept))
            return true;
        return false;
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
				if (!DeptMatchesRole(roles[iDeptFilter-1], fs.role))
				{
					FilterOutFace(fs);
				}
			}
		}
		// Bring back ones from inactive list if match current dept selected.
		for (int i = faceSpritesFiltered.Count - 1; i >= 0; i--)
		{
			FaceSprite fs = faceSpritesFiltered[i];
			if (iDeptFilter == 0 || DeptMatchesRole(roles[iDeptFilter-1], fs.role))
			{
				faceSpritesFiltered.Remove(fs);
				totalGuessNameChars += fs.fullName.Length;
				faceSprites.Add(fs);
			}
		}
		countDeptTotal = faceSprites.Count;

		if (iTenureFilter > 0)
		{ // Apply tenure filter
			if (countTenureMembers < countDeptTotal)
			{
				faceSprites.Sort(delegate (FaceSprite fs1, FaceSprite fs2)
				{
					int diff = (FaceSprite.iGuessNameIndex == 5) ? System.DateTime.Compare(fs2.dateTime, fs1.dateTime) : System.DateTime.Compare(fs1.dateTime, fs2.dateTime);
					if (diff == 0)
					{
						diff = string.Compare(fs1.lastName + fs1.firstName, fs2.lastName + fs2.firstName);
					}
					return diff;
				});
				int countToRemove = countDeptTotal - countTenureMembers;
				int indexRemoveStart = ((iTenureFilter == 1 && FaceSprite.iGuessNameIndex != 5) || (iTenureFilter == 2 && FaceSprite.iGuessNameIndex == 5)) ? countTenureMembers : 0;
				int indexRemoveEnd = indexRemoveStart + countToRemove-1;
				for (int i = indexRemoveEnd; i >= indexRemoveStart; i--)
				{
					FilterOutFace (faceSprites[i]);
				}
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
		if (isFlashcardsMode || gameMode == GameMode.map)
			ReturnFaceToYearbook(faceSpriteCrnt);
	}

	public void ArrangeFacesOnPage()
	{
		foreach (FaceSprite fs in faceSprites)
		{
			ReturnFaceToYearbook(fs);
			//fs.card.ArrangeOnYearbook(0.25f);
		}
		if (gameMode == GameMode.memoryGame)
			faceSpriteCrnt.card.MoveTo(transform.position, YearBook.aspectCorrectHeight1 * heightFaceGuessDislplay, 0.25f);
	}

	void FilterOutFace(FaceSprite fs)
	{
		totalGuessNameChars -= fs.fullName.Length;
		fs.card.indexOrder = -1;
		fs.card.uiTextName.gameObject.SetActive(false);
		faceSprites.Remove(fs);
		faceSpritesFiltered.Add(fs);
		fs.collected = false;
		fs.card.MoveTo(transform.position, Vector2.zero, 0.5f);
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
		totalGuessNameChars = 0;
		string path = System.IO.Path.Combine(Application.streamingAssetsPath, "Faces");
		DirectoryInfo dir = new DirectoryInfo(path);
		FileInfo[] info = dir.GetFiles("*");
		foreach (FileInfo f in info)
		{
			if (f.FullName.EndsWith(".png") || f.FullName.EndsWith(".jpg"))
			{
				//Debug.Log("file: " + f.FullName);
				yield return LoadFaceSprite(f.FullName, f.CreationTime);
				guiTextNofM.text = faceSprites.Count.ToString();
			}
			if (debugMaxCards > 0 && faceSprites.Count >= debugMaxCards) break;
		}
	}

	int idDefault = 1;
	public IEnumerator LoadFaceSprite(string absoluteImagePath, System.DateTime dateTime)
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
				if (nameParts.Length >= 3)
				{
					if (nameParts.Length > 3)
					{ // parse date from name: YYYY-MM-DD
						string[] dateParts = nameParts[3].Split('-');
						int year;
						int month = 1;
						int day = 1;
						if (int.TryParse(dateParts[0], out year))
						{
							if (dateParts.Length > 1)
							{
								if (int.TryParse(dateParts[1], out month))
								{
									if (dateParts.Length > 2)
									{
										if (!int.TryParse(dateParts[2], out day))
											Debug.LogError("Unable to parse day from rom date part of filename:" + nameParts[3]);
									}
								}
								else
									Debug.LogError("Unable to parse month from rom date part of filename:" + nameParts[3]);
							}
							dateTime = new System.DateTime(year, month, day);
						}
						else
							Debug.LogError("Unable to parse year from date part of filename: " + nameParts[3]);
					}
					string idString = (idDefault++).ToString();
					//Debug.Log("File date: " + dateTime);
					if (nameParts.Length > 4)
					{ // parse employee id
						idString = nameParts[4];
					}

					FaceSprite faceSprite = new FaceSprite(nameParts[0], nameParts[1], nameParts[2], dateTime, idString, sprite, texture);
					if (faceSprite != null)
					{
						int indexOrder = faceSprites.Count;
						faceSprites.Add(faceSprite);
						++countDeptTotal;
                        if (!roles.Contains(nameParts[2]))
							roles.Add(nameParts[2]);
                        string[] roleParts = nameParts[2].Split('-');
                        if (roleParts.Length > 1)
                        { // It is a multi-role person, e.g. "IT-QA" so add addition to role of "IT-QA", add roles for "IT" and "QA"
                            foreach (string rolePart in roleParts)
                            {
                                if (!roles.Contains(rolePart))
                                    roles.Add(rolePart);
                            }
                        }

                        GameObject goFaceCard = Instantiate(faceCardPrefab);
						faceSprite.card = goFaceCard.GetComponent<Card>();
						faceSprite.card.indexOrder = indexOrder;
						faceSprite.card.Init(sprite, FaceSprite.spriteCardBack, FaceSprite.spriteCardFrontBG, FaceSprite.spriteCardFrontFrame, Vector3.zero);
						faceSprite.card.SetHeight(heightFaceDealStartDisplay);
						faceSprite.card.SetPos(transform.position);
						faceSprite.card.ArrangeOnYearbook();
						faceSprite.card.FlipShowBack(1.0f);
						faceSprite.card.uiTextName.gameObject.SetActive(false);


						totalGuessNameChars += faceSprite.fullName.Length;
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
	public override void Update() {
		base.Update();
		if (doneLoading)
		{
			if (!bgColorPicker.IsActive())
			{


				if (Input.GetMouseButtonUp(0))
				{
					if (valTenureSlider != valTenureSliderLastRelease)
					{
						valTenureSliderLastRelease = valTenureSlider;
						RestartCurrentMode();
					}
					if (valMapScaleSlider != valMapScaleSliderLastRelease)
					{
						valMapScaleSliderLastRelease = valMapScaleSlider;
						RestartCurrentMode();
					}
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
					CalcMapScaleLimits();

					//Mouse is inside the screen by 1 pixel or more, so can't be outside of window, therefore is over the game screen
					YearBook.Init(FaceSprite.spriteCardBack.texture.width, FaceSprite.spriteCardBack.texture.height, widthCardSlot, heightPaddingCardSlot, topMargin, sideMargin); // make it update it's Screen-size based values.
					if (!isYearBookMode)
					{
						Invoke("ArrangeFacesOnPage", 0.1f); // requires delay or it doesn't work. Don't know why.
					}
					else
					{
						ChangeToFromYearBookMode();
					}
					needScreenLayoutUpdate = false;
				}
			}
		}

		if (!AreAllCollected())
			timeGameEnded = Time.time;

		UpdateHangMan();
	}


	void ChangeToFromYearBookMode()
	{
		if (isYearBookMode)
		{
			YearBook.Init(FaceSprite.spriteCardBack.texture.width, FaceSprite.spriteCardBack.texture.height, widthYearbookNameLabel, heightYearBookNameLabel, topMargin, sideMargin);
			foreach (FaceSprite fs in faceSprites)
			{
				fs.card.uiTextName.gameObject.SetActive(true);
				fs.card.uiTextName.text = fs.fullName + "\n" + fs.role;
				//fs.collected = true;
				fs.card.FlipShowFront();
			}
			SortByGuessName();
			RestartGame(false);
		}
		else
		{
			YearBook.Init(FaceSprite.spriteCardBack.texture.width, FaceSprite.spriteCardBack.texture.height, widthCardSlot, heightPaddingCardSlot, topMargin, sideMargin);
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
			case 4: // Tenure Most (date of hire, earlier dates first)
				{
					int diff = System.DateTime.Compare(fs1.dateTime, fs2.dateTime);
					if (diff == 0)
					{
						diff = string.Compare(fs1.lastName + fs1.firstName, fs2.lastName + fs2.firstName);
					}
					return diff;
				}
			case 5: // Tenure Least (date of hire, laster dates first)
				{
					int diff = System.DateTime.Compare(fs2.dateTime, fs1.dateTime);
					if (diff == 0)
					{
						diff = string.Compare(fs1.lastName + fs1.firstName, fs2.lastName + fs2.firstName);
					}
					return diff;
				}
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
		if (gameMode != GameMode.memoryGame)
			guiTextNofM.text = "";

		if (!fs.collected && gameMode == GameMode.memoryGame) //!showAllFaces && !isYearBookMode
			fs.card.FlipShowBack();
		else
			fs.card.FlipShowFront();

		if (fs.card.indexOrder == -1)
			fs.card.MoveTo(transform.position, Vector2.zero, 0.5f);
		else if (gameMode == GameMode.map)
			fs.ArrangeOnMap(scaleMap);
		else
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

	void RestartCurrentMode()
	{
		FilterByDepartment(iDeptFilter);
		switch (gameMode)
		{
		case GameMode.memoryGame:
			RestartGame();
			break;
		case GameMode.flashCards:
			SortByGuessName();
			break;
		case GameMode.yearBook:
			ChangeToFromYearBookMode();
			break;
		case GameMode.map:
			break;
		}
	}
	void OnGUI()
	{
 		if (doneLoading && !bgColorPicker.IsActive())
		{

			// Department Selector
			int selectedItemIndex = comboBoxControl.Show();
			if (selectedItemIndex != iDeptFilter)
			{
				if (AreAllCollected())
				{
					if (fsCurrentUser != null)
					{
						ReturnFaceToYearbook(fsCurrentUser);
					}
				}
				iDeptFilter = selectedItemIndex;
				RestartCurrentMode();
			}

			// Name part to Guess selector, or sort filter for Yearbook mode.
			selectedItemIndex = comboBoxControlName.Show();
			if (selectedItemIndex != FaceSprite.iGuessNameIndex)
			{
				iGuessnamePrevious = FaceSprite.iGuessNameIndex;
				FaceSprite.iGuessNameIndex = selectedItemIndex;
				if (gameMode == GameMode.memoryGame) //(!showAllFaces && !isYearBookMode)
				{
					if (selectedItemIndex < 4)
					{
						Randomize();
						RestartGame();
					}
				}
				else
				{
					ReturnFaceToYearbook(faceSpriteCrnt);
					guiTextName.text = "";
					guiTextRole.text = "";

				}
				if (isFlashcardsMode || isYearBookMode)
					SortByGuessName();
				if (selectedItemIndex == 4 &&  gameMode == GameMode.memoryGame) //!isYearBookMode && !showAllFaces)
				{
					FaceSprite.iGuessNameIndex = iGuessnamePrevious;
					comboBoxControlName.SelectedItemIndex = iGuessnamePrevious;
				}
			}

			GameMode gameModeBefore = gameMode;
			selectedItemIndex = comboBoxControlMode.Show();
			if (selectedItemIndex != (int)gameMode)
			{
				if (gameMode == GameMode.memoryGame || selectedItemIndex == (int)GameMode.memoryGame)	// Flash change of Guess/SortBy selector if moving to or from Memory Game mode (i.e. if the selector is changing, don't flash between yearbook and flashcards swaps)
					comboBoxControlName.FlashLabelText();

				gameMode = (GameMode)selectedItemIndex;
				if (gameMode == GameMode.memoryGame)
				{ // Game Mode
					if (FaceSprite.iGuessNameIndex >= comboBoxListNameGuess.Length)
					{
						FaceSprite.iGuessNameIndex = iGuessnamePrevious = 0;
						comboBoxControlName.SelectedItemIndex = iGuessnamePrevious;
						comboBoxControlName.FlashButtonText();
					}
					comboBoxControlName.UpdateContent(comboBoxListNameGuess[FaceSprite.iGuessNameIndex], comboBoxListNameGuess);
					comboBoxControlName.comboLabel = "Guess:";
				}
				else // not Game Mode
				{
					comboBoxControlName.UpdateContent(comboBoxListNameSort[FaceSprite.iGuessNameIndex], comboBoxListNameSort);
					comboBoxControlName.comboLabel = (gameMode == GameMode.memoryGame) ? "Guess:" : "Sort by*:";
				}
				switch (gameMode)
				{
				case GameMode.memoryGame:	TransitionToState(stateMemoryGame); break;
				case GameMode.flashCards:	TransitionToState(stateFlashCards); break;
				case GameMode.yearBook:		TransitionToState(stateYearbook); break;
				case GameMode.map:			TransitionToState(stateMapView); break;
				}
			}
			//*************** Map Scale slider
			if (!comboBoxControlMode.isComboBoxOpen && gameMode == GameMode.map)
			{
				Rect rectMapScaleSlider = comboBoxControlMode.rectPosition;
				rectMapScaleSlider.y -= 32;
				valMapScaleSlider = GUI.HorizontalSlider(rectMapScaleSlider, valMapScaleSlider, scaleMapMin, scaleMapMax);
				scaleMap = valMapScaleSlider;
			}


			// *************** TENURE
			selectedItemIndex = comboBoxControlTenure.Show();
			if (selectedItemIndex != iTenureFilter)
			{
				iTenureFilter = selectedItemIndex;
				if (iTenureFilter >= 3)
				{
					valTenureSlider = 0.1f;	// select 10% of people
					switch (iTenureFilter)
					{
					case 3: // OGs
						iTenureFilter = 1;
						if (gameMode != GameMode.memoryGame)
							comboBoxControlName.SelectedItemIndex = 4;
						break;
					case 4: // Newbies
						iTenureFilter = 2;
						if (gameMode != GameMode.memoryGame)
							comboBoxControlName.SelectedItemIndex = 5;
						break;
					}
					if (gameMode != GameMode.memoryGame)
						comboBoxControlName.FlashButtonText(0.75f);

					comboBoxControlTenure.FlashButtonText(0.75f);
					comboBoxControlTenure.SelectedItemIndex = iTenureFilter;
				}
				RestartCurrentMode();
			}

			if (!comboBoxControlTenure.isComboBoxOpen && iTenureFilter != 0)
			{
				Rect rectTenureSlider = comboBoxControlTenure.rectPosition;
				rectTenureSlider.y -= 32;
				valTenureSlider = GUI.HorizontalSlider(rectTenureSlider, valTenureSlider, 0.0f, 1.0f);

				rectTenureSlider.y -= 16;
				rectTenureSlider.x += valTenureSlider * (rectTenureSlider.width - 12);
				if (!comboBoxControlTenure.isFlashedOff)
					GUI.Label(rectTenureSlider, new GUIContent(countTenureMembers.ToString()));
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
			if (!AreAllCollected() && timeGameStarted <= Time.time && gameMode == GameMode.memoryGame) //!showAllFaces && !isYearBookMode)
			{
				GUI.Label(new Rect(rectText.x + rectText.width, rectText.y, 16, 32), cursorStr, guiStyleStats);
				guiTextBadChar.pixelOffset = new Vector2(rectText.x + rectText.width + 16, Screen.height - rectText.y);
			}

			const float caseBtnWidth = 110;
			caseSensitive = GUI.Toggle(new Rect(/*btnHSpacing*/Screen.width - 120 - btnWidthSpaced, Screen.height - btnHeightSpaced * 3, caseBtnWidth, btnHeight), caseSensitive, "Case-sensitive");

			if (gameModeBefore != gameMode) //showAllFaces != showAllFacesBefore)
			{
				if (gameMode == GameMode.yearBook || gameModeBefore == GameMode.yearBook)
				{
					ChangeToFromYearBookMode();
				}
				if (gameMode == GameMode.memoryGame)
					RestartGame();
				else if (gameMode == GameMode.flashCards)
				{
					foreach (FaceSprite fs in faceSprites)
					{
						fs.card.FlipShowFront();
						fs.card.uiTextName.gameObject.SetActive(false);
						fs.card.ArrangeOnYearbook();
					}
					foreach (FaceSprite fs in faceSpritesFiltered)
					{
						fs.card.uiTextName.gameObject.SetActive(false);
					}
					ReturnFaceToYearbook(faceSpriteCrnt);
					SortByGuessName();
				}
				else if (gameMode == GameMode.map)
				{
					foreach (FaceSprite fs in faceSprites)
					{
						fs.card.FlipShowFront();
						fs.card.uiTextName.gameObject.SetActive(false);
						fs.ArrangeOnMap(scaleMap);
					}
					foreach (FaceSprite fs in faceSpritesFiltered)
					{
						fs.card.uiTextName.gameObject.SetActive(false);
					}
					ReturnFaceToYearbook(faceSpriteCrnt);
				}
				mapRendererMap.goMap.SetActive(gameMode == GameMode.map);
			}

			if (GUI.Button(new Rect(Screen.width - btnWidthSpaced, Screen.height - btnHeightSpaced * 3, 64, btnHeight), "Shuffle"))
			{
				Randomize();
				RestartGame(false);
			}
			if (GUI.Button(new Rect(Screen.width - btnWidthSpaced, Screen.height - btnHeightSpaced * 2, 64, btnHeight), "Restart"))
			{
				RestartCurrentMode();
			}

			if (GUI.Button(new Rect(Screen.width - btnWidthSpaced, Screen.height - btnHeightSpaced, btnWidth, btnHeight), "Exit"))
			{
				Application.Quit();
			}

			// Stats
			if (timeGameStarted <= Time.time && gameMode == GameMode.memoryGame) //!showAllFaces && !isYearBookMode)
			{
				// Accuracy
				int typedTotal = typedGood + typedBad + typedGoodWrongCase;
				float accuracy = (float)typedGood / (float)typedTotal;
				string accuracyPercent = (typedGood == typedTotal) ? "100" : (accuracy * 100.0f).ToString("00");
				if (typedTotal == 0)
					accuracyPercent = "__";
				GUI.Label(new Rect(Screen.width * 0.5f + 128, Screen.height - 32, 64, 32),  accuracyPercent + "%", guiStyleStats);

				// Timer

				float secondsElapsed = timeGameEnded - timeGameStarted;
				if (secondsElapsed < 0) secondsElapsed = 0;
				string minutes = Mathf.Floor(secondsElapsed / 60).ToString("00");
				string seconds = Mathf.Floor(secondsElapsed % 60).ToString("00");

				GUI.Label(new Rect(Screen.width * 0.5f - 128 - 58, Screen.height - 32, 64, 32), minutes + ":" + seconds, guiStyleStats);

				// Score. Only displayed if game completed.
				if (AreAllCollected() && guiTextRole.text == "")
				{
					guiTextName.text = msgVictory;
					secondsElapsed = Mathf.Floor(secondsElapsed);
					score = (secondsElapsed <= 0 || typedTotal == 0) ? 0 : (totalGuessNameChars / secondsElapsed) * (accuracy * accuracy * accuracy * accuracy) * totalGuessNameChars * 100f;
                    string scoreString = "Score: " + string.Format("{0:n0}", score); // Mathf.Floor(score).ToString("0000000");
                    GUI.Label(new Rect(Screen.width * 0.5f, Screen.height * 0.5f - transform.position.y+32, 0, 32), scoreString, guiStyleScore);
				}
			}

			// Version
			GUI.Label(new Rect(Screen.width-40 - btnWidthSpaced, Screen.height-16, 48, 16), "V 2.5", guiStyleVersion);
		}
    }
}
