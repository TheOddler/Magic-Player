using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiniJSON;

public class CardInfo {

	public event SimpleEventHandler Updated;
	

	public string Name { get; private set; }
	public string ImageURL { get; private set; }

	public string ManaCost { get; private set; }
	public double ConvertedManaCost { get; private set; }

	public string Power { get; private set; }
	public string Toughness { get; private set; }
	public long Loyalty { get; private set; }

	public string Text { get; private set; }
	public string Flavor { get; private set; }
	
	public long MultiverseID { get; private set; }

	public CardInfo(string name) {
		Name = name;

		Updated += UpdateMaterialIfUsed;
	}

	public void CallUpdated() {
		if (Updated != null) {
			Updated ();
		}
	}
	
	public void SetInfoFromLocalDatabase(Dictionary<string,object> info) {
		lock (this) {
			Name = info["name"] as string;
			
			if (info.ContainsKey("imageName")) {
				ImageURL = "http://mtgimage.com/card/" + (info["imageName"] as string) + ".jpg";
			}
			
			if (info.ContainsKey("manaCost")) ManaCost = info["manaCost"] as string;
			
			// can have a decimal point, though this only occurs with unhinged cards.
			if (info.ContainsKey ("cmc")) {
				object cmc = info["cmc"];
				if (cmc.GetType() == typeof(double)) {
					ConvertedManaCost = (double)cmc;
				}
				else {
					ConvertedManaCost = (double)((long)cmc);
				}
			}

			if (info.ContainsKey("power")) Power = info["power"] as string;
			if (info.ContainsKey("toughness")) Toughness = info["toughness"] as string;
			if (info.ContainsKey("loyalty")) Loyalty = (long)info["loyalty"];

			if (info.ContainsKey("text")) Text = info["text"] as string;
			if (info.ContainsKey("flavor")) Flavor = info["flavor"] as string;
			
			if (info.ContainsKey("multiverseid")) MultiverseID = (long)info["multiverseid"];
		}
		
		//Used on separate thread, so don't call update here. Will be called when all cards are loaded by the CardInfoManager.
	}
	
	public void SetInfoFromMTGdbinfo(Dictionary<string,object> info) {
		lock (this) {
			Name = info["name"] as string;
			
			if (info.ContainsKey("name")) {
				ImageURL = "http://mtgimage.com/card/" + (info["name"] as string) + ".jpg";
			}
			
			if (info.ContainsKey("manaCost")) ManaCost = info["manaCost"] as string; //TODO possibly manacost instead of capital C
			
			// can have a decimal point, though this only occurs with unhinged cards.
			if (info.ContainsKey ("convertedManaCost")) {
				object cmc = info["convertedManaCost"];
				if (cmc.GetType() == typeof(double)) {
					ConvertedManaCost = (double)cmc;
				}
				else {
					ConvertedManaCost = (double)((long)cmc);
				}
			}
			
			if (info.ContainsKey("power")) Power = info["power"] as string;
			if (info.ContainsKey("toughness")) Toughness = info["toughness"] as string;
			if (info.ContainsKey("loyalty")) Loyalty = (long)info["loyalty"];
			
			if (info.ContainsKey("description")) Text = info["description"] as string;
			if (info.ContainsKey("flavor")) Flavor = info["flavor"] as string;
			
			if (info.ContainsKey("id")) MultiverseID = (long)info["id"];
		}
		
		CallUpdated();
	}
	
	public void SetInfoFromMTGapicom(Dictionary<string,object> info) {
		lock (this) {
			Name = info["name"] as string;
			
			if (info.ContainsKey("name")) {
			//if (info.ContainsKey("image")) {
				ImageURL = "http://mtgimage.com/card/" + (info["name"] as string).Simplify() + ".jpg";
				//ImageURL = info["image"] as string; //don't use the gatherer image. This will give crossdomain error.
			}
			
			if (info.ContainsKey("mana")) {
				/*var mana = info["mana"];
				var manaObjectList = mana as List<object>;
				var manaStringList = manaObjectList.OfType<string>();
				var manaArray = manaStringList.ToArray();*/
				ManaCost = string.Join("\n", (info["mana"] as List<object>).OfType<string>().ToArray());
			}
			
			// can have a decimal point, though this only occurs with unhinged cards.
			if (info.ContainsKey ("cmc")) {
				ConvertedManaCost = double.Parse(info["cmc"] as string);
			}
			
			if (info.ContainsKey("power")) Power = info["power"] as string;
			if (info.ContainsKey("toughness")) Toughness = info["toughness"] as string;
			if (info.ContainsKey("loyalty")) Loyalty = (long)info["loyalty"];
			
			if (info.ContainsKey("text")) {
				Text = string.Join("\n", (info["text"] as List<object>).OfType<string>().ToArray());
			}
			if (info.ContainsKey("flavor")) {
				Flavor = string.Join("\n", (info["flavor"] as List<object>).OfType<string>().ToArray());
			}
			if (info.ContainsKey("id")) MultiverseID = long.Parse(info["id"] as string);
		}
		
		CallUpdated();
	}
	
	private Material _imageMaterial;
	public Material ImageMaterial {
		get {
			if (_imageMaterial == null) {
				var manager = CardInfoManager.Instance;
				_imageMaterial = new Material(manager.CardShader);
				_imageMaterial.mainTexture = manager.CardBack;
			}
			if (!string.IsNullOrEmpty(ImageURL)) {
				UpdateMaterialIfUsed();
			}
			return _imageMaterial;
		}
	}

	void UpdateMaterialIfUsed() {
		// only update the material if it is being used (_imageMaterial != null).
		if (_imageMaterial != null && !string.IsNullOrEmpty(ImageURL)) {
			var manager = CardInfoManager.Instance;
			manager.StartCoroutine(manager.LoadCardImageInto(ImageURL, _imageMaterial));
		}
	}
}