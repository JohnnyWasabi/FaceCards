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
	public UnityEngine.UI.Text uiTextName; 

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

	public enum WaggleState {  stopped, waggling, stopping}
	public WaggleState waggleState;
	public bool isWaggling { get { return waggleState == WaggleState.waggling; } }
	float timeWaggleAnchor = 0;
	float sinWagglePrev;

	bool bShowFront;

	// Use this for initialization
	void Awake () {
		waggleState = WaggleState.stopped;
	}

	public void StartWaggle()
	{
		if (!isWaggling)
		{
			timeWaggleAnchor = Time.time;
			waggleState = WaggleState.waggling;
		}
	}
	public void StopWaggle()
	{
		if (waggleState != WaggleState.stopped)
			waggleState = WaggleState.stopping;
	}

	public void Init(Sprite face, Sprite back, Sprite front, Sprite frontFrame, Vector2 pos)
	{
		spriteFace = face;
		spriteBack = back;
		spriteFront = front;    // Todo: get a different im
		dimCard = YearBook.dimPhoto;
		SetPos(pos);
		spriteRendererCard.sprite = front;
		spriteRendererFace.sprite = face;
		spriteRendererFaceFrame.sprite = frontFrame;
		bShowFront = true;
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
						spriteRendererCard.sprite = (bShowFront) ? spriteFront : spriteBack;
						spriteRendererFace.enabled = spriteRendererFaceFrame.enabled = bShowFront;
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
		if (waggleState != WaggleState.stopped)
		{
			float timeDelta = Time.time - timeWaggleAnchor;
			float radians = timeDelta * (2f * Mathf.PI * 1.5f);// 1.75f);
			float sinWaggle = Mathf.Sin(radians);
			float tiltDegs = sinWaggle * 30f;// 45f;
			Vector3 eulers = transform.localRotation.eulerAngles;
			transform.localRotation = Quaternion.Euler(eulers.x, eulers.y, tiltDegs);
			
			if (waggleState == WaggleState.stopping)
			{
				if (Mathf.Sign(sinWaggle) != Mathf.Sign(sinWagglePrev))
				{
					transform.localRotation = Quaternion.Euler(eulers.x, eulers.y, 0);
					waggleState = WaggleState.stopped;
				}
			}
			sinWagglePrev = sinWaggle;
		}
	}

	public void SetPos(Vector3 pos)
	{
		transform.localPosition = new Vector3(pos.x, pos.y, -(dimCard.x/1000f));
	}
	public Vector3 GetPos()
	{
		return transform.localPosition;
	}

	public void SetHeight(float height)
	{
		dimCard = YearBook.aspectCorrectHeight1 * height;
		UpdateSpritesScales();
	}

	public void Flip(float duration = 0.5f)
	{
		bShowFront = !bShowFront;
		if (flipState == FlipState.none)
		{
			timeFlipStart = Time.time;
			flipDuration = duration;
			flipState = FlipState.toEdge;
		}
		else 
		{
			float timeElapsed = Time.time - timeFlipStart;
			timeFlipStart = Time.time - (flipDuration- timeElapsed);
			flipState = (flipState == FlipState.toEdge) ? FlipState.toFlat : FlipState.toEdge;
		}
	}

	public void FlipShowFront(float duration = 0.5f)
	{
		if (!bShowFront)
			Flip(duration);
	}
	public void FlipShowBack(float duration = 0.5f)
	{
		if (bShowFront)
			Flip(duration);
	}
	public void ArrangeOnYearbook(float moveDuration = 0.5f)
	{
		Vector3 pos = YearBook.GetPosOfCard(spriteRendererCard.sprite, indexOrder);
		if (moveDuration > 0)
		{
			MoveTo(pos, YearBook.dimPhoto, moveDuration);
		}
		else
		{
			dimCard = YearBook.dimPhoto;
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
			spriteRenderer.transform.localPosition =  new Vector3 ( 0,  (dimCard.y - spriteRenderer.sprite.texture.height * scaleY) * 0.5f, spriteRenderer.transform.localPosition.z);
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

	public bool IsWorldXYInCard(float xWorld, float yWorld)
	{
		Vector2 dimCardScaled = dimCard * transform.lossyScale.x;
		float xMin = transform.position.x - dimCardScaled.x * 0.5f;
		
		return (xMin <= xWorld && xWorld <= xMin + dimCardScaled.x
			&& transform.position.y <= yWorld && yWorld <= transform.position.y + dimCardScaled.y
			);
	}
}
