using System.Collections.Generic;
using EventSys;
using UnityEngine;

public enum FarmerActionType {
	Jump
}

public class FarmerActionTrigger : MonoBehaviour {
	public FarmerActionType Type = FarmerActionType.Jump;

	private void OnTriggerEnter2D(Collider2D other) {
		var farmer = other.GetComponent<FarmerController>();

		if ( !farmer ) {
			return;
		}
		Debug.Log("Farmer trigger enter");
		EventManager.Fire(new Event_FarmerActionTrigger { Trigger = this });

	}
}