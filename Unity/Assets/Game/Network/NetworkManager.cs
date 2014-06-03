using UnityEngine;
using System.Collections;

public class NetworkManager : NetworkMonobehaviour {

	public string _version = "0.0.1prealpha";
	public string _roomName = "Room";

	// Use this for initialization
	void Start () {
		PhotonNetwork.logLevel = PhotonLogLevel.ErrorsOnly;
		PhotonNetwork.autoCleanUpPlayerObjects = false;
		
		PhotonNetwork.ConnectUsingSettings(_version);
		//bool useNat = !Network.HavePublicAddress();
		//Network.InitializeServer(32, 25000, useNat);
	}
	
	// Update is called once per frame
	void Update () {
	}

	void OnJoinedLobby() {
		Debug.Log ("OnJoinedLobby");
		PhotonNetwork.JoinRandomRoom();
	}

	void OnPhotonRandomJoinFailed() {
		Debug.Log ("OnPhotonRandomJoinFailed");
		PhotonNetwork.CreateRoom(_roomName);
	}

	void OnGUI() {
		GUILayout.Label("Photon: " + PhotonNetwork.connectionStateDetailed.ToString() + "\tUNet: " + Network.peerType.ToString());
	}


}
