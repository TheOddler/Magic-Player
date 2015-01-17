using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum CardLocation {
	Hand,
	Field,
}

public class Card : NetworkMonobehaviour, ISmartInputListener {
	const int MAX_LETTERS_WIDTH_NAME = 14;
	
	//
	// Binding
	// ----------------------
	public Renderer _cardFrontRenderer;
	public TextMesh _powerToughness;
	public Collider _collider;
	public LayerMask _fieldGroundMask;
	
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
	
	RawInputInfo _draggingInputInfo;
	Vector3 _draggingOffset;
	
	bool _tapped = false;
	bool _flipped = false;
	bool _faceDown = false;
	
	
	void Start () {
		if (!string.IsNullOrEmpty(_name)) {
			Initialize(_name, -1);
		}
	}
	
	void Update () {
		if (_location == CardLocation.Field) {
			if (_draggingInputInfo != null /*&& _draggingInputInfo.phase == RawInputPhase.Moved*/) {
				Ray ray = PlayersManager.Instance.LocalPlayer.Seat.Camera.ScreenPointToRay(_draggingInputInfo.position);
				RaycastHit hitInfo;
				if (Physics.Raycast(ray, out hitInfo, float.PositiveInfinity, _fieldGroundMask)) {
				
					float cardThickness = _collider.bounds.size.y;
					Vector3 cardThicknessOffset = ray.direction / ray.direction.y * cardThickness;
					
					transform.position = hitInfo.point + _draggingOffset + cardThicknessOffset;
				}
				
				if (_draggingInputInfo.phase == RawInputPhase.Ended) {
					_draggingInputInfo.beingUsed = false;
					_draggingInputInfo = null;
					CallRemote(CallMode.Others, FinishMove, transform.position);
				}
			}
		}
	}
	
	public void HandleRawInput (RawInputInfo input, RaycastHit hitInfo) {
		if (input.phase == RawInputPhase.Began) {
			_draggingInputInfo = input;
			_draggingInputInfo.beingUsed = true;
			_draggingOffset = transform.position - hitInfo.point;
		}
	}
	
	public void HandleInput (InputInfo input, RaycastHit hitInfo) {
		
	}
	
	public void HandleStartHoover() {
		
	}
	public void HandleEndHoover() {
		
	}
	
	[RPC]
	void FinishMove(Vector3 pos) {
		transform.position = pos;
	}
	
	
	
	
	
	void OnPlayerConnected(NetworkPlayer player) {
		CallRemote(player, SyncData, _name, _ownerNumber, transform.position);
	}
	
	void OnPhotonPlayerConnected (PhotonPlayer player) {
		CallRemote(player, SyncData, _name, _ownerNumber, transform.position);
	}
	
	void OnDestroy() {
		_info.Updated -= HandleInfoUpdated;
	}
	
	
	
	
	
	public void Initialize(string name, int ownerNumber) {
		SyncData(name, ownerNumber, transform.position);
		CallRemote(CallMode.Others, SyncData, name, ownerNumber, transform.position);
	}
	[RPC]
	void SyncData(string name, int ownerNumber, Vector3 pos) {
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
		
		//
		// Position
		// -----------------------------
		transform.position = pos;
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
