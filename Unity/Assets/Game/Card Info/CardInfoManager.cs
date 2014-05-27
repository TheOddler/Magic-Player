using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using MiniJSON;

public class CardInfoManager : MonoBehaviour {
	
	static CardInfoManager _instance;
	public static CardInfoManager Instance { get { return _instance; } }
	
	// Properties
	public Shader _cardShader;
	public Shader CardShader {
		get {
			return _cardShader;
		}
	}
	
	public Texture2D _cardBack;
	public Texture2D CardBack {
		get {
			return _cardBack;
		}
	}
	
	public bool _loadDatabaseOnline = false;
	public TextAsset _database;
	string _databaseString;

	Thread _databaseLoadingThread;
	bool _databaseThreadWasAlive;
	
	// Info
	private Dictionary<string, CardInfo> _cardInfo = new Dictionary<string, CardInfo>();
	public static Dictionary<string, CardInfo> CardInfo {
		get {
			return _instance._cardInfo;
		}
	}
	
	void Awake() {
		if (_instance != null) throw new UnityException("Creating more than one CardImageManager");
		_instance = this;

		_databaseString = _database.text;
	}

	void Start() {
		_databaseLoadingThread = new Thread(new ThreadStart(LoadDatabase));
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
	
	public CardInfo GetCardInfo(string name) {
		lock (_cardInfo) {
			var simpleName = name.Simplify();
			// If you try to get the Card Info before it was loaded from the database, create a temp one and it will be filled when loaded by the database.
			if (!_cardInfo.ContainsKey (simpleName)) {
				var card = new CardInfo (simpleName);
				_cardInfo.Add (simpleName, card);
			}
			return _cardInfo [simpleName];
		}
	}
	
	
	public void LoadDatabase() {
		var sets = Json.Deserialize(_databaseString) as Dictionary<string,object>;

		foreach (var set in sets) {
			var setInfo = set.Value as Dictionary<string,object>;
			var cards = setInfo["cards"] as List<object>;
			foreach (var card in cards) {
				var cardInfo = card as Dictionary<string,object>;
				var info = GetCardInfo(cardInfo["name"] as String);
				info.SetInfo(cardInfo);
			}
		}
	}
	
	public IEnumerator LoadCardImageInto(string url, Material material) {
		var www = new WWW(url);
		yield return www;
		
		if (www.error == null) {
			material.mainTexture = new Texture2D(4, 4, TextureFormat.DXT1, false);
			www.LoadImageIntoTexture(material.mainTexture as Texture2D);
			material.mainTexture.anisoLevel = 4;
		}
		else {
			Debug.Log("Card not found: " + url + "; with error: " + www.error);
		}
	}
}

