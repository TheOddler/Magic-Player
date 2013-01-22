using UnityEngine;
using System.Collections;

public class Card : MonoBehaviour {
	
	const int MAX_LETTERS_WIDTH_NAME = 14;
	
	public string testName;
	
	public TextMesh _powerTouchness;
	
	private CardInfo _info;
	
	void Start () {
		if (!string.IsNullOrEmpty(testName)) SetInfo(testName);
	}
	
	public void SetInfo(string name) {
		if (_info != null) _info.Updated -= UpdateInfo;
		_info = CardInfoManager.Instance.GetCardInfo(name);
		_info.Updated += UpdateInfo;
		UpdateInfo();
	}
	
	public void UpdateInfo() {
		renderer.material = _info.ImageMaterial;
		
		_powerTouchness.text =
			_info.Power != CardInfo.UNUSED_VALUE && _info.Toughness != CardInfo.UNUSED_VALUE ?
			(_info.Power + "/" + _info.Toughness) : " ";
	}
}
