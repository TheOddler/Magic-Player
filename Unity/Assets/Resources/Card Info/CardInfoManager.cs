using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;

public class CardInfo {
	public string Name {get; set;}
	public string Cost {get; set;}
	public string Power {get; set;}
	public string Toughness {get; set;}
	public string Rules {get; set;}
	public string Flavor {get; set;}
	
	private Material _imageMaterial;
	public Material ImageMaterial {
		get {
			if (_imageMaterial == null) {
				var manager = CardInfoManager.Instance;
				_imageMaterial = new Material(manager.CardShader);
				_imageMaterial.mainTexture = manager.CardBack;
				manager.StartCoroutine(manager.LoadCardImageInto(Name, _imageMaterial));
			}
			return _imageMaterial;
		}
	}
}

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

	public TextAsset _database;
	
	// Info
	Dictionary<string, CardInfo> _cardInfo = new Dictionary<string, CardInfo>();
	public Dictionary<string, CardInfo> CardInfo {
		get {
			return _cardInfo;
		}
	}
	
	void Awake() {
		if (_instance != null) throw new UnityException("Creating more than one CardImageManager");
		_instance = this;
	}
	
	void Start() {
		float startTime = Time.realtimeSinceStartup;
		LoadDatabase();
		Debug.Log("Loaded Database in " + (Time.realtimeSinceStartup - startTime) + " seconds.");
	}
	
	
	public void LoadDatabase() {
		MemoryStream assetStream = new MemoryStream(_database.bytes);
		
		XmlReaderSettings settings = new XmlReaderSettings();
		settings.IgnoreComments = true;
		settings.IgnoreWhitespace = true;
		XmlReader reader = XmlReader.Create(assetStream, settings);
			
		CardInfo tempCardInfo = new CardInfo();
		
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
					if (!_cardInfo.ContainsKey(tempCardInfo.Name)) _cardInfo.Add (tempCardInfo.Name, tempCardInfo);
					tempCardInfo = null;
				}
			}
		}
	}
	
	public IEnumerator LoadCardImageInto(string name, Material material) {
		var www = new WWW(CardInfoManager.CardImageURLByName(name));
		while (!www.isDone) {
			yield return new WaitForSeconds(.5f);
		}
		material.mainTexture = new Texture2D(4, 4, TextureFormat.DXT1, false);
		www.LoadImageIntoTexture(material.mainTexture as Texture2D);
	}
	public static string CardImageURLByName(string name)
	{
		string nameFixed = name.Replace(" ", "%20");
		//return "http://gatherer.wizards.com/Handlers/Image.ashx?name=" + nameFixed + "&type=card";
		return "http://deckbox.org/mtg/" + nameFixed + "/tooltip.jpg";
	}
}
