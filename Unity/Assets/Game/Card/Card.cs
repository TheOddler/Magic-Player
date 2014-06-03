using UnityEngine;
using System.Collections;

public enum CardLocation {
	Hand,
	Field,
}

public class Card : NetworkMonobehaviour {
	
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
	}

	public int _ownerNumber;
	public int OwnerNumber {
		get {
			return _ownerNumber;
		}
		set {
			_ownerNumber = value;
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
	
	
	bool _tapped = false;
	bool _flipped = false;
	bool _faceDown = false;
	
	
	void Start () {
		_wantedTransform = transform;
	}
	
	void Update () {
		transform.position = Vector3.MoveTowards(transform.position, _wantedTransform.position, 10.0f * Time.deltaTime);
		transform.rotation = Quaternion.RotateTowards(transform.rotation, _wantedTransform.rotation, 90.0f * Time.deltaTime);
	}
	
	void OnPlayerConnected(NetworkPlayer player) {
		CallRemote(player, SyncData, _name, _ownerNumber);
	}
	
	void OnPhotonPlayerConnected (PhotonPlayer player) {
		CallRemote(player, SyncData, _name, _ownerNumber);
	}
	
	public void Initialize(string name, int ownerNumber) {
		SyncData(name, ownerNumber);
		CallRemote(CallMode.Others, SyncData, name, ownerNumber);
	}
	[RPC]
	void SyncData(string name, int ownerNumber) {
		if (string.IsNullOrEmpty(name)) {
			throw new UnityException("Trying to initialize with invaled name");
		}
		
		//
		// Name
		// -----------------------------
		// Set name and get info based on it
		_name = name;
		if (_info != null) { //Make sure we aren't subscribed to the Update of our info twice.
			_info.Updated -= HandleInfoUpdated;
		}
		// (Re)Initialize
		_info = CardInfoManager.Instance.GetCardInfo(name);
		_info.Updated += HandleInfoUpdated;
		HandleInfoUpdated();
		
		//
		// Owner Number
		// -----------------------------
		_ownerNumber = OwnerNumber;
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
