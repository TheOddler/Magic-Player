using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
	
	private string _type = UNUSED_VALUE;
	public string Type {
		get {
			return _type;
		}
		set {
			_type = value;
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
	
	
	private Material _imageMaterial;
	public Material ImageMaterial {
		get {
			if (_imageMaterial == null) {
				var manager = CardInfoManager.Instance;
				_imageMaterial = new Material(manager.CardShader);
				_imageMaterial.mainTexture = manager.CardBack;
				manager.StartCoroutine(manager.LoadCardImageInto(ImageURL, _imageMaterial));
			}
			return _imageMaterial;
		}
	}
	
	public string ImageURL {
		get {
			if (_ids.Count == 0) {
				return LQImageURL;
			}
			else {
				return HQImageURL;
			}
		}
	}
	public string LQImageURL {
		get {
			string nameFixed = Name.ToLower().Replace(" ", "%20");
			return "http://deckbox.org/mtg/" + nameFixed + "/tooltip.jpg";
		}
	}
	public string HQImageURL {
		get {
			return "https://dl.dropbox.com/u/10448192/Magic%20Player/Images/" + _ids[0] + ".full.jpg";
		}
	}
	
	public void AddInfo(CardInfo info) {
		if (info.Name != _name) throw new UnityException("Trying to add info of two different cards together");
		
		_ids.AddRange(info.IDs);
		
		if (_cost != UNUSED_VALUE && _cost != info.Cost) Debug.LogWarning("Added info of card with same name but different cost: " + info.Name);
		_cost = info.Cost;
		
		if (_power != UNUSED_VALUE && _power != info.Power) Debug.LogWarning("Added info of card with same name but different power: " + info.Name);
		_power = info.Power;
		
		if (_toughness != UNUSED_VALUE && _toughness != info.Toughness) Debug.LogWarning("Added info of card with same name but different toughness: " + info.Name);
		_toughness = info.Toughness;
		
		if (_rules != UNUSED_VALUE && _rules != info.Rules) Debug.LogWarning("Added info of card with same name but different rules: " + info.Name);
		_rules = info.Rules;
		
		if (_ids.Count > 0 && _imageMaterial != null) {
			var manager = CardInfoManager.Instance;
			manager.StartCoroutine(manager.LoadCardImageInto(HQImageURL, _imageMaterial));
		}
		if (Updated != null) Updated();
	}
}