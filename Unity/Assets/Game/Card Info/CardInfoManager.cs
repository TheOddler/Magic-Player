using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiniJSON;

public class CardInfoManager : MonoBehaviour {
	
	public static CardInfoManager Instance { get; private set; }
	
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
	
	public Texture2D _cardLoading;
	public Texture2D CardLoading {
		get {
			return _cardLoading;
		}
	}
	
	// Info
	public CardInfoProvider _cardInfoProvider;
	public ICardImageUrlProvider _cardImageUrlProvider = new ciupMtgImageDotCom();
	
	private Dictionary<string, CardInfo> _cardInfoDictionary = new Dictionary<string, CardInfo>();
	
	// Initialize singleton
	void Awake() {
		if (Instance != null) throw new UnityException("More than one CardImageManager.");
		Instance = this;
	}
	
	// Get info and cash the ones already asked for.
	public CardInfo GetCardInfo(string name) {
		String simpleName = name.Simplify();
		
		// If you try to get the Card Info before it was loaded from the database, create a temp one and it will be filled when loaded by the database.
		if (!_cardInfoDictionary.ContainsKey (simpleName)) {
			var card = new CardInfo (simpleName);
			_cardInfoDictionary.Add (simpleName, card);
			
			_cardInfoProvider.FillInfo(card);
		}
		
		return _cardInfoDictionary [simpleName];
	}
	
	
	
	public void LoadImageFor(CardInfo cardInfo, Action<Texture2D> callback) {
		string url = _cardImageUrlProvider.GetUrl(cardInfo);
		
		StartCoroutine(LoadImageInto(url, callback));
	}
	
	public IEnumerator LoadImageInto(string url, Action<Texture2D> callback) {
		
		var www = new WWW( url );
		yield return www;
		
		if (www.error == null) {
			var texture = new Texture2D(1, 1/*, TextureFormat.DXT1, false*/);
			www.LoadImageIntoTexture(texture as Texture2D);
			texture.anisoLevel = 4;
			
			Debug.Log("Image loaded: " + url);
			callback(texture);
		}
		else {
			Debug.Log("Failed to load image: " + url + "; " + www.error);
			callback(null);
		}
	}
	
}

