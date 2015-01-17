using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using MiniJSON;

public class LocalMtgJson : MonoBehaviour {
	
	private Dictionary<string, CardInfo> _cardInfo = new Dictionary<string, CardInfo>();
	
	
	public bool _loadDatabaseOnline = false;
	public TextAsset _database;
	string _databaseString;
	
	Thread _databaseLoadingThread;
	bool _databaseThreadWasAlive;
	
	
	
	void Awake() {
		_databaseString = _database.text;
	}
	
	void Start() {
		_databaseLoadingThread = new Thread(new ThreadStart(LoadLocalDatabase));
		_databaseLoadingThread.Start ();
		_databaseThreadWasAlive = true;
	}
	
	void Update() {
		if (_databaseThreadWasAlive && !_databaseLoadingThread.IsAlive) {
			Debug.Log("Database loaded");
			foreach (var cardInfo in _cardInfo) {
				cardInfo.Value.CallUpdated();
			}
		}
		_databaseThreadWasAlive = _databaseLoadingThread.IsAlive;
	}
	
	
	
	public CardInfo RequestInfo(string name) {
		return null;
	}
	
	
	
	public void LoadLocalDatabase() {
		var sets = Json.Deserialize(_databaseString) as Dictionary<string,object>;
		
		foreach (var set in sets) {
			var setInfo = set.Value as Dictionary<string,object>;
			var cards = setInfo["cards"] as List<object>;
			foreach (var card in cards) {
				var cardInfo = card as Dictionary<string,object>;
				var info = RequestInfo(cardInfo["name"] as String);
				//info.SetInfoOnSeparateThread(cardInfo);
			}
		}
	}
	
}
