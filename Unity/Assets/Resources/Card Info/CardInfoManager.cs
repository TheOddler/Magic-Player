using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;
using System.Threading;

public class CardInfoManager : MonoBehaviour {
	public const int NUMBER_OF_CARDS_TO_READ_PER_FRAME = 100;
	
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

	public TextAsset _database;
	
	// Info
	private Dictionary<string, CardInfo> _cardInfo = new Dictionary<string, CardInfo>();
	
	
	void Awake() {
		if (_instance != null) throw new UnityException("Creating more than one CardImageManager");
		_instance = this;
		
		StartCoroutine(LoadDatabase());
	}
	
	public CardInfo GetCardInfo(string name) {
		if (!_cardInfo.ContainsKey(name)) {
			_cardInfo.Add(name, new CardInfo() {
				Name = name
			});
		}
		return _cardInfo[name];
	}
	
	public IEnumerator LoadDatabase() {
		float startTime = Time.realtimeSinceStartup;
		
		var www = new WWW("https://dl.dropbox.com/u/10448192/Magic%20Player/CardDatabase/mtg_card_database.xml");
		yield return www;
		if (www.error != null) {
			Debug.Log("Could not load databse: " + www.error);
		}
		else {
			Debug.Log("Downloaded Database in " + (Time.realtimeSinceStartup - startTime) + " seconds.");
			
			MemoryStream assetStream = new MemoryStream(www.bytes);
			
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.IgnoreComments = true;
			settings.IgnoreWhitespace = false;
			XmlReader reader = XmlReader.Create(assetStream, settings);
				
			CardInfo tempCardInfo = new CardInfo();
			
			int cardsParsedThisFrame = 0;
			while (reader.Read())
			{
				// Only detect start elements.
				if (reader.IsStartElement())
				{
					// Get element name and switch on it.
					switch (reader.Name)
					{
					case "card":
						tempCardInfo = new CardInfo();
						break;
						
					case "id":
						tempCardInfo.IDs.Add(reader.ReadElementContentAsString());
						break;
						
					case "name":
						tempCardInfo.Name = reader.ReadElementContentAsString();
						break;
						
					case "cost":
						tempCardInfo.Cost = reader.ReadElementContentAsString();
						break;
						
					case "power":
						tempCardInfo.Power = reader.ReadElementContentAsString();
						break;
						
					case "toughness":
						tempCardInfo.Toughness = reader.ReadElementContentAsString();
						break;
						
					case "rules":
						tempCardInfo.Rules = reader.ReadElementContentAsString();
						break;
						
					case "flavor":
						tempCardInfo.Flavor = reader.ReadElementContentAsString();
						break;
					}
				}
				else {
					if (reader.Name == "card") {
						if (_cardInfo.ContainsKey(tempCardInfo.Name)) {
							_cardInfo[tempCardInfo.Name].AddInfo(tempCardInfo);
						}
						else {
							_cardInfo.Add (tempCardInfo.Name, tempCardInfo);
						}
						tempCardInfo = null;
						
						// Don't read all in the same frame to prevent stuttering.
						++cardsParsedThisFrame;
						if (cardsParsedThisFrame >= NUMBER_OF_CARDS_TO_READ_PER_FRAME) {
							cardsParsedThisFrame = 0;
							yield return null;
						}
					}
				}
			}
		
			Debug.Log("Loaded Database in " + (Time.realtimeSinceStartup - startTime) + " seconds.");
		}
	}
	
	public IEnumerator LoadCardImageInto(CardInfo info, Material material) {
		var www = new WWW(info.ImageURL);
		yield return www;
		
		if (www.error == null) {
			material.mainTexture = new Texture2D(4, 4, TextureFormat.DXT1, false);
			www.LoadImageIntoTexture(material.mainTexture as Texture2D);
		}
		else {
			Debug.Log("Card not found: " + www.error);
		}
	}
}

public class CardInfo {
	public const string UNUSED_VALUE = "";
	
	public event SimpleEventHandler Updated;
	
	private List<string> _ids = new List<string>();
	public List<string> IDs {
		get {
			return _ids;
		}
		set {
			_ids = value;
		}
	}
	
	private string _name = "No Name In Database";
	public string Name {
		get {
			return _name;
		}
		set {
			_name = value;
		}
	}
	
	private string _cost = UNUSED_VALUE;
	public string Cost {
		get {
			return _cost;
		}
		set {
			_cost = value;
		}
	}
	
	private string _power = UNUSED_VALUE;
	public string Power {
		get {
			return _power;
		}
		set {
			_power = value;
		}
	}
	
	private string _toughness = UNUSED_VALUE;
	public string Toughness {
		get {
			return _toughness;
		}
		set {
			_toughness = value;
		}
	}
	
	private string _rules = UNUSED_VALUE;
	public string Rules {
		get {
			return _rules;
		}
		set {
			_rules = value;
		}
	}
	
	private string _flavor = UNUSED_VALUE;
	public string Flavor {
		get {
			return _flavor;
		}
		set {
			_flavor = value;
		}
	}
	
	
	private Material _imageMaterial;
	public Material ImageMaterial {
		get {
			if (_imageMaterial == null) {
				var manager = CardInfoManager.Instance;
				_imageMaterial = new Material(manager.CardShader);
				_imageMaterial.mainTexture = manager.CardBack;
				manager.StartCoroutine(manager.LoadCardImageInto(this, _imageMaterial));
			}
			return _imageMaterial;
		}
	}
	
	public string ImageURL {
		get {
			string nameFixed = Name.Replace(" ", "%20");
			return "http://deckbox.org/mtg/" + nameFixed + "/tooltip.jpg";
		}
	}
	
	public void AddInfo(CardInfo info) {
		if (info.Name != _name) throw new UnityException("Trying to add info of two different cards together");
		
		_ids.AddRange(info.IDs);
		_cost = info.Cost;
		_power = info.Power;
		_toughness = info.Toughness;
		_rules = info.Rules;
		_flavor = info.Flavor;
		
		if (Updated != null) Updated();
	}
}

