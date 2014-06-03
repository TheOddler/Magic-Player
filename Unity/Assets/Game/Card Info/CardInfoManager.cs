using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using MiniJSON;

public enum DatabaseProvider {
	local,		//mtgjson.com
	mtgdbinfo,	//mtgdb.info
	mtgapicom,	//mtgapi.com
}

public class CardInfoManager : MonoBehaviour {
	
	static CardInfoManager _instance;
	public static CardInfoManager Instance { get { return _instance; } }
	
	// Settings
	public DatabaseProvider _databaseProvider = DatabaseProvider.local;
	
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
		if (_databaseProvider == DatabaseProvider.local) {
			_databaseLoadingThread = new Thread(new ThreadStart(LoadLocalDatabase));
			_databaseLoadingThread.Start ();
			_databaseThreadWasAlive = true;
		}
	}

	void Update() {
		if (_databaseProvider == DatabaseProvider.local) {
			if (_databaseThreadWasAlive && !_databaseLoadingThread.IsAlive) {
				Debug.Log("Database loaded");
				foreach (var cardInfo in _cardInfo) {
					cardInfo.Value.CallUpdated();
				}
			}
			_databaseThreadWasAlive = _databaseLoadingThread.IsAlive;
		}
	}
	
	public CardInfo GetCardInfo(string name) {
		lock (_cardInfo) {
			var simpleName = name.Simplify();
			// If you try to get the Card Info before it was loaded from the database, create a temp one and it will be filled when loaded by the database.
			if (!_cardInfo.ContainsKey (simpleName)) {
				var card = new CardInfo (simpleName);
				_cardInfo.Add (simpleName, card);
				if (_databaseProvider == DatabaseProvider.mtgdbinfo) {
					StartCoroutine(LoadCardInfoFromMTGdb(card));
				}
				else if (_databaseProvider == DatabaseProvider.mtgapicom) {
					StartCoroutine(LoadCardInfoFromMTGapicom(card));
				}
			}
			return _cardInfo [simpleName];
		}
	}
	
	
	public void LoadLocalDatabase() {
		var sets = Json.Deserialize(_databaseString) as Dictionary<string,object>;

		foreach (var set in sets) {
			var setInfo = set.Value as Dictionary<string,object>;
			var cards = setInfo["cards"] as List<object>;
			foreach (var card in cards) {
				var cardInfo = card as Dictionary<string,object>;
				var info = GetCardInfo(cardInfo["name"] as String);
				info.SetInfoFromLocalDatabase(cardInfo);
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
	
	public IEnumerator LoadCardInfoFromMTGapicom(CardInfo info) {
		//Using database: http://mtgapi.com/ //doesn't have the crossdomain.xml (yet)
		
		string urlName = "http://api.mtgapi.com/v1/card/name/" + info.Name;
		urlName = Helpers.BypassCrosdomain(urlName, "xml");
		var wwwName = new WWW(urlName);
		yield return wwwName;
		
		if(wwwName.error == null) {
			string jsonName = wwwName.text;
			// Doesn't have crossdomain, so will always use the bypass, there will be some xml padding, remove it.
			jsonName = jsonName.Remove(0, jsonName.LastIndexOf("<p>") +3); //+3 for the <p> itself
			jsonName = jsonName.Remove(jsonName.LastIndexOf("</p>")); //removes the last xml padding, now it's just a json list
			
			var cards = Json.Deserialize(jsonName) as List<object>;
			
			// Get the id of the proper card
			var cardInfo = cards.FirstOrDefault(
				obj=> {
					var card = obj as Dictionary<string,object>;
					return (card["name"] as String).Simplify() == info.Name.Simplify();
				})
				as Dictionary<string,object>;
				
			if (cardInfo != null) {
				// Get all the info based on the id
				string urlId = "http://api.mtgapi.com/v1/card/id/" + cardInfo["id"];
				urlId = Helpers.BypassCrosdomain(urlId, "xml");
				var wwwId = new WWW(urlId);
				yield return wwwId;
				
				if(wwwId.error == null) {
					string jsonId = wwwId.text;
					// Doesn't have crossdomain, so will always use the bypass, there will be some xml padding, remove it.
					jsonId = jsonId.Remove(0, jsonId.LastIndexOf("<p>") +3); //+3 for the <p> itself
					jsonId = jsonId.Remove(jsonId.LastIndexOf("</p>")); //removes the last xml padding, now it's just a json list
					
					var cardsExtraInfo = Json.Deserialize(jsonId) as List<object>;
					var cardExtraInfo = cardsExtraInfo[0] as Dictionary<string,object>;
					
					info.SetInfoFromMTGapicom(cardExtraInfo);
				}
				else {
					Debug.Log("Error downloading from " + urlName + " with error: " + wwwName.error);
				}
			}
			else {
				Debug.Log("Failed to find proper cardID");
			}
		}
		else {
			Debug.Log("Error downloading from " + urlName + " with error: " + wwwName.error);
		}
	}
	
	public IEnumerator LoadCardInfoFromMTGdb(CardInfo info, bool useBypass = false) {
		//Using database: https://www.mtgdb.info/
		
		string fields = "[]"; //"name,manacost,convertedManaCost"; //Don't work on the server yet. [] just gives them all
		string url = "http://api.mtgdb.info/cards/" + info.Name + "?fields=" + fields;
		if (useBypass) {
			url = Helpers.BypassCrosdomain(url, "xml");
		}
		var www = new WWW(url);
		yield return www;
		
		if(www.error == null) {
			string json = www.text;
			if (useBypass) {
				// When the bypass is used, there will be some xml padding, remove it.
				json = json.Remove(0, json.LastIndexOf("<p>") +3); //+3 for the <p> itself
				json = json.Remove(json.LastIndexOf("</p>")); //removes the last xml padding, now it's just a json list
			}
			
			var cards = Json.Deserialize(json) as List<object>;
			var cardInfo = cards[0] as Dictionary<string,object>;
			
			info.SetInfoFromMTGdbinfo(cardInfo);
		}
		else {
			Debug.Log("Error downloading from " + url + " with error: " + www.error);
			if (!useBypass) {
				Debug.Log("Trying again with bypass.");
				LoadCardInfoFromMTGdb(info, true);
			}
		}
	}
}

