using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Card : MonoBehaviour {

	public enum FlipState
	{
		none,
		toEdge, // starting flip, shrinking to thin edge
		toFlat, // ending flip, expanding from thin edge to flat again.
	}

	public SpriteRenderer spriteRendererCard;		// SpriteRenderer of the card (could be front side or backside), drawn behind face and face-frame
	public SpriteRenderer spriteRendererFace;		// SpriteRenderer for person's face on the front of the card
	public SpriteRenderer spriteRendererFaceFrame;   // SpriteRenderer for frame drawn on front of card over the person's face (like a picture frame).

	[HideInInspector]
	public Sprite spriteFace;
	[HideInInspector]
	public Sprite spriteFront;
	[HideInInspector]
	public Sprite spriteBack;
	[HideInInspector]
	public int indexOrder;

	[HideInInspector]
	public Vector3 posMoveStart;
	[HideInInspector]
	public float scaleMoveStart;
	[HideInInspector]
	public Vector3 posMoveEnd;
	[HideInInspector]
	public float scaleMoveEnd;
	[HideInInspector]
	public float timeMoveStart;
	[HideInInspector]
	public float moveDuration;

	[HideInInspector]
	public Vector2 dimCard;     // Dimensions of the card
	[HideInInspector]
	public Vector2 dimMoveStart;
	[HideInInspector]
	public Vector2 dimMoveEnd;

	[HideInInspector]
	public FlipState flipState;
	[HideInInspector]
	public bool isMoving;

	float timeFlipStart;
	float flipDuration;



	// Use this for initialization
	void Awake () {
	}

	public void Init(Sprite face, Sprite back, Sprite front, Sprite frontFrame, Vector2 pos)
	{
		spriteFace = face;
		spriteBack = back;
		spriteFront = front;    // Todo: get a different im
		dimCard = YearBook.dimCard;
		SetPos(pos);
		spriteRendererCard.sprite = front;
		spriteRendererFace.sprite = face;
		spriteRendererFaceFrame.sprite = frontFrame;
	}

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
						Sprite spriteOld = spriteRendererCard.sprite;
						spriteRendererCard.sprite = (spriteRendererCard.sprite == spriteFront) ? spriteBack : spriteFront;
						spriteRendererFace.enabled = 
							spriteRendererFaceFrame.enabled = (spriteRendererCard.sprite == spriteFront);
						if (isMoving)
						{
							float width = scaleMoveEnd * spriteOld.texture.width;
							float height = scaleMoveEnd * spriteOld.texture.height;

							float scaleSprite;
							Vector3 pos;
							//YearBook.ArrangeAt(spriteRenderer.sprite, posMoveEnd.x, posMoveEnd.y, out scaleSprite, out pos);
							//posMoveEnd = pos;
							//scaleMoveEnd = scaleSprite;

						}
						else
						{
							float scaleSprite;
							Vector3 pos;
							//YearBook.ArrangeAt(spriteRenderer.sprite, posMoveEnd.x, posMoveEnd.y, out scaleSprite, out pos);
							//SetScale(scaleSprite);
							//SetPos(pos);
						}
					}
				}
				transform.localScale = new Vector3(scaleFlip, 1, 1);
			}
			else
			{
				flipState = FlipState.none;
				transform.localScale = new Vector3(1, 1, 1);
			}
		}
		if (isMoving)
		{
			float timeElapsed = Time.time - timeMoveStart;
			if (timeElapsed < moveDuration)
			{
				float radians = Mathf.PI * 0.5f * timeElapsed / moveDuration;
				float sinLerp = Mathf.Sin(radians);
				SetPos(posMoveStart + (posMoveEnd - posMoveStart) * sinLerp);
				dimCard = dimMoveStart + (dimMoveEnd - dimMoveStart) * sinLerp;
				UpdateSpritesScales();
			}
			else
			{
				isMoving = false;
				SetPos(posMoveEnd);
				UpdateSpritesScales();
			}
		}
	}

	public void SetPos(Vector3 pos)
	{
		transform.position = new Vector3(pos.x, pos.y, -(dimCard.x/1000f));
	}
	public Vector3 GetPos()
	{
		return transform.position;
	}

	public void SetHeight(float height)
	{
		dimCard = YearBook.aspectCardVert1 * height;
		UpdateSpritesScales();
	}

	public void Flip(float duration = 0.5f)
	{
		timeFlipStart = Time.time;
		flipDuration = duration;
		flipState = FlipState.toEdge;
	}

	public void ArrangeOnYearbook(float moveDuration = 0.5f)
	{
		float scale;
		Vector3 pos;
		YearBook.Arrange(spriteRendererCard.sprite, indexOrder, out scale, out pos);
		if (moveDuration > 0)
		{
			MoveTo(pos, new Vector2(YearBook.picWidth, YearBook.picHeight), moveDuration);
		}
		else
		{
			//SetScale(scale);
			dimCard = YearBook.dimCard;
			SetPos(pos);
		}
	}

	public void UpdateSpritesScales()
	{
		UpdateSpriteObjScales(spriteRendererCard, true);
		UpdateSpriteObjScales(spriteRendererFaceFrame, false);
		UpdateSpriteObjScales(spriteRendererFace, false);
	}
	public void UpdateSpriteObjScales(SpriteRenderer spriteRenderer, bool stretch)
	{
		float scaleX = dimCard.x / spriteRenderer.sprite.texture.width; // faceSprite.texture.width;
		float scaleY = dimCard.y / spriteRenderer.sprite.texture.height; //faceSprite.texture.height;
		float scaleMin = Mathf.Min(scaleX, scaleY);

		if (!stretch)
		{
			scaleX = scaleY = scaleMin;
			spriteRenderer.transform.localPosition =  new Vector3 ( (dimCard.x - spriteRenderer.sprite.texture.width * scaleX) * 0.5f,  (dimCard.y - spriteRenderer.sprite.texture.height * scaleY) * 0.5f, spriteRenderer.transform.localPosition.z);
		}
		spriteRenderer.gameObject.transform.localScale = new Vector3(scaleX, scaleY, 1);

		transform.position = new Vector3(transform.position.x, transform.position.y, -(dimCard.x / 1000f)); ;

	}
	public void MoveTo(Vector3 newPos, Vector2 newDim, float duration)
	{
		timeMoveStart = Time.time;
		moveDuration = duration;
		dimMoveStart = dimCard;
		dimMoveEnd = newDim;
		posMoveStart = GetPos();
		posMoveEnd= newPos;
		isMoving = true;
		
	}
}
