using UnityEngine;
using System.Collections;

public enum CardLocation {
	Hand,
	Field,
}

public class Card : Photon.MonoBehaviour {
	
	const int MAX_LETTERS_WIDTH_NAME = 14;
	
	//
	// Binding
	// ----------------------
	public Renderer _cardFrontRenderer;
	public TextMesh _powerToughness;
	
	//
	// Info
	// ----------------------
	private CardInfo _info;
	
	public string _name;
	public string Name {
		get {
			return _name;
		}
		set {
			if (_name != value) {
				_name = value;
				UpdateInfo();
			}
		}
	}

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
		if (!string.IsNullOrEmpty(_name)) UpdateInfo();
		_wantedTransform = transform;
	}
	
	void Update () {
		transform.position = Vector3.MoveTowards(transform.position, _wantedTransform.position, 10.0f * Time.deltaTime);
		transform.rotation = Quaternion.RotateTowards(transform.rotation, _wantedTransform.rotation, 90.0f * Time.deltaTime);
	}
	
	
	
	public void UpdateInfo() {
		if (string.IsNullOrEmpty(_name)) {
			throw new UnityException("Trying to update the card info without setting a _name");
		}
		
		if (_info != null) { // We already initialized once
			_info.Updated -= HandleInfoUpdated; // Different name so we need te reinitialize.
		}
		// (Re)Initialize
		_info = CardInfoManager.Instance.GetCardInfo(_name);
		_info.Updated += HandleInfoUpdated;
		HandleInfoUpdated();
	}
	
	public void HandleInfoUpdated() {
		_cardFrontRenderer.material = _info.ImageMaterial;

		if (string.IsNullOrEmpty (_info.Power) && string.IsNullOrEmpty (_info.Toughness)) {
			_powerToughness.text = "";
		}
		else {
			_powerToughness.text = _info.Power + "/" + _info.Toughness;
		}
	}
}
