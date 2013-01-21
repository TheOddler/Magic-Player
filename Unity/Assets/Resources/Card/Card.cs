using UnityEngine;
using System.Collections;

public class Card : MonoBehaviour {
	
	public string _name = "Frenetic Raptor";
	
	void Start () {
		renderer.material = CardInfoManager.Instance.CardInfo[_name].ImageMaterial;
	}
}
