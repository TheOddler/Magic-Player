using UnityEngine;
using System.Collections;

public class NetworkManager : Photon.MonoBehaviour {

	const int PLAYER_GROUP = 0;

	public string _version = "0.0.1prealpha";
	public string _roomName = "Room";

	public GameObject _playerPrefab;

	// Use this for initialization
	void Start () {
		PhotonNetwork.ConnectUsingSettings(_version);
	}
	
	// Update is called once per frame
	void Update () {

	}

	void OnJoinedLobby() {
		Debug.Log( PhotonNetwork.JoinRandomRoom() );
	}

	void OnPhotonRandomJoinFailed() {
		Debug.Log ("OnPhotonRandomJoinFailed");
		PhotonNetwork.CreateRoom(_roomName);
	}

	void OnJoinedRoom() {
		Debug.Log ("OnJoinedRoom");
		PhotonNetwork.Instantiate(_playerPrefab.name, Vector3.zero, Quaternion.identity, PLAYER_GROUP);
	}

	void OnGUI() {
		GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
	}


}
