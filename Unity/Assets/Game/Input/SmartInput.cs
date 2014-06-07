using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum RawInputPhase {
	Began,
	Stationary,
	Moved,
	Ended,
}

public class RawInputInfo {
	public int id;
	
	public float timeBegin;
	public float timeLastUpdate = -1;
	public float timeDelta = 0;
	public float timeEnded = -1;
	
	public Vector2 positionBegin;
	public Vector2 positionEnded;
	public Vector2 position;
	public Vector2 positionDelta = Vector2.zero;
	public float totalMoveDistance;
	
	public RawInputPhase phase;
	public RawInputPhase phasePrevious;
	
	public int count = 0;
	
	public bool beingUsed = false;
	public bool removed = false;
}

public enum InputAction {
	SingleClick,
	DoubleClick,
	TripleClick,
}

public struct InputInfo {
	public InputInfo(InputAction action, Vector2 pos, Vector2 deltaPos, RawInputInfo creator) {
		this.action = action;
		position = pos;
		deltaPosition = deltaPos;
		
		correspondingRawInput = creator;
	}

	public InputAction action;
	public Vector2 position;
	public Vector2 deltaPosition;
	
	public RawInputInfo correspondingRawInput;
}

public class SmartInput: MonoBehaviour {
	
	const int MOUSE_BUTTON0_ID = -1000;
	const int MOUSE_BUTTON1_ID = -1001;
	const int MOUSE_BUTTON2_ID = -1002;
	
	const float MAX_POS_DIFF = 3.0f; //pixels
	const float MOVED_THRESHOLD = 0.5f; //frame by frame (in pixels)
	const float WAIT_TO_REMOVE = 0.5f; //seconds

	static SmartInput _instance;
	
	List<RawInputInfo> _rawInputs = new List<RawInputInfo>();
	public static List<RawInputInfo> RawInputInfo {
		get {
			return _instance._rawInputs;
		}
	}
	public static IEnumerable<RawInputInfo> RawInputInfoUnused {
		get {
			return _instance._rawInputs.Where(input=> !input.beingUsed);
		}
	}
	
	List<InputInfo> _inputInfo = new List<InputInfo>();
	public static List<InputInfo> InputInfo {
		get {
			return _instance._inputInfo;
		}
	}

	// Use this for initialization
	void Start () {
		if (_instance != null) throw new UnityException("More than one SmartInput object in the scene. Please make sure there's exactly one.");
		_instance = this;
	}
	
	void OnLevelWasLoaded(int level) {
		_rawInputs.Clear();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			AddInput(MOUSE_BUTTON0_ID, Input.mousePosition);
		}
		if (Input.GetMouseButtonDown(1)) {
			AddInput(MOUSE_BUTTON1_ID, Input.mousePosition);
		}
		if (Input.GetMouseButtonDown(2)) {
			AddInput(MOUSE_BUTTON2_ID, Input.mousePosition);
		}
		
		_inputInfo.Clear(); //will be populated when updating the raw input.
		
		_rawInputs.RemoveAll(input=> UpdateInput(input));
		
		
		
		
		
		
		foreach (var rawInput in RawInputInfoUnused) {
			Ray ray = PlayersManager.Instance.LocalPlayer.Seat.Camera.ScreenPointToRay(rawInput.position);
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo)) {
				ISmartInputListener listener = hitInfo.transform.GetComponent(typeof(ISmartInputListener)) as ISmartInputListener;
				if (listener != null) {
					listener.HandleRawInput(rawInput, hitInfo);
				}
			}
		}
		
		foreach (var input in _inputInfo) {
			Ray ray = PlayersManager.Instance.LocalPlayer.Seat.Camera.ScreenPointToRay(input.position);
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo)) {
				ISmartInputListener listener = hitInfo.transform.GetComponent(typeof(ISmartInputListener)) as ISmartInputListener;
				if (listener != null) {
					listener.HandleInput(input, hitInfo);
				}
			}
		}
	}
	
	bool UpdateInput(RawInputInfo input) {
		int mouseIndex = MouseIndexByID(input.id);
		if (mouseIndex >= 0) {
			return UpdateMouseInput(input);
		}
		else {
			//TODO: Not mouse input, not supported yet
			return true;
		}
	}
	 
	// returns true if it should be removed
	bool UpdateMouseInput(RawInputInfo input) {
		int mouseIndex = MouseIndexByID(input.id);
		
		input.phasePrevious = input.phase;
		
		input.timeDelta = Time.timeSinceLevelLoad - input.timeLastUpdate;
		input.timeLastUpdate = Time.timeSinceLevelLoad;
		
		Vector2 mousePos = Input.mousePosition;
		input.positionDelta = mousePos - input.position;
		input.position = mousePos;
		
		input.totalMoveDistance += input.positionDelta.magnitude;
		
		if (Input.GetMouseButtonDown(mouseIndex)) {
			input.phase = RawInputPhase.Began;
			input.count += 1;
		}
		else if (Input.GetMouseButton(mouseIndex)) { //Holding mouse
			if (input.positionDelta.sqrMagnitude > MOVED_THRESHOLD * MOVED_THRESHOLD) {
				input.phase = RawInputPhase.Moved;
			}
			else {
				input.phase = RawInputPhase.Stationary;
			}
		}
		else if (Input.GetMouseButtonUp(mouseIndex)) {
			input.phase = RawInputPhase.Ended;
			input.timeEnded = Time.timeSinceLevelLoad;
			input.positionEnded = mousePos;
		}
		else {
			//Mouse is up, do nothing extra here
		}
		
		
		
		
		
		
		//Populate the non-raw input info
		if (input.phase == RawInputPhase.Ended ) {
			if ( input.timeEnded + WAIT_TO_REMOVE < Time.timeSinceLevelLoad || input.totalMoveDistance > MAX_POS_DIFF ){
				if ((input.positionEnded - input.positionBegin).sqrMagnitude <= MAX_POS_DIFF * MAX_POS_DIFF) {
					switch(input.count) {
					case 1:
						_inputInfo.Add(new InputInfo(InputAction.SingleClick, input.positionBegin, Vector2.zero, input));
						break;
					case 2:
						_inputInfo.Add(new InputInfo(InputAction.DoubleClick, input.positionBegin, Vector2.zero, input));
						break;
					case 3:
						_inputInfo.Add(new InputInfo(InputAction.TripleClick, input.positionBegin, Vector2.zero, input));
						break;
					}
				}
				return true;
			}
			else {
				return false;
			}
		}
		
		return false;
	}
	
	RawInputInfo GetInputByID(int id) {
		return _rawInputs.FirstOrDefault(i=>i.id == id);
	}
	
	void AddInput(int id, Vector2 pos) {
		if (_rawInputs.Any(i=>i.id == id)) {
			return;
		}
	
		var input = new RawInputInfo();
		input.id = id;
		/*input.phase =*/ input.phase = /*input.phasePrevious =*/ input.phasePrevious = RawInputPhase.Began;
		input.timeBegin = input.timeLastUpdate = Time.timeSinceLevelLoad;
		input.positionBegin = input.position = pos;
		
		_rawInputs.Add (input);
	}
	
	static int MouseIndexByID(int id) {
		return
			id == MOUSE_BUTTON0_ID ? 0 :
			id == MOUSE_BUTTON1_ID ? 1 :
			id == MOUSE_BUTTON2_ID ? 2 :
			-1;
	}
	
	
	
	
	
	
	void OnGUI () {
		foreach(var input in _rawInputs) {
			GUILayout.Label("Raw Input: ");
			GUILayout.Label("id: " +				input.id);
			
			GUILayout.Label("timeBegin: " +			input.timeBegin);
			GUILayout.Label("timeLastUpdate: " +	input.timeLastUpdate);
			GUILayout.Label("timeDelta: " +			input.timeDelta);
			GUILayout.Label("timeEnded: " +			input.timeEnded);

			GUILayout.Label("position: " +			input.position);
			GUILayout.Label("positionDelta: " +		input.positionDelta);

			//GUILayout.Label("phase: " +				input.phase);
			//GUILayout.Label("phasePrevious: " +		input.phasePrevious);
			GUILayout.Label("phaseFrameByFrame: " +	input.phase);
			GUILayout.Label("phasePreviousFrameByFrame: " +	input.phasePrevious);

			GUILayout.Label("count" +				input.count);
		}
	}
}
