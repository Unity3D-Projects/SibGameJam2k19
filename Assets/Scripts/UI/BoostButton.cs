using UnityEngine;

using SMGCore.EventSys;
using KOZA.Events;

using TMPro;

public sealed class BoostButton : MonoBehaviour {
	public BoostType BoostType = BoostType.SpeedUp;
	public TMP_Text  PriceText = null;

	void Start() {
		PriceText.text = GameState.Instance.BoostWatcher.GetBoostInfo(BoostType).Price.ToString();
	}

	public void BoostButtonClick() {
		Debug.Log("Boost type used: " + BoostType);
		EventManager.Fire(new Event_TryActivateBoost { Type = BoostType });
	}
}
