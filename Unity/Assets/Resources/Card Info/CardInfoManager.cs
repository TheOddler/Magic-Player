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
	
	public bool _loadDatabaseOnline = false;
	public TextAsset _database;
	
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
		bool bytesLoadedSucessfully = true;
		byte[] bytes;
		
		if (_loadDatabaseOnline) {
			var www = new WWW("https://dl.dropbox.com/u/10448192/Magic%20Player/CardDatabase/mtg_card_database.xml");
			yield return www;
			if (www.error != null) {
				Debug.Log("Could not download databse: " + www.error);
				bytesLoadedSucessfully = false;
			}
			else {
				Debug.Log("Downloaded database.");
			}
			
			bytes = www.bytes;
		}
		else {
			bytes = _database.bytes;
			Debug.Log("Loaded local database.");
		}
		
		if (bytesLoadedSucessfully) {
			MemoryStream assetStream = new MemoryStream(bytes);
			
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
						
					case "type":
						tempCardInfo.Type = reader.ReadElementContentAsString();
						break;
						
					case "rules":
						tempCardInfo.Rules = reader.ReadElementContentAsString();
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
		
			Debug.Log("Succesfully parsed database.");
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

