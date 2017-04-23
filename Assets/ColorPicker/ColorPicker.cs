using UnityEngine;
using System.Collections;

public class ColorPicker : MonoBehaviour {

	public Texture2D colorSpace;
	public Texture2D alphaGradient;
	public string Title = "Color Picker";
	public Vector2 startPos = new Vector2(20, 20);
	public GameObject receiver;
	public string colorSetFunctionName = "OnSetNewColor";
	public string colorGetFunctionName = "OnGetColor";
	public bool useExternalDrawer = false;
	public int drawOrder = 0;
	public bool showAlpha = true;

	private Color TempColor; 
	private Color SelectedColor;
	private Color OriginalColor;

	static ColorPicker activeColorPicker = null;

	enum ESTATE
	{
		Hidden,
		Showed,
		Showing,
		Hidding
	}; 
	ESTATE mState = ESTATE.Hidden;
	
	public int sizeFull = 200;
	public int sizeHidden = 20;
	float animTime = 0.25f;
	float dt = 0;

	float sizeCurr = 0;
	public const float alphaGradientHeight = 16;

	GUIStyle titleStyle = null;
	Color textColor = Color.black;
	Texture2D txColorDisplay;

	string txtR, txtG, txtB, txtA;
	float valR, valG, valB, valA;
	
	public void NotifyColor(Color color)
	{
		SetColor(color);
		SelectedColor = color;
		UpdateColorEditFields(false);
		UpdateColorSliders(false);
	}

	void Start()
	{
		sizeCurr = sizeHidden;

		txColorDisplay = new Texture2D(1, 1, TextureFormat.ARGB32, false);
		if(receiver)
		{
			receiver.SendMessage(colorGetFunctionName, this, SendMessageOptions.DontRequireReceiver);
		}
	}


	void OnGUI()
	{
		if(!useExternalDrawer)
		{
			_DrawGUI();
		}
	}

	void UpdateColorSliders(bool isFocused)
	{
		if(!isFocused)
		{
			valR = TempColor.r;
			valG = TempColor.g;
			valB = TempColor.b;
			valA = TempColor.a;
		}
		else
		{
			SetColor(new Color(valR, valG, valB, valA));
			ApplyColor();
		}
	}

	void UpdateColorEditFields(bool isFocused)
	{
		if(!isFocused)
		{
			txtR = ((int)(255f * TempColor.r)).ToString();
			txtG = ((int)(255f * TempColor.g)).ToString();
			txtB = ((int)(255f * TempColor.b)).ToString();
			txtA = ((int)(255f * TempColor.a)).ToString();
		}
		else
		{
			byte r = 0;
			byte g = 0;
			byte b = 0;
			byte a = 0;
			if(!string.IsNullOrEmpty(txtR)) {
				int rInt;
				if (!int.TryParse(txtR, out rInt))
				{
					r = (byte)(255f * TempColor.r);
				}
				else
					r = (byte)Mathf.Clamp(rInt, 0, 255);

				txtR = r.ToString();
				//r = byte.Parse(txtR, System.Globalization.NumberStyles.Any);
			}
			if(!string.IsNullOrEmpty(txtG)) {
				int gInt;
				if (!int.TryParse(txtG, out gInt))
				{
					g = (byte)(255f * TempColor.g);
				}
				else
					g = (byte)Mathf.Clamp(gInt, 0, 255);

				txtG = g.ToString();
				//g = byte.Parse(txtG, System.Globalization.NumberStyles.Any);
			}
			if(!string.IsNullOrEmpty(txtB)) {
				int bInt;
				if (!int.TryParse(txtB, out bInt))
				{
					b = (byte)(255f * TempColor.b);
				}
				else
					b = (byte)Mathf.Clamp(bInt, 0, 255);

				txtB = b.ToString();
				//b = byte.Parse(txtB, System.Globalization.NumberStyles.Any);
			}
			if(!string.IsNullOrEmpty(txtA)) {
				int aInt;
				if (!int.TryParse(txtA, out aInt))
				{
					a = (byte)(255f * TempColor.a);
				}
				else
					a = (byte)Mathf.Clamp(aInt, 0, 255);

				txtA = a.ToString();
				//a = byte.Parse(txtA, System.Globalization.NumberStyles.Any);
			}
			SetColor(new Color32(r, g, b, a));
		}
	}

	public void Show()
	{
		if (mState == ESTATE.Hidden)
		{
			mState = ESTATE.Showing;
			activeColorPicker = this;
			dt = 0;
			OriginalColor = GetColor();
			NotifyColor(OriginalColor);
		}
	}

	// Update is called once per frame
	public void _DrawGUI () 
	{
		if (titleStyle == null) {
			titleStyle = new GUIStyle (GUI.skin.label);
			titleStyle.normal.textColor = textColor;
		}

		Rect rectColorEdit = new Rect(startPos.x + sizeCurr + 10, startPos.y + 30, 40, 140);
		Rect rectColorSlider = new Rect(startPos.x + sizeCurr + 50, startPos.y + 30, 60, 140);

		GUI.Label(new Rect(startPos.x + sizeCurr + 60, startPos.y, 200, 30), Title, titleStyle);

		GUI.DrawTexture(new Rect(startPos.x + sizeCurr + 10, startPos.y, 40, 20), txColorDisplay);

		if(mState == ESTATE.Showed)
		{
			string txtOldR = txtR;
			string txtOldG = txtG;
			string txtOldB = txtB;
			string txtOldA = txtA;
			txtR = GUI.TextField(new Rect(startPos.x + sizeCurr + 10, startPos.y + 30, 40, 20), txtR, 3);
			txtG = GUI.TextField(new Rect(startPos.x + sizeCurr + 10, startPos.y + 60, 40, 20), txtG, 3);
			txtB = GUI.TextField(new Rect(startPos.x + sizeCurr + 10, startPos.y + 90, 40, 20), txtB, 3);
			if (showAlpha)
				txtA = GUI.TextField(new Rect(startPos.x + sizeCurr + 10, startPos.y + 120, 40, 20), txtA, 3);
			valR = GUI.HorizontalSlider(new Rect(startPos.x + sizeCurr + 50, startPos.y + 35, 60, 20), valR, 0.0f, 1.0f);
			valG = GUI.HorizontalSlider(new Rect(startPos.x + sizeCurr + 50, startPos.y + 65, 60, 20), valG, 0.0f, 1.0f);
			valB = GUI.HorizontalSlider(new Rect(startPos.x + sizeCurr + 50, startPos.y + 95, 60, 20), valB, 0.0f, 1.0f);

			if (txtOldR != txtR || txtOldG != txtG || txtOldB != txtB)
			{
				UpdateColorEditFields(true);
				UpdateColorSliders(false);
				SetColor(new Color(valR, valG, valB, valA));
				ApplyColor();
			}
			if (showAlpha)
				valA = GUI.HorizontalSlider(new Rect(startPos.x + sizeCurr + 50, startPos.y + 125, 60, 20), valA, 0.0f, 1.0f);
			if(GUI.Button(new Rect(startPos.x + sizeCurr + 10, startPos.y + 150, 60, 20), "OK"))
			{
				ApplyColor();
				SelectedColor = TempColor;
				if(receiver)
				{
					receiver.SendMessage(colorSetFunctionName, SelectedColor, SendMessageOptions.DontRequireReceiver);
				}
				mState = ESTATE.Hidding;
			}
			if (GUI.Button(new Rect(startPos.x + sizeCurr + 10, startPos.y + 150 + 30, 60, 20), "Cancel"))
			{
				SetColor(OriginalColor);
				receiver.SendMessage(colorSetFunctionName, OriginalColor, SendMessageOptions.DontRequireReceiver);
				mState = ESTATE.Hidding;
			}
			GUIStyle labelStyleRGBA = new GUIStyle(GUI.skin.label);
			labelStyleRGBA.normal.textColor = Color.white;
			GUI.Label(new Rect(startPos.x + sizeCurr + 110, startPos.y + 30, 20, 20), "R", labelStyleRGBA);
			GUI.Label(new Rect(startPos.x + sizeCurr + 110, startPos.y + 60, 20, 20), "G", labelStyleRGBA);
			GUI.Label(new Rect(startPos.x + sizeCurr + 110, startPos.y + 90, 20, 20), "B", labelStyleRGBA);
			if (showAlpha)
				GUI.Label(new Rect(startPos.x + sizeCurr + 110, startPos.y + 120, 20, 20), "A", labelStyleRGBA);
		}

		//update scaling states
		if(mState == ESTATE.Showing)
		{
			sizeCurr = Mathf.Lerp(sizeHidden, sizeFull, dt/animTime);
			if(dt/animTime > 1.0f) {
				mState = ESTATE.Showed;
			}
			dt += Time.deltaTime;
		}
		if(mState == ESTATE.Hidding)
		{
			sizeCurr = Mathf.Lerp(sizeFull, sizeHidden, dt/animTime);
			if(dt/animTime > 1.0f) {
				mState = ESTATE.Hidden;
			}
			dt += Time.deltaTime;
		}
		//draw color picker
		Rect rect = new Rect(startPos.x, startPos.y, sizeCurr, sizeCurr);
//		if (mState != ESTATE.Hidden) // jma
			GUI.DrawTexture(rect, colorSpace);

		Rect rectFullSize = new Rect(startPos.x, startPos.y, sizeCurr, sizeCurr);

		float alphaGradHeight = alphaGradientHeight * (sizeCurr / sizeFull);
		Vector2 startPosAlpha = startPos + new Vector2(0, sizeCurr);
		Rect rectAlpha = new Rect(startPosAlpha.x, startPosAlpha.y, sizeCurr, alphaGradHeight);
		if (showAlpha)
		{
			if (mState != ESTATE.Hidden) // jma
				GUI.DrawTexture(rectAlpha, alphaGradient);

			rectFullSize = new Rect(startPos.x, startPos.y, sizeCurr, sizeCurr + alphaGradHeight);
		}

		Vector2 mousePos = Event.current.mousePosition;
		Event e = Event.current;
		bool isLeftMBtnClicked = e.type == EventType.mouseUp;
		bool isLeftMBtnDragging = e.type == EventType.MouseDrag;
#if false
		bool openCondition = (rectFullSize.Contains(e.mousePosition) && (((e.type == EventType.MouseUp || e.type == EventType.mouseDrag || e.type == EventType.MouseMove) && e.isMouse)));
//		bool closeCondition = isLeftMBtnClicked || (!rectFullSize.Contains(e.mousePosition)) && (e.isMouse && (e.type == EventType.MouseMove || e.type == EventType.MouseDown));
		if(openCondition && (activeColorPicker == null || activeColorPicker.mState == ESTATE.Hidden))
		{
			if(mState == ESTATE.Hidden)
			{
				mState = ESTATE.Showing;
				activeColorPicker = this;
				dt = 0;
				OriginalColor = GetColor();
			}
		}

		if(closeCondition)
		{
			if(mState == ESTATE.Showed)
			{
				if(isLeftMBtnClicked)
				{
					ApplyColor();
				}
				else
				{
					SetColor(SelectedColor);
				}

				mState = ESTATE.Hidding;
				dt = 0;
			}
		}
#endif
		if (mState == ESTATE.Showed)
		{
			if(rect.Contains(e.mousePosition) && (isLeftMBtnClicked || isLeftMBtnDragging))
			{
				float coeffX = colorSpace.width / sizeCurr;
				float coeffY = colorSpace.height / sizeCurr;
				Vector2 localImagePos = (mousePos - startPos);
				Color res = colorSpace.GetPixel((int)(coeffX * localImagePos.x), colorSpace.height - (int)(coeffY * localImagePos.y)-1);
				SetColor(res);
				//if(isLeftMBtnDragging )
				{
					ApplyColor();
				}
				UpdateColorEditFields(false);
				UpdateColorSliders(false);
			}
			else if(showAlpha && rectAlpha.Contains(e.mousePosition))
			{
				float coeffX = alphaGradient.width / sizeCurr;
				float coeffY = alphaGradient.height / sizeCurr;
				Vector2 localImagePos = (mousePos - startPosAlpha);
				Color res = alphaGradient.GetPixel((int)(coeffX * localImagePos.x), colorSpace.height - (int)(coeffY * localImagePos.y)-1);
				Color curr = GetColor();
				curr.a = res.r;
				SetColor(curr);
				if(isLeftMBtnDragging)
				{
					ApplyColor();
				}
				UpdateColorEditFields(false);
				UpdateColorSliders(false);
			}
			else if(rectColorEdit.Contains(e.mousePosition))
			{
				UpdateColorEditFields(true);
				UpdateColorSliders(false);
			}
			else if(rectColorSlider.Contains(e.mousePosition))
			{
				UpdateColorEditFields(false);
				UpdateColorSliders(true);
			}
			else
			{
				SetColor(SelectedColor);

			}
		}
	}

	public void SetColor(Color color)
	{
		TempColor = color;
		if(txColorDisplay != null)
		{
			txColorDisplay.SetPixel(0, 0, color);
			txColorDisplay.Apply();
		}
	}

	public Color GetColor()
	{
		return TempColor;
	}

	public void SetTitle(string title, Color textColor)
	{
		this.Title = title;
		this.textColor = textColor;
	}

	public void ApplyColor()
	{
		SelectedColor = TempColor;
		if(receiver)
		{
			receiver.SendMessage(colorSetFunctionName, SelectedColor, SendMessageOptions.DontRequireReceiver);
		}
	}
}
