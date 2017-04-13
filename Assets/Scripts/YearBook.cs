using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YearBook {

	static int cols;
	static int rows;
	static public int picWidth = 64;
	static public int picHeight = 72;

	static public void Init () {
		cols = Screen.width / picWidth;
		rows = Screen.height / picHeight;
	}
	
	static public void ArrangeFaceSprite(FaceSprite faceSprite, int index)
	{
		int row = index / cols;
		int col = index % cols;
		float x = col * picWidth;
		float y = row * picHeight;

		float scaleX = (float)picWidth / faceSprite.spriteRenderYearbook.sprite.texture.width; // faceSprite.texture.width;
		float scaleY = (float)picHeight / faceSprite.spriteRenderYearbook.sprite.texture.height; //faceSprite.texture.height;
		float scale = Mathf.Min(scaleX, scaleY);

		x += Camera.main.transform.position.x - Screen.width * 0.5f;
		x += faceSprite.spriteRenderYearbook.sprite.pivot.x * scale;
		y = Screen.height - y + Camera.main.transform.position.y - Screen.height * 0.5f;
		y -= picHeight - faceSprite.spriteRenderYearbook.sprite.pivot.y * scale;

		faceSprite.spriteRenderYearbook.gameObject.transform.localScale = new Vector3(scale, scale, 1f);
		//faceSprite.spriteRenderYearbook.gameObject.transform.position = new Vector3(x, y, 0.1f);
		faceSprite.cardYearbook.SetPos(new Vector3(x, y, 0.1f));

	}
}
