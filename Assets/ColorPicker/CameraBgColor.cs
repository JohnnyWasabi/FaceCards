using UnityEngine;
using System.Collections;

public class CameraBgColor : MonoBehaviour {

	const float bgDefaultR = 94f / 255f;
	const float bgDefaultG = 144 / 255f;
	const float bgDefaultB = 70 / 255f;

	public Camera camera;

	void Start()
	{
		Color bgColor = new Color(PlayerPrefs.GetFloat("bgColorR", bgDefaultR), PlayerPrefs.GetFloat("bgColorG", bgDefaultG), PlayerPrefs.GetFloat("bgColorB", bgDefaultB));
		camera.backgroundColor = bgColor;

	}
	void OnSetColor(Color color)
	{
		camera.backgroundColor = color;
		PlayerPrefs.SetFloat("bgColorR", color.r);
		PlayerPrefs.SetFloat("bgColorG", color.g);
		PlayerPrefs.SetFloat("bgColorB", color.b);
	}

	void OnGetColor(ColorPicker picker)
	{
		picker.NotifyColor(GetComponent<Camera>().backgroundColor);
	}

	void OnGetDefaultColor(ColorPicker picker)
	{
		Color bgColor = new Color( bgDefaultR, bgDefaultG, bgDefaultB);
		picker.NotifyColor(bgColor);
	}
}
