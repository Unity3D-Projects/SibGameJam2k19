using System.Collections;
using System.Collections.Generic;
using EventSys;
using UnityEngine;

public class EndTrigger : MonoBehaviour {
	private void OnTriggerEnter2D(Collider2D other) {
		var goat = other.GetComponent<GoatController>();

		if (!goat) {
			return;
		}
		Debug.Log("End trigger enter");
		EventManager.Fire(new Event_GameWin());

	}
}
