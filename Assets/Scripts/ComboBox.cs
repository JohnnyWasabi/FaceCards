/*
 * 
// Popup list created by Eric Haines
// ComboBox Extended by Hyungseok Seo.(Jerry) sdragoon@nate.com
// Refactored by zhujiangbo jumbozhu@gmail.com
// Slight edit for button to show the previously selected item AndyMartin458 www.clubconsortya.blogspot.com
// 
// -----------------------------------------------
// This code working like ComboBox Control.
// I just changed some part of code, 
// because I want to seperate ComboBox button and List.
// ( You can see the result of this code from Description's last picture )
// -----------------------------------------------
//
// === usage ======================================
using UnityEngine;
using System.Collections;
 
public class ComboBoxTest : MonoBehaviour
{
	GUIContent[] comboBoxList;
	private ComboBox comboBoxControl;// = new ComboBox();
	private GUIStyle listStyle = new GUIStyle();
 
	private void Start()
	{
		comboBoxList = new GUIContent[5];
		comboBoxList[0] = new GUIContent("Thing 1");
		comboBoxList[1] = new GUIContent("Thing 2");
		comboBoxList[2] = new GUIContent("Thing 3");
		comboBoxList[3] = new GUIContent("Thing 4");
		comboBoxList[4] = new GUIContent("Thing 5");
 
		listStyle.normal.textColor = Color.white; 
		listStyle.onHover.background =
		listStyle.hover.background = new Texture2D(2, 2);
		listStyle.padding.left =
		listStyle.padding.right =
		listStyle.padding.top =
		listStyle.padding.bottom = 4;
 
		comboBoxControl = new ComboBox(new Rect(50, 100, 100, 20), comboBoxList[0], comboBoxList, "button", "box", listStyle);
	}
 
	private void OnGUI () 
	{
		comboBoxControl.Show();
	}
}
 
*/


using UnityEngine;

public class ComboBox
{
	private static bool forceToUnShow = false;
	private static int useControlID = -1;
	private bool isClickedComboButton = false;
	private int selectedItemIndex = 0;

	private Rect rect;
    private Rect rectTriangle;
	private GUIContent buttonContent;
	private GUIContent blankText = new GUIContent(" ");
	private GUIContent[] listContent;
	private string boxStyle;
	private GUIStyle listStyle;
    private GUIStyle dropDownButtonStyle;
    public bool isBoxAbove = true;
    private GUIStyle triangleStyle;
	public bool isComboBoxOpen {  get { return isClickedComboButton; } }
	public Rect rectPosition {  get { return rect; } }
	public string comboLabel;
	private bool _isFlashedOff = false;
	public bool isFlashedOff {  get { return _isFlashedOff; } }
	private float secondsFlashing = 0;
	private float timeFlashingStarted = 0;
	private float secondsFlashInterval = 0.10f;

	private bool _isLabelFlashedOff = false;
	public bool isLabelFlashedOff { get { return _isFlashedOff; } }
	private float secondsLabelFlashing = 0;
	private float timeLabelFlashingStarted = 0;
	private float secondsLabelFlashInterval = 0.10f;

	public ComboBox(Rect rect, GUIContent buttonContent, GUIContent[] listContent, GUIStyle listStyle, string comboLabel = "")
	{
		this.rect = rect;
		this.buttonContent = buttonContent;
		this.listContent = listContent;
		this.boxStyle = "box";
		this.listStyle = listStyle;
		this.comboLabel = comboLabel;

		this.dropDownButtonStyle = new GUIStyle("button");
        this.dropDownButtonStyle.alignment = TextAnchor.MiddleLeft;
        triangleStyle = new GUIStyle("label");
        triangleStyle.alignment = TextAnchor.MiddleRight;
        Reposition(rect);
    }

    public ComboBox(Rect rect, GUIContent buttonContent, GUIContent[] listContent, string buttonStyle, string boxStyle, GUIStyle listStyle, string comboLabel = "")
	{
		this.rect = rect;
		this.buttonContent = buttonContent;
		this.listContent = listContent;
		this.boxStyle = boxStyle;
		this.listStyle = listStyle;
		this.comboLabel = comboLabel;

        this.dropDownButtonStyle = new GUIStyle("button");
        this.dropDownButtonStyle.alignment = TextAnchor.MiddleLeft;
        triangleStyle = new GUIStyle("label");
        triangleStyle.alignment = TextAnchor.MiddleRight;

        Reposition(rect);
    }
	public void FlashButtonText(float secondsDuration = 0.75f, float interval = 0.10f)
	{
		this.secondsFlashing = secondsDuration;
		secondsFlashInterval = interval;
		timeFlashingStarted = Time.time;
	}
	public void FlashLabelText(float secondsDuration = 0.75f, float interval = 0.10f)
	{
		this.secondsLabelFlashing = secondsDuration;
		secondsLabelFlashInterval = interval;
		timeLabelFlashingStarted = Time.time;
	}
	public void UpdateContent(GUIContent buttonContent, GUIContent[] listContent)
	{
		this.buttonContent = buttonContent;
		this.listContent = listContent;
	}
    public void Reposition(Rect rect)
	{
		this.rect = rect;
        rectTriangle = rect;
        rectTriangle.width -= 4;
    }
    public int Show()
	{
		if (forceToUnShow)
		{
			forceToUnShow = false;
			isClickedComboButton = false;
		}

		bool done = false;
		int controlID = GUIUtility.GetControlID(FocusType.Passive);

		switch (Event.current.GetTypeForControl(controlID))
		{
		case EventType.mouseUp:
			{
				if (isClickedComboButton)
				{
					done = true;
				}
			}
			break;
		}

		if (secondsFlashing > 0)
		{
			float elapsedTime = Time.time - timeFlashingStarted;
			if (elapsedTime > secondsFlashing)
			{
				_isFlashedOff = false;
				secondsFlashing = 0;
			}
			else
			{
				int intervalIndex = (int)(elapsedTime / secondsFlashInterval);
				_isFlashedOff = (intervalIndex & 1) == 0;
			}
		}

		if (GUI.Button(rect, (_isFlashedOff) ? blankText :  buttonContent, dropDownButtonStyle))
		{
			if (useControlID == -1)
			{
				useControlID = controlID;
				isClickedComboButton = false;
			}

			if (useControlID != controlID)
			{
				forceToUnShow = true;
				useControlID = controlID;
			}
			isClickedComboButton = true;
		}

        GUI.Label(rectTriangle, (isBoxAbove) ? "▲" : "▼", triangleStyle);

		if (isClickedComboButton)
		{
			float itemHeight = listStyle.CalcHeight(listContent[0], 1.0f);
			float listBoxHeight = itemHeight * listContent.Length;
			float listBoxY = (rect.y + itemHeight + listBoxHeight > Screen.height) ? rect.y - listBoxHeight : rect.y + itemHeight;		// make it appear below button, unless goes of bottom of screen, in which case position above button.

			Rect listRect = new Rect(rect.x, listBoxY,
					  rect.width, listBoxHeight);

			GUI.Box(listRect, "", boxStyle);
			int newSelectedItemIndex = GUI.SelectionGrid(listRect, selectedItemIndex, listContent, 1, listStyle);
			if (newSelectedItemIndex != selectedItemIndex)
			{
				selectedItemIndex = newSelectedItemIndex;
				buttonContent = listContent[selectedItemIndex];
			}
		}

 
        if (done)
			isClickedComboButton = false;

		if (secondsLabelFlashing > 0)
		{
			float elapsedTime = Time.time - timeLabelFlashingStarted;
			if (elapsedTime > secondsLabelFlashing)
			{
				_isLabelFlashedOff = false;
				secondsLabelFlashing = 0;
			}
			else
			{
				int intervalIndex = (int)(elapsedTime / secondsLabelFlashInterval);
				_isLabelFlashedOff = (intervalIndex & 1) == 0;
			}
		}
		if (!string.IsNullOrEmpty(comboLabel) && (!isComboBoxOpen || !isBoxAbove) && !_isLabelFlashedOff)
		{
			Rect rectLabel = rectPosition;
			rectLabel.y -= 20;
			rectLabel.x += 6;
			GUI.Label(rectLabel, comboLabel);
		}

		return selectedItemIndex;
	}

	public int SelectedItemIndex
	{
		get
		{
			return selectedItemIndex;
		}
		set
		{
			selectedItemIndex = value;
			buttonContent = listContent[selectedItemIndex];
		}
	}
}
