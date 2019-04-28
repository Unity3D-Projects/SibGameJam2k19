using UnityEngine;
using EventSys;

using TMPro;

public class BoostButton : MonoBehaviour {
	public BoostType BoostType;
	public TMP_Text PriceText = null;

	void Start() {
		PriceText.text = GameState.Instance.BoostWatcher.GetBoostInfo(BoostType).Price.ToString();
	}

	public void BoostButtonClick() {
		Debug.Log("Boost type used: " + BoostType);
		EventManager.Fire(new Event_TryActivateBoost { Type = BoostType });
	}
}
