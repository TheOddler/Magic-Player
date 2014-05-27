using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CardInfo {

	public event SimpleEventHandler Updated;
	

	public string Name { get; private set; }
	public string ImageName { get; private set; }

	public string ManaCost { get; private set; }
	public double ConvertedManaCost { get; private set; }

	public string Power { get; private set; }
	public string Toughness { get; private set; }
	public long Loyalty { get; private set; }

	public List<string> Colors { get; private set; }

	public string Type { get; private set; }
	public string Supertypes { get; private set; }
	public string Types { get; private set; }
	public string Subtypes { get; private set; }

	public string Rarity { get; private set; }
	public string Text { get; private set; }
	public string Flavor { get; private set; }
	public string Artist { get; private set; }

	public string Layout { get; private set; }

	public string Number { get; private set; }
	public long MultiverseID { get; private set; }

	public CardInfo(string name) {
		Name = name;

		Updated += TryUpdateMaterial;
	}

	public void CallUpdated() {
		if (Updated != null) {
			Updated ();
		}
	}
	
	public void SetInfo(Dictionary<string,object> info) {
		lock (this) {
			Name = info["name"] as string;
			if (info.ContainsKey("imageName")) ImageName = info["imageName"] as string;
			
			if (info.ContainsKey("manaCost")) ManaCost = info["manaCost"] as string;
			// can have a decimal point, though this only occurs with unhinged cards.
			if (info.ContainsKey ("cmc")) {
				object cmc = info["cmc"];
				if (cmc.GetType() == typeof(double)) {
					ConvertedManaCost = (double)info["cmc"];
				}
				else {
					ConvertedManaCost = (double)((long)info["cmc"]);
				}
			}

			if (info.ContainsKey("power")) Power = info["power"] as string;
			if (info.ContainsKey("toughness")) Toughness = info["toughness"] as string;
			if (info.ContainsKey("loyalty")) Loyalty = (long)info["loyalty"];

			if (info.ContainsKey("colors")) Colors = (info["colors"] as List<object>).ConvertAll(input=>input as string);

			if (info.ContainsKey("type")) Type = info["type"] as string;
			if (info.ContainsKey("supertypes")) Supertypes = info["supertypes"] as string;
			if (info.ContainsKey("types")) Types = info["types"] as string;
			if (info.ContainsKey("subtypes")) Subtypes = info["subtypes"] as string;

			if (info.ContainsKey("rarity")) Rarity = info["rarity"] as string;
			if (info.ContainsKey("text")) Text = info["text"] as string;
			if (info.ContainsKey("flavor")) Flavor = info["flavor"] as string;
			if (info.ContainsKey("artist")) Artist = info["artist"] as string;

			if (info.ContainsKey("layout")) Layout = info["layout"] as string;

			if (info.ContainsKey("number")) Number = info["number"] as string;
			if (info.ContainsKey("multiverseid")) MultiverseID = (long)info["multiverseid"];
		}
	}

	
	private Material _imageMaterial;
	public Material ImageMaterial {
		get {
			if (_imageMaterial == null) {
				var manager = CardInfoManager.Instance;
				_imageMaterial = new Material(manager.CardShader);
				_imageMaterial.mainTexture = manager.CardBack;
			}
			if (!string.IsNullOrEmpty(ImageName)) {
				TryUpdateMaterial();
			}
			return _imageMaterial;
		}
	}

	void TryUpdateMaterial() {
		// only try to update the material if it is being used (_imageMaterial != null).
		if (_imageMaterial != null && !string.IsNullOrEmpty(ImageName)) {
			var manager = CardInfoManager.Instance;
			manager.StartCoroutine(manager.LoadCardImageInto(ImageURL, _imageMaterial));
		}
	}

	public string ImageURL {
		get {
			return "http://mtgimage.com/card/" + ImageName + ".jpg";
		}
	}
}