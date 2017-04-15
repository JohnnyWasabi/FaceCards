using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YearBook {

	static int cols;
	static int rows;
	static public int picWidth = 64;
	static public int picHeight = 72;


	static public Vector2 aspectCardHorz1 = new Vector2(1.0f, 9f / 8f);
	static public Vector2 aspectCardVert1 = new Vector2(8f/9f, 1.0f);
	static public Vector2 dimCard = new Vector2(64, 72);

	static public void Init () {
		cols = Screen.width / picWidth;
		rows = Screen.height / picHeight;
	}
	
	static public void Arrange(Sprite sprite, int index, out float scale, out Vector3 pos)
	{
		int row = index / cols;
		int col = index % cols;
		float x = col * picWidth + picWidth*0.5f;
		float y = row * picHeight + picHeight;

		float scaleX = (float)picWidth / sprite.texture.width; // faceSprite.texture.width;
		float scaleY = (float)picHeight / sprite.texture.height; //faceSprite.texture.height;
		scale = Mathf.Min(scaleX, scaleY);

		x += Camera.main.transform.position.x - Screen.width * 0.5f;
//		x += sprite.pivot.x * scale;
		y = Screen.height - y + Camera.main.transform.position.y - Screen.height * 0.5f;
//		y -= picHeight - sprite.pivot.y * scale;

		pos = new Vector3(x, y, 0.1f);

	}

	static public void ArrangeAt(Sprite sprite, float x, float y, float picScale, out float scale, out Vector3 pos)
	{
		float scaleX = (float)picWidth * picScale / sprite.texture.width; // faceSprite.texture.width;
		float scaleY = (float)picHeight * picScale / sprite.texture.height; //faceSprite.texture.height;
		scale = Mathf.Min(scaleX, scaleY);

		//x += Camera.main.transform.position.x - Screen.width * 0.5f;
		x += sprite.pivot.x * scale;
		//y = Screen.height - y + Camera.main.transform.position.y - Screen.height * 0.5f;
		y -= picHeight - sprite.pivot.y * scale;

		pos = new Vector3(x, y, 0.1f);

	}
}
