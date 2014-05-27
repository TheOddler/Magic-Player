using UnityEngine;
using System.Collections;

public enum CardLocation {
	Hand,
	Field,
}

public class Card : Photon.MonoBehaviour {
	
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
	
	
	
	public void Initialize(string name) {
		if (_info != null) { // We already initialized once
			if (name == _info.Name) return; // Name is the same so info should be the same too
			else _info.Updated -= HandleInfoUpdated; // Different name so we need te reinitialize.
		}
		// (Re)Initialize
		_info = CardInfoManager.Instance.GetCardInfo(name);
		_info.Updated += HandleInfoUpdated;
		HandleInfoUpdated();
	}
	
	public void HandleInfoUpdated() {
		renderer.material = _info.ImageMaterial;

		if (string.IsNullOrEmpty (_info.Power) && string.IsNullOrEmpty (_info.Toughness)) {
			_powerTouchness.text = "";
		}
		else {
			_powerTouchness.text = _info.Power + "/" + _info.Toughness;
		}
	}
}
