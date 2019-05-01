using EventSys;
using UnityEngine;

public enum FarmerActionType {
	Jump,
	SlowDown,
	Piano
}

public sealed class FarmerActionTrigger : MonoBehaviour {
	public FarmerActionType Type = FarmerActionType.Jump;

	private void OnTriggerEnter2D(Collider2D other) {
		var farmer = other.GetComponent<FarmerController>();

		if ( !farmer ) {
			return;
		}
		EventManager.Fire(new Event_FarmerActionTrigger { Trigger = this });

	}
}
