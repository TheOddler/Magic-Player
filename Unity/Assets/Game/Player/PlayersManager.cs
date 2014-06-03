using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayersManager : NetworkMonobehaviour {

	public const int MAX_PLAYERS = 2; //any aditional will become observers

	static PlayersManager _instance;
	public static PlayersManager Instance { get { return _instance; } }
	
	public Player _playerPrefab;
	
	public Camera _overviewCamera;
	public Camera OverviewCamera {
		get {
			return _overviewCamera;
		}
	}

	public List<PlayerSeat> _playerSeats;
	public List<PlayerSeat> PlayerSeats {
		get {
			return _playerSeats;
		}
	}

	List<Player> _players = new List<Player>();
	
	Player _localPlayer;
	public Player LocalPlayer {
		get {
			return _localPlayer;
		}
	}
	
	void Awake () {
		InitializeNetworking();
	
		if (_instance != null) throw new UnityException("Creating more than one CardImageManager");
		_instance = this;
	}
	
	void Start () {
		if (_playerSeats.Count < MAX_PLAYERS) throw new UnityException("There aren't enough seats for all players.");
	}
	
	
	//
	// (Un)Register
	// --------------------------------
	public void RegisterPlayer(Player player) {
		if (!_players.Contains(player)) {
			_players.Add(player);
		}
	}
	public void UnregisterPlayer(Player player) {
		if (_players.Contains(player)) {
			_players.Remove(player);
		}
	}
	
	
	
	//
	// Unity Networking
	// --------------------------------------
	void OnServerInitialized() {
		InstantiatePlayer();
	}
	void OnConnectedToServer() {
		InstantiatePlayer();
	}
	
	void OnPlayerDisconnected(NetworkPlayer networkPlayer) {
		var player = _players.Find(pl=>pl.NetworkPlayer == networkPlayer);
		NetworkDestroy(player);
	}
	
	
	
	//
	// Photon
	// -----------------------
	void OnJoinedRoom() {
		InstantiatePlayer();
	}
	
	void OnPhotonPlayerDisconnected(PhotonPlayer networkPlayer) {
		// The PhotonPlayer will already be removed, so the owner of the leaving player's player object will be null.
		var player = _players.Find(pl=>pl.PhotonPlayer == networkPlayer || pl.PhotonPlayer == null);
		NetworkDestroy(player);
	}
	
	
	
	
	//
	// Shared
	// ------------------------
	void InstantiatePlayer() {
		_localPlayer = NetworkInstantiate<Player>(_playerPrefab, Vector3.zero, Quaternion.identity);
	}
	
	
	//
	// Player Numbers
	// ----------------------------------
	public void RequestNumberFor(Player player) {
		// Find a free spot
		for (int i = 0; i < MAX_PLAYERS; ++i) {
			if (_players.All(pl=>pl.PlayerNumber != i)) {
				player.SetPlayerNumber(i);
				return;
			}
		}
		//We got more than the max number of players, any aditional players will be desginated observers and they get number -1
		player.SetPlayerNumber(-1);
	}
}
