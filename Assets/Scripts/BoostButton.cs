using UnityEngine;
using EventSys;

public class BoostButton : MonoBehaviour
{
	BoostType BoostType;

	public void BoostButtonClick() { 
		EventManager.Fire(new Event_BoostActivated { Type = BoostType });
	}
}
