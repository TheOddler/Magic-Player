using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GameState {
	Game,
	Zoomed,
}

public class Player : Photon.MonoBehaviour {
	
	private static List<Player> _players = new List<Player>();
	public static List<Player> Players {
		get {
			return _players;
		}
	}
	
	public Transform _zoomLocation;
	public Transform ZoomLocation {
		get {
			return this._zoomLocation;
		}
	}
	
	public Camera _playerCam;
	
	void Awake () {
		_players.Add(this);
	}

	void Start () {
		if (photonView.isMine) {
			Debug.Log ("Player start");
			Camera.main.gameObject.SetActive (false);
			_playerCam.gameObject.SetActive (true);
		}
		else {
			_playerCam.gameObject.SetActive (false);
		}
	}
	
	void Update () {
		if (photonView.isMine) {
			
		}
	}
}
