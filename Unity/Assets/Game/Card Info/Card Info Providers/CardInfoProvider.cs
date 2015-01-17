using UnityEngine;
using System.Collections;

public abstract class CardInfoProvider: MonoBehaviour, ICardInfoProvider {
	
	abstract public void FillInfo(CardInfo cardInfo);
	
}
