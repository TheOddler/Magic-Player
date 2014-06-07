using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GameState {
	Game,
	Zoomed,
}

public class Player : NetworkMonobehaviour {
	
	public Card _cardPrefab;
	public List<string> _testCards;
	
	public LayerMask _cardLayerMask;
	
	int _playerNumber = -1; //-1 means observer or not yet set (default). Anything else means the actual player number.
	public int PlayerNumber {
		get {
			return _playerNumber;
		}
	}
	
	PlayerSeat _seat;
	public PlayerSeat Seat {
		get {
			return _seat;
		}
	}
	
	void Awake () {
		InitializeNetworking();
		PlayersManager.Instance.RegisterPlayer(this);
	}

	void Start () { 
		if (IsMine) {
			RequestPlayerNumber();
		}
	}
	
	void OnDestroy() {
		PlayersManager.Instance.UnregisterPlayer(this);
	}
	
	void Update () {
		if (IsMine) {
			foreach (var input in SmartInput.InputInfo) {
				if (input.action == InputAction.SingleClick) {
					var card = NetworkInstantiate<Card>(_cardPrefab, Vector3.zero, Quaternion.identity);
					card.Initialize(_testCards.RandomElement(), _playerNumber);
				}
			}
		}
	}
	
	void OnPlayerConnected(NetworkPlayer player) {
		CallRemote(player, DoSetPlayerNumber, _playerNumber);
	}
	
	void OnPhotonPlayerConnected (PhotonPlayer player) {
		CallRemote(player, DoSetPlayerNumber, _playerNumber);
	}
	
	
	//
	// Player Number
	// -------------------------------
	void RequestPlayerNumber() {
		CallRemote (CallMode.Server, DoRequestPlayerNumber);
	}
	[RPC]
	void DoRequestPlayerNumber() {
		if (!IsServer) throw new UnityException("This should only be called on the server since it's the server that manages the player numbers.");
		
		PlayersManager.Instance.RequestNumberFor(this);
	}
	
	public void SetPlayerNumber(int number) {
		DoSetPlayerNumber(number);
		CallRemote(CallMode.Others, DoSetPlayerNumber, number);
	}
	[RPC]
	void DoSetPlayerNumber(int number) {
		_playerNumber = number;
		name = "Player " + number;
		
		if (number >= 0 && IsMine) {
			// If after requesting a number it turns out you're a player, and not an observer
			TakeSeat();
		}
		else {
			if (_seat != null) {
				_seat.LeaveSeat();
			}
			_seat = null;
		}
	}
	
	//
	// Take Seat
	// ----------------------------
	void TakeSeat() {
		// Set the correct camera
		_seat = PlayersManager.Instance.PlayerSeats[_playerNumber];
		_seat.TakeThisSeat();
	}
}
