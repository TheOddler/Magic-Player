using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum RawInputPhase {
	Began,
	Stationary,
	Moved,
	Ended,
}

public class RawInputInfo {
	public int id;
	
	public float timeBegin;
	public float timeLastUpdate;
	public float timeDelta = 0;
	public float timeEnded = -1;
	
	public Vector2 position;
	public Vector2 positionDelta = Vector2.zero;
	
	public RawInputPhase phase;
	public RawInputPhase phasePrevious;
	public RawInputPhase phaseFrameByFrame;
	public RawInputPhase phasePreviousFrameByFrame;
	
	public int count;
}

public enum InputAction {
	Holding,
	Dragging,
	
	Click,
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
	
	const float MOVED_THRESHOLD = 2.0f; //pixels
	const float WAIT_TO_REMOVE = 0.5f; //seconds

	static SmartInput _instance;
	
	static List<RawInputInfo> _rawInputs = new List<RawInputInfo>();
	public static List<RawInputInfo> RawInputInfo {
		get {
			return _rawInputs;
		}
	}
	
	static List<InputInfo> _inputInfo = new List<InputInfo>();
	public static List<InputInfo> InputInfo {
		get {
			return _inputInfo;
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
			var input = GetOrCreateInput(MOUSE_BUTTON0_ID);
			input.position = Input.mousePosition;
		}
		if (Input.GetMouseButtonDown(1)) {
			var input = GetOrCreateInput(MOUSE_BUTTON1_ID);
			input.position = Input.mousePosition;
		}
		if (Input.GetMouseButtonDown(2)) {
			var input = GetOrCreateInput(MOUSE_BUTTON2_ID);
			input.position = Input.mousePosition;
		}
		
		_inputInfo.Clear(); //will be populated when updating the raw input.
		
		foreach (var input in _rawInputs) {
			UpdateInput(input);
		}
		
		_rawInputs.RemoveAll(input=> input.phase == RawInputPhase.Ended && input.timeEnded + WAIT_TO_REMOVE < Time.timeSinceLevelLoad);
	}
	
	void UpdateInput(RawInputInfo input) {
		int mouseIndex = MouseIndexByID(input.id);
		if (mouseIndex >= 0) {
			UpdateMouseInput(input);
		}
		else {
			//TODO: Not mouse input, not supported yet
		}
	}
	
	void UpdateMouseInput(RawInputInfo input) {
		int mouseIndex = MouseIndexByID(input.id);
		
		input.phasePreviousFrameByFrame = input.phase;
		var phaseBackup = input.phase;
		
		if (input.phase == RawInputPhase.Began) {
			input.phase = RawInputPhase.Stationary;
		}
		
		input.timeDelta = Time.timeSinceLevelLoad - input.timeLastUpdate;
		input.timeLastUpdate = Time.timeSinceLevelLoad;
		
		Vector2 mousePos = Input.mousePosition;
		input.positionDelta = mousePos - input.position;
		input.position = mousePos;
		
		if (Input.GetMouseButtonDown(mouseIndex)) {
			input.phase = RawInputPhase.Began;
			input.phaseFrameByFrame = RawInputPhase.Began;
			input.count += 1;
		}
		else if (Input.GetMouseButton(mouseIndex)) { //Holding mouse
			if (input.positionDelta.sqrMagnitude > MOVED_THRESHOLD * MOVED_THRESHOLD) {
				input.phase = RawInputPhase.Moved;
				input.phaseFrameByFrame = RawInputPhase.Moved;
			}
			else {
				input.phaseFrameByFrame = RawInputPhase.Stationary;
			}
		}
		else if (Input.GetMouseButtonUp(mouseIndex)) {
			input.phaseFrameByFrame = RawInputPhase.Ended;
			input.phase = RawInputPhase.Ended;
			input.timeEnded = Time.timeSinceLevelLoad;
		}
		else {
			//Mouse is up, do nothing extra here
		}
		
		if (phaseBackup != input.phase) {
			input.phasePrevious = phaseBackup;
		}
		
		//Populate the non-raw input info
		if (input.phasePrevious != RawInputPhase.Ended && input.phase == RawInputPhase.Ended
		    && input.timeEnded + WAIT_TO_REMOVE < Time.timeSinceLevelLoad
			) {
			switch(input.count) {
			case 1:
				_inputInfo.Add(new InputInfo(InputAction.Click, input.position, input.positionDelta, input));
				break;
			case 2:
				_inputInfo.Add(new InputInfo(InputAction.DoubleClick, input.position, input.positionDelta, input));
				break;
			case 3:
				_inputInfo.Add(new InputInfo(InputAction.TripleClick, input.position, input.positionDelta, input));
				break;
			}
		}
		else if (input.phase == RawInputPhase.Stationary) {
			_inputInfo.Add(new InputInfo(InputAction.Holding, input.position, input.positionDelta, input));
		}
		else if (input.phase == RawInputPhase.Moved) {
			_inputInfo.Add(new InputInfo(InputAction.Dragging, input.position, input.positionDelta, input));
		}
	}
	
	static RawInputInfo GetInputByID(int id) {
		return _rawInputs.Find(i=>i.id == id);
	}
	
	static RawInputInfo CreateInput(int id) {
		var input = new RawInputInfo();
		input.id = id;
		input.phase = input.phaseFrameByFrame = input.phasePrevious = RawInputPhase.Began;
		input.timeBegin = input.timeLastUpdate = Time.timeSinceLevelLoad;
		
		_rawInputs.Add (input);
		
		return input;
	}
	
	static RawInputInfo GetOrCreateInput(int id) {
		var input = GetInputByID(id);
		
		if (input == null) {
			input = CreateInput(id);
		}
		
		return input;
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

			GUILayout.Label("phase: " +				input.phase);
			GUILayout.Label("phasePrevious: " +		input.phasePrevious);
			GUILayout.Label("phaseFrameByFrame: " +	input.phaseFrameByFrame);
			GUILayout.Label("phasePreviousFrameByFrame: " +	input.phasePreviousFrameByFrame);

			GUILayout.Label("count" +				input.count);
		}
	}
}
