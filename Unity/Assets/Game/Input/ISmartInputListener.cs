using UnityEngine;
using System.Collections;

public interface ISmartInputListener {
	
	void HandleRawInput(RawInputInfo input, RaycastHit hitInfo);
	void HandleInput(InputInfo input, RaycastHit hitInfo);
	
}
