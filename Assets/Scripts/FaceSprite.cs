using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceSprite : System.IComparable
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
	int randSortOrder;	// big random number

	public float secondsPerLetter;  // how much time user spent typing in the letters they typed.
	public int countWrongChars;     // How manny chars typed wrong while typing the name.
	public int countRevealed;       // How many chars were revealed for free (with right-arrow or Enter keys).
	public bool wasFullNameDisplayed;   // Flag turns true when full name displayed (whether by typing or reveals). After this is true, scoring stops for this face.
	public float timeStarted;       // game time in seconds when Face first shown.

	public Card card;
	public static Sprite spriteCardBack;
	public static Sprite spriteCardFrontFrame;
	public static Sprite spriteCardFrontBG;

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

}

