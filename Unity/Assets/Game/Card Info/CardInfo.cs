using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiniJSON;

public class CardInfo {

	public event SimpleEventHandler Updated;
	

	public string Name { get; private set; }
	public string ImageURL { get; private set; }
	
	public string Type { get; private set; }

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
	
	public void SetInfoOnSeparateThread(Dictionary<string,object> info) {
		lock (this) {
			GetCommonInfoDefault(info);
			
			//temp fix for local database land images
			if (Type.Contains("Basic Land")) {
				ImageURL = "http://mtgimage.com/card/" + Name +	".jpg";
			}
			
			if (!Type.Contains("Land")) {
				GetManaInfoDefault(info);
			}
			if (Type.Contains("Creature")) {
				GetPowerAndThoughnessInfoDefault(info);
			}
			if (Type.Contains("Planeswalker")) {
				GetLoyaltyInfoDefault(info); //Call this even when given an empty json, since it'll set the imageurl.
			}
		}
		//Used on separate thread, so don't call update here. Will be called when all cards are loaded by the CardInfoManager.
	}
	
	public void SetInfoFromMTGdb(string json) {
		var cards = Json.Deserialize(json) as List<object>;
		Dictionary<string,object> info;
		
		if (cards.Count > 0) {
			info = cards[0] as Dictionary<string,object>;
		}
		else {
			Debug.Log("Couldn't get data for the card");
			info = new Dictionary<string,object>();
		}
	
		GetCommonInfoDefault(info); //The type info gathered in here is very limited for MTGdb, no super-or subtypes is included
		
		if (!Type.Contains("Land")) {
			GetManaInfoDefault(info);
		}
		if (Type.Contains("Creature")) {
			GetPowerAndThoughnessInfoDefault(info);
		}
		if (Type.Contains("Planeswalker")) {
			GetLoyaltyInfoDefault(info); //Call this even when given an empty json, since it'll set the imageurl.
		}
		
		CallUpdated();
	}
	
	void GetCommonInfoDefault(Dictionary<string,object> info) {
		Name = TryGetValueFromJson<string>(info, new []{"name"}, Name);
		
		ImageURL =  "http://mtgimage.com/card/" + 
			TryGetValueFromJson<string>(info, new []{"imageName", "name"}, Name) +
				".jpg";
		
		Text = TryGetValueFromJson<string>(info, new []{"text", "description"}, null);
		Flavor = TryGetValueFromJson<string>(info, new []{"flavor"}, null);
		
		Type = TryGetValueFromJson<string>(info, new []{"type"}, "");
		
		MultiverseID = TryGetValueFromJson<long>(info, new []{"multiverseid", "id"}, 0);
	}
	
	void GetManaInfoDefault(Dictionary<string,object> info) {
		ManaCost = TryGetValueFromJson<string>(info, new []{"manaCost"}, null);
		ConvertedManaCost = TryGetValueFromJson<double>(info, new []{"cmc", "convertedManaCost"}, 0.0);
	}
	
	void GetLoyaltyInfoDefault(Dictionary<string,object> info) {
		Loyalty = TryGetValueFromJson<long>(info, new []{"loyalty"}, 0);
	}
	void GetPowerAndThoughnessInfoDefault(Dictionary<string,object> info) {
		Power = TryGetValueFromJson<string>(info, new []{"power"}, null);
		Toughness = TryGetValueFromJson<string>(info, new []{"toughness"}, null);
	}
	
	
	
	
	
	T TryGetValueFromJson<T>(Dictionary<string,object> info, string[] possibleValueNames, T notFoundValue) {
		foreach (string valueName in possibleValueNames) {
			object value;
			if (info.TryGetValue(valueName, out value)) {
				return (T)Convert.ChangeType(value, typeof(T));
			}
		}
		
		return notFoundValue; //or use Default(T)
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