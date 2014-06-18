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
				LoadCardInfo(card);
			}
			return _cardInfo [simpleName];
		}
	}
	
	
	void LoadCardInfo(CardInfo card) {
		switch(_databaseProvider) {
		case DatabaseProvider.mtgdbinfo:
			StartCoroutine(LoadCardInfoFromMTGdb(card));
			break;
		default:
			//Do nothing
			break;
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
				info.SetInfoOnSeparateThread(cardInfo);
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
	
	public IEnumerator LoadCardInfoFromMTGdb(CardInfo info, bool useBypass = false) {
		//Using database: https://www.mtgdb.info/
		
		string fields = "[]"; //"name,manacost,convertedManaCost"; //Don't work on the server yet. [] just gives them all
		string url = "http://api.mtgdb.info/cards/" + info.Name.Replace("-"," ").Replace(":","").Replace (",","") + "?fields=" + fields;
		if (useBypass) {
			url = Helpers.BypassCrosdomain(url, "xml");
		}
		var www = new WWW(url);
		yield return www;
			
		Debug.Log(Uri.EscapeUriString(url));

		if(www.error == null) {
			string json = www.text;
			if (useBypass) {
				json = Helpers.RemoveBypassPadding(json);
			}
			
			//Debug.Log(json); //Look out with this. If this is a HUUUGE json (like with a very common card as an Island), this will cause the editor to crash.
			
			info.SetInfoFromMTGdb(json);
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

