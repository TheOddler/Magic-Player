using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CardInfo {

	public event SimpleEventHandler Updated;
	
	public string Name { get; set; }
	public long MultiverseID { get; set; }

	public string Type { get; set; }

	public string ManaCost { get; set; }
	public double ConvertedManaCost { get; set; }

	public string Power { get; set; }
	public string Toughness { get; set; }
	public long Loyalty { get; set; }

	public string Text { get; set; }
	public string Flavor { get; set; }
	
	
	
	public CardInfo(string name) {
		Name = name;
		
		//Updated += UpdateMaterialIfUsed;
	}

	public void CallUpdated() {
		if (Updated != null) Updated ();
	}
	
	
	
	Material _imageMaterial = null;
	public Material ImageMaterial {
		get {
			if (_imageMaterial == null) {
				var manager = CardInfoManager.Instance;
				
				_imageMaterial = new Material(manager.CardShader);
				_imageMaterial.mainTexture = manager.CardLoading;
				
				manager.LoadImageFor(this, OnImageLoaded);
			}
			return _imageMaterial;
		}
	}
	
	void OnImageLoaded(Texture2D texture) {
		if (texture != null) {
			_imageMaterial.mainTexture = texture;
		}
	}

	/*void UpdateMaterialIfUsed() {
		// only update the material if it is being used (_imageMaterial != null).
		if (_imageMaterial != null) {
			CardInfoManager.Instance.LoadImageFor(this, OnImageLoaded);
		}
	}*/
}
