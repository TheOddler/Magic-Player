using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiniJSON;

public class MtgDbDotInfo : CardInfoProvider {
	
	override public void FillInfo(CardInfo cardInfo) {
		StartCoroutine(LoadCardInfo(cardInfo));
	}
	
	IEnumerator LoadCardInfo(CardInfo info, bool useBypass = false) {
		//Using database: https://www.mtgdb.info/
		
		string fields = "[]"; //"name,manacost,convertedManaCost"; //Don't work on the server yet. [] just gives them all
		string url = "http://api.mtgdb.info/cards/" + info.Name.Replace("-"," ").Replace(":","").Replace (",","") + "?fields=" + fields;
		if (useBypass) {
			url = Helpers.BypassCrosdomain(url, "xml");
		}
		var www = new WWW(url);
		yield return www; //wait to load info
		
		//Debug.Log(Uri.EscapeUriString(url));
		
		if(www.error == null) {
			string json = www.text;
			if (useBypass) {
				json = Helpers.RemoveBypassPadding(json);
			}
			
			//Debug.Log(json); //Look out with this. If this is a HUUUGE json (like with a very common card as an Island), this will cause the editor to crash.
			
			SetInfoFromMTGdb(info, json);
		}
		else {
			Debug.Log("Error downloading from " + url + " with error: " + www.error);
			if (!useBypass) {
				Debug.Log("Trying again with bypass.");
				LoadCardInfo(info, true);
			}
		}
	}
	
	public void SetInfoFromMTGdb(CardInfo cardInfo, string json) {
		var cards = Json.Deserialize(json) as List<object>;
		Dictionary<string,object> info;
		
		if (cards.Count > 0) {
			info = cards[0] as Dictionary<string,object>;
		}
		else {
			Debug.Log("Couldn't get data for the card");
			info = new Dictionary<string,object>();
		}
		
		GetCommonInfoDefault(cardInfo, info); //The type info gathered in here is very limited for MTGdb, no super-or subtypes is included
		
		if (!cardInfo.Type.Contains("Land")) {
			GetManaInfoDefault(cardInfo, info);
		}
		if (cardInfo.Type.Contains("Creature")) {
			GetPowerAndThoughnessInfoDefault(cardInfo, info);
		}
		if (cardInfo.Type.Contains("Planeswalker")) {
			GetLoyaltyInfoDefault(cardInfo, info); //Call this even when given an empty json, since it'll set the imageurl.
		}
		
		cardInfo.CallUpdated();
	}
	
	void GetCommonInfoDefault(CardInfo cardInfo, Dictionary<string,object> info) {
		//cardInfo.Name = TryGetValueFromJson<string>(info, new []{"name"}, cardInfo.Name);
		
		cardInfo.Text = TryGetValueFromJson<string>(info, new []{"text", "description"}, null);
		cardInfo.Flavor = TryGetValueFromJson<string>(info, new []{"flavor"}, null);
		
		cardInfo.Type = TryGetValueFromJson<string>(info, new []{"type"}, "");
		
		cardInfo.MultiverseID = TryGetValueFromJson<long>(info, new []{"multiverseid", "id"}, 0);
	}
	
	void GetManaInfoDefault(CardInfo cardInfo, Dictionary<string,object> info) {
		cardInfo.ManaCost = TryGetValueFromJson<string>(info, new []{"manaCost"}, null);
		cardInfo.ConvertedManaCost = TryGetValueFromJson<double>(info, new []{"cmc", "convertedManaCost"}, 0.0);
	}
	
	void GetLoyaltyInfoDefault(CardInfo cardInfo, Dictionary<string,object> info) {
		cardInfo.Loyalty = TryGetValueFromJson<long>(info, new []{"loyalty"}, 0);
	}
	void GetPowerAndThoughnessInfoDefault(CardInfo cardInfo, Dictionary<string,object> info) {
		cardInfo.Power = TryGetValueFromJson<string>(info, new []{"power"}, null);
		cardInfo.Toughness = TryGetValueFromJson<string>(info, new []{"toughness"}, null);
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
	
}
