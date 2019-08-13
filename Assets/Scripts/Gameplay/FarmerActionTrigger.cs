using UnityEngine;

using SMGCore.EventSys;
using KOZA.Events;

public enum FarmerActionType {
	Jump,
	SlowDown,
	Piano
}

public sealed class FarmerActionTrigger : MonoBehaviour {
	public FarmerActionType Type = FarmerActionType.Jump;

	public bool  OverrideJumpPower = false;
	public float NewJumpPower      = 0;

	private void OnTriggerEnter2D(Collider2D other) {
		var farmer = other.GetComponent<FarmerController>();

		if ( !farmer ) {
			return;
		}
		EventManager.Fire(new Event_FarmerActionTrigger { Trigger = this });
	}
}
