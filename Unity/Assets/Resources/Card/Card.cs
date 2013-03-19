using UnityEngine;
using System.Collections;

public enum CardLocation {
	Hand,
	Field,
}

public class Card : MonoBehaviour {
	
	const int MAX_LETTERS_WIDTH_NAME = 14;
	
	public string testName;
	public TextMesh _powerTouchness;
	
	private CardInfo _info;
	public Player _owner;
	public Player Owner {
		get {
			return _owner;
		}
		set {
			_owner = value;
		}
	}
	
	public CardLocation _location;
	public CardLocation Location {
		get {
			return _location;
		}
		set {
			_location = value;
		}
	}
	
	public Transform _wantedTransform;
	
	void Start () {
		if (!string.IsNullOrEmpty(testName)) Initialize(testName);
		_wantedTransform = transform;
	}
	
	void Update () {
		transform.position = Vector3.MoveTowards(transform.position, _wantedTransform.position, 10.0f * Time.deltaTime);
		transform.rotation = Quaternion.RotateTowards(transform.rotation, _wantedTransform.rotation, 90.0f * Time.deltaTime);
	}
	
	public void Zoom () {
		_wantedTransform = Player.Players[0].ZoomLocation;
		_powerTouchness.gameObject.active = false;
	}
	
	public void Initialize(string name) {
		if (_info != null) {
			if (name == _info.Name) return;
			_info.Updated -= UpdateInfo;
		}
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
