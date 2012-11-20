using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CardImageManager : MonoBehaviour {
	static CardImageManager _instance;
	public static CardImageManager Instance { get { return _instance; } }
	
	// Properties
	public Shader _cardShader;
	
	// Images
	Dictionary<string, Material> _cardImageMaterials = new Dictionary<string, Material>();
	
	void Awake() {
		if (_instance != null) throw new UnityException("Creating more than one CardImageManager");
		_instance = this;
	}
	
	IEnumerator LoadCardImageInto(string name, Material material) {
		material.mainTexture = new Texture2D(4, 4, TextureFormat.DXT1, false);
		var www = new WWW(CardImageManager.CardImageURLByName(name));
		while (!www.isDone) {
			yield return new WaitForSeconds(.5f);
		}
		www.LoadImageIntoTexture(material.mainTexture as Texture2D);
	}
	
	public Material GetImageMaterial(string name) {
		if (!_cardImageMaterials.ContainsKey(name) ) {
			Material newImageMaterial = new Material(_cardShader);
			_cardImageMaterials.Add(name, newImageMaterial);
			StartCoroutine(LoadCardImageInto(name, newImageMaterial));
		}
		
		return _cardImageMaterials[name];
	}
	
	public static string CardImageURLByName(string name)
	{
		string nameFixed = name.Replace(" ", "%20");
		//return "http://gatherer.wizards.com/Handlers/Image.ashx?name=" + name + "&type=card";
		return ("http://deckbox.org/mtg/" + nameFixed + "/tooltip");
	}
	
	public static string CardInfoURLByName(string name)
	{
		return "http://ww2.wizards.com/gatherer/CardDetails.aspx?name=" + name;
	}
}
