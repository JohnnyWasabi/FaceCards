using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceSprite : System.IComparable
{
	public const int iGuessFullName = 0;
	public const int iGuessFirstName = 1;
	public const int iGuessLastName = 2;
	public const int iGuessRole = 3;
	public const int iGuessProjects = 4;
	public const int iGuessMax = 4;

	public FaceSprite(string first, string last, string role, string projects, System.DateTime date, string idStr, Sprite sprite, Texture2D texture)
	{
		guessNames = new string[iGuessMax + 1];
		firstName = first;
		lastName = last;
		fullName = first + " " + last;
		dateTime = date;
		id = idStr;
		dateHired = date.Year.ToString() + "-" + date.Month.ToString("00") + "-" + date.Day.ToString("00");
		this.role = role;
		this.projects = projects;
		this.sprite = sprite;
		this.texture = texture;
		RandomizeOrder();
		collected = false;
		++numCreated;
	}
	static int numCollected = 0;
	static int numCreated = 0;


	static public int iGuessNameIndex = 0;
	static public int GetNumCollected() { return numCollected;  }

	public System.DateTime dateTime;	// Date encoded in file name, or if non present then from system file creation time.
	public string[] guessNames;
	public string fullName		{ get { return guessNames[0]; } set { guessNames[0] = value; } }
	public string firstName		{ get { return guessNames[1]; } set { guessNames[1] = value; } }
	public string lastName		{ get { return guessNames[2]; } set { guessNames[2] = value; } }
	public string role			{ get { return guessNames[3]; } set { guessNames[3] = value; } }
	public string projects		{ get { return guessNames[4]; } set { guessNames[4] = value; } }
	public string dateHired; 	//{ get { return guessNames[5]; } set { guessNames[5] = value; } }
	public string id; 			//{ get { return guessNames[6]; } set { guessNames[6] = value; } }
	// The actual string they need to guess.
	public string guessName		{ get { return guessNames[iGuessNameIndex]; } set { } }
	public Sprite sprite;
	public Texture2D texture;
	int randSortOrder;	// big random number

	public float secondsPerLetter;  // how much time user spent typing in the letters they typed.
	public int countWrongChars;     // How manny chars typed wrong while typing the name.
	public int countRevealed;       // How many chars were revealed for free (with right-arrow or Enter keys).
	public bool wasFullNameDisplayed;   // Flag turns true when full name displayed (whether by typing or reveals). After this is true, scoring stops for this face.
	public float timeStarted;       // game time in seconds when Face first shown.
	public bool _collected;     // True if player got name right and added this card to their collection.
	public bool collected { get { return _collected; }
		set {
			if (_collected != value)
			{
				_collected = value;
				numCollected += _collected ? 1 : -1;
			}
		}
	}			// True if player got name right and added this card to their collection.

	public Card card;
	public static Sprite spriteCardBack;
	public static Sprite spriteCardFrontFrame;
	public static Sprite spriteCardFrontBG;

	void OnDestroy()
	{
		collected = false;
		--numCreated;
	}
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
		countRevealed = 0;
		countWrongChars = 0;
		wasFullNameDisplayed = false;
	}

	public void ArrangeOnMap(float moveDuration = 0.5f)
	{
		Vector2 pos = SpotManager.GetSpotPos(id);
		pos.y = -pos.y;
		pos.y -= YearBook.dimPhoto.y * 0.5f;
		card.MoveTo(pos, YearBook.dimPhoto, moveDuration);
	}
}

