using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YearBook : MonoBehaviour {

	int cols;
	int rows;
	public int picWidth = 64;
	public int picHeight = 72;
	public List<FaceSprite> slots;

	void Awake()
	{
		slots = new List<FaceSprite>();
	}
	// Use this for initialization
	void Start () {
		cols = Screen.width / picWidth;
		rows = Screen.height / picHeight;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void AddFaceSprite(FaceSprite faceSprite)
	{
		int index = slots.Count;

		int row = index / cols;
		int col = index % cols;
		float x = col * picWidth;
		float y = row * picHeight;

		float scaleX = (float)picWidth / faceSprite.texture.width;
		float scaleY = (float)picHeight / faceSprite.texture.height;
		float scale = Mathf.Min(scaleX, scaleY);
		/*
		y += Screen.height - y;
		y += picHeight;
		x += picWidth * 0.5f;
		*/
		x += Camera.main.transform.position.x - Screen.width * 0.5f;
		x += faceSprite.sprite.pivot.x * scale;
		y = Screen.height - y + Camera.main.transform.position.y - Screen.height * 0.5f;
		y -= picHeight - faceSprite.sprite.pivot.y * scale;

		faceSprite.spriteRenderYearbook.gameObject.transform.localScale = new Vector3(scale, scale, 1f);
		faceSprite.spriteRenderYearbook.gameObject.transform.position = new Vector3(x, y, 0.1f);
		slots.Add(faceSprite);
	}
}
