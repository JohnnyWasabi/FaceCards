using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Pog : MonoBehaviour
{
	public CircleCard circleCard;
	private CircleText circleTextTop;
	private CircleText circleTextBottom;
	public TextMeshPro tmproTop;
	public TextMeshPro tmproBottom;

	void Awake()
	{
		circleTextTop = tmproTop.gameObject.GetComponent<CircleText>();
		circleTextBottom = tmproBottom.gameObject.GetComponent<CircleText>();
	}
	public void SetActive(bool active)
	{
		circleCard.gameObject.SetActive(active);
		tmproTop.gameObject.SetActive(active);
		tmproBottom.gameObject.SetActive(active);
	}
	public void SetTopText(string label)
	{
		tmproTop.text = label;
		circleTextTop.EncircleText();
	}
	public void SetBottomText(string label)
	{
		tmproBottom.text = label;
		circleTextBottom.EncircleText();
	}
	public void SetColors(Color32[] colorsNew)
	{
		circleCard.SetDonutMeshColors(colorsNew);
	}

}
