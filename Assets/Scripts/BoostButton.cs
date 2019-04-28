using UnityEngine;
using EventSys;

public class BoostButton : MonoBehaviour
{
	public BoostType BoostType;

	public void BoostButtonClick() {
		Debug.Log("Boost type used: " + BoostType);
		EventManager.Fire(new Event_TryActivateBoost { Type = BoostType });
	}
}
