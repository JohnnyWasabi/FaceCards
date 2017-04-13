using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Card : MonoBehaviour {

	public Sprite spriteFace;
	public Sprite spriteBack;
	public SpriteRenderer spriteRenderer;
	public GameObject goFlipper;	// Generated parent object for flipping.
	public int indexOrder;

	public enum FlipState{
		none,
		toEdge,	// starting flip, shrinking to thin edge
		toFlat,	// ending flip, expanding from thin edge to flat again.
	}
	public FlipState flipState;

	// Use this for initialization
	void Awake () {
		goFlipper = new GameObject();
		transform.parent = goFlipper.transform;
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	public void Init(Sprite face, Sprite back, Vector2 pos)
	{
		spriteFace = face;
		spriteBack = back;
		goFlipper.transform.position = pos;
		transform.localPosition = Vector3.zero;
		spriteRenderer.sprite = face;
	}

	float timeFlipStart;
	public float flipDuration;

	// Update is called once per frame
	void Update () {

		if (flipState != FlipState.none)
		{
			float timeElapsed = Time.time - timeFlipStart;
			if (timeElapsed < flipDuration)
			{
				float radians = Mathf.PI * timeElapsed / flipDuration;
				float scaleFlip = Mathf.Cos(radians);
				if (scaleFlip <= 0)
				{
					scaleFlip = -scaleFlip;
					if (flipState != FlipState.toFlat)
					{
						flipState = FlipState.toFlat;
						spriteRenderer.sprite = (spriteRenderer.sprite == spriteFace) ? spriteBack : spriteFace;
						float scaleSprite;
						Vector3 pos;
						YearBook.Arrange(spriteRenderer.sprite, indexOrder, out scaleSprite, out pos);
						SetScale(scaleSprite);
						SetPos(pos);
					}
				}
				goFlipper.transform.localScale = new Vector3(scaleFlip, 1, 1);
			}
			else
			{
				flipState = FlipState.none;
				goFlipper.transform.localScale = new Vector3(1, 1, 1);
			}
		}
	}

	public void SetPos(Vector3 pos)
	{
		goFlipper.transform.position = pos;
	}
	void SetScale(float scale)
	{
		transform.localScale = new Vector3(scale, scale, 1);
	}

	public void Flip(float duration = 0.5f)
	{
		timeFlipStart = Time.time;
		flipDuration = duration;
		flipState = FlipState.toEdge;

	}

	public void ArrangeOnYearbook()
	{
		float scale;
		Vector3 pos;
		YearBook.Arrange(spriteRenderer.sprite, indexOrder, out scale, out pos);
		SetScale(scale);
		SetPos(pos);
	}
}
