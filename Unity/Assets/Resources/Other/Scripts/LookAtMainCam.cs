using UnityEngine;
using System.Collections;

public class LookAtMainCam : MonoBehaviour {
	
	void Update () {
		transform.rotation = Camera.main.transform.rotation; //= Quaternion.LookRotation(transform.position - Camera.main.transform.position, Vector3.up);
	}
	
}
