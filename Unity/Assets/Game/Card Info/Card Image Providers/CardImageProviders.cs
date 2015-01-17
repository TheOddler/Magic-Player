using UnityEngine;
using System.Collections;

public interface ICardImageUrlProvider {
	string GetUrl(CardInfo cardInfo);
}

public class ciupMtgImageDotCom: ICardImageUrlProvider {
	public string GetUrl(CardInfo cardInfo) {
		return "http://mtgimage.com/card/" + cardInfo.Name + ".jpg";
	}
}
