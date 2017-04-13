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
	
	static public void Arrange(Sprite sprite, int index, out float scale, out Vector3 pos)
	{
		int row = index / cols;
		int col = index % cols;
		float x = col * picWidth;
		float y = row * picHeight;

		float scaleX = (float)picWidth / sprite.texture.width; // faceSprite.texture.width;
		float scaleY = (float)picHeight / sprite.texture.height; //faceSprite.texture.height;
		scale = Mathf.Min(scaleX, scaleY);

		x += Camera.main.transform.position.x - Screen.width * 0.5f;
		x += sprite.pivot.x * scale;
		y = Screen.height - y + Camera.main.transform.position.y - Screen.height * 0.5f;
		y -= picHeight - sprite.pivot.y * scale;

		pos = new Vector3(x, y, 0.1f);

	}
}
