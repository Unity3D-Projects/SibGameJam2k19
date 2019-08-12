using UnityEngine;

using SMGCore.EventSys;
using KOZA.Events;

public sealed class FallTrigger : MonoBehaviour {
	private void OnTriggerEnter2D(Collider2D other) {
		var goat = other.GetComponent<GoatController>();

		if ( !goat ) {
			return;
		}
		Debug.Log("Fall trigger enter");
		goat.CurrentState.ChangeState(new DeadState(goat));
		EventManager.Fire(new Event_GoatDies());
	}
}
