using UnityEngine;
using System.Collections;

public class PlayerSeat : MonoBehaviour {

	public Camera _camera;
	public Camera Camera {
		get {
			return _camera;
		}
	}

	// Use this for initialization
	void Start () {
		DisableCamera();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void DisableCamera() {
		_camera.gameObject.SetActive(false);
	}
	public void EnableCamera() {
		_camera.gameObject.SetActive(true);
	}
	
	public void TakeThisSeat() {
		var playerManager = PlayersManager.Instance;
		playerManager.OverviewCamera.gameObject.SetActive(false);
		foreach (var seat in playerManager.PlayerSeats) {
			seat.DisableCamera();
		}
		EnableCamera();
	}
}
