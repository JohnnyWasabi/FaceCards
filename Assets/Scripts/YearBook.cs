using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YearBook {

	static int cols;
//	static int rows;
	static public Vector2 aspectCorrectWidth1 = new Vector2(1.0f, 9f / 8f);	// multiply this by a width you want your result to be, and the height will be at correct aspect ratio.
	static public Vector2 aspectCorrectHeight1 = new Vector2(8f/9f, 1.0f);   // multiply this by a height you want your result to be, and the width will be at correct aspect ratio.
	static public Vector2 dimPhoto = new Vector2(64, 72);   // Dimensions of picture  as it appears in yearbook layout.

	// Extra space below picture required for centered name. 
	static int nameWidth;
	static int nameHeight;
	static int slotWidth;
	static int slotHeight;

	static int dxCentered;
	static public void Init(int thumbnailWidth, int thumbnailHeight, int nameLabelWidth = 0, int nameLabelHeight = 0) {
		dimPhoto = new Vector2(thumbnailWidth, thumbnailHeight);
		aspectCorrectWidth1 = new Vector2(1.0f, dimPhoto.y / dimPhoto.x);
		aspectCorrectHeight1 = new Vector2(dimPhoto.x / dimPhoto.y, 1.0f);

		nameWidth = nameLabelWidth;
		nameHeight = nameLabelHeight;
		slotWidth = Mathf.Max((int)dimPhoto.x, nameWidth);
		slotHeight = (int)dimPhoto.y + nameHeight;
		cols = (int)(Screen.width / slotWidth);
//		rows = (int)(Screen.height / slotHeight);

		dxCentered = (Screen.width - (cols * slotWidth)) / 2;
	}
	
	static public Vector3 GetPosOfCard(Sprite sprite, int indexCard)
	{
		int row = indexCard / cols;
		int col = indexCard % cols;
		float x = col * slotWidth + slotWidth * 0.5f + dxCentered;
		float y = row * slotHeight + slotHeight - nameHeight;

		x += Camera.main.transform.position.x - Screen.width * 0.5f;
		y = Screen.height - y + Camera.main.transform.position.y - Screen.height * 0.5f;

		return new Vector3(x, y, 0.1f);

	}

	// y==0 is at bottom left corner of screen. y==Screen.height at top of screen.
	static public int IndexAtScreenXY(int x, int y)
	{
		y = Screen.height - 1 - y;
		int row = y / slotHeight;
		int col = (x-dxCentered) / slotWidth;
		int index = row * cols + col;

		
		float xcardCenter = col * slotWidth + slotWidth * 0.5f + dxCentered;
		float halfPhotoWidth = dimPhoto.x * 0.5f;
		if (x < xcardCenter - halfPhotoWidth || x > xcardCenter + halfPhotoWidth)
			index = -1;

		return index;
	}
}
