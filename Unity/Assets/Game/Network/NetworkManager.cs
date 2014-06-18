using UnityEngine;
using System.Net;
using System.Collections;

public class NetworkManager : MonoBehaviour /*NetworkMonobehaviour*/ {

	public string _version = "0.0.1prealpha";
	public string _roomName = "Room";
	
	public string _localIP = "192.168.1.6";
	
	RoomInfo[] _roomsList;
	string _newRoomName = "New Room";

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(gameObject);
	
		PhotonNetwork.logLevel = PhotonLogLevel.ErrorsOnly;
		PhotonNetwork.autoCleanUpPlayerObjects = false;
		
		PhotonNetwork.ConnectUsingSettings(_version);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void UpdateRoomInfo () {
		if (PhotonNetwork.insideLobby) {
			_roomsList = PhotonNetwork.GetRoomList();
		}
	}

	void OnJoinedLobby() {
		Debug.Log ("OnJoinedLobby");
		UpdateRoomInfo();
		InvokeRepeating("UpdateRoomInfo", 5.0f, 5.0f);
	}

	void OnPhotonRandomJoinFailed() {
		Debug.Log ("OnPhotonRandomJoinFailed");
	}
	
	void OnJoiningGame() {
		Application.LoadLevel("Game");
	}

	void OnGUI() {
		GUILayout.Label("Photon: " + PhotonNetwork.connectionStateDetailed.ToString() + "\tUNet: " + Network.peerType.ToString());
		
		if (Application.loadedLevelName == "Menu") {
			// Photon
			if (PhotonNetwork.insideLobby && _roomsList != null) {
				foreach (var room in _roomsList) {
					if (GUILayout.Button(room.name + ": " + room.playerCount + "/" + room.maxPlayers + "; " + (room.open ? "Open" : "Closed"))) {
						PhotonNetwork.JoinRoom(room.name);
						OnJoiningGame();
					}
				}
				GUILayout.BeginHorizontal();
				_newRoomName = GUILayout.TextField(_newRoomName);
				if (GUILayout.Button("Create")) {
					PhotonNetwork.CreateRoom(_newRoomName);
					OnJoiningGame();
				}
				GUILayout.EndHorizontal();
			}
			
			GUILayout.Space(50);
			if (GUILayout.Button("Create local")) {
				Invoke("CreateLocalServer", 2.0f);
				OnJoiningGame();
			}
			GUILayout.BeginHorizontal();
			_localIP = GUILayout.TextField(_localIP);
			if (GUILayout.Button("Connect")) {
				Invoke("JoinLocalServer", 2.0f);
				OnJoiningGame();
			}
			GUILayout.EndHorizontal();
		}
		else if (Application.loadedLevelName == "Game") {
			if (PhotonNetwork.inRoom) {
				GUILayout.BeginHorizontal();
				GUILayout.Label("Connected to: " + PhotonNetwork.room.name);
				if (GUILayout.Button("Leave.")) {
					PhotonNetwork.LeaveRoom();
				}
				GUILayout.EndHorizontal();
			}
			
			if (Network.isServer) {
				GUILayout.Label("Hosting server: ");
				
				// Get host name
				string strHostName = Dns.GetHostName();
				// Find host by name
				IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);
				// Enumerate IP addresses
				int nIP = 0;
				foreach(IPAddress ipaddress in iphostentry.AddressList) {
					GUILayout.Label(ipaddress.ToString());
				}
			}
		}
	}
	
	void CreateLocalServer() {
		bool useNat = !Network.HavePublicAddress();
		Network.InitializeServer(32, 25000, useNat);
	}
	
	void JoinLocalServer() {
		Network.Connect(_localIP, 25000);
	}
}
