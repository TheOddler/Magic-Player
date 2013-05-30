using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GameState {
	Game,
	Zoomed,
}

public class Player : MonoBehaviour {
	
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
	
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			Ray ray = _playerCam.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit)) {
				var cardComp = hit.transform.GetComponent<Card>();
				if (cardComp != null) {
					cardComp.Zoom();
				}
			}
		}
	}
}