using UnityEngine;
using EventSys;

public sealed class Apple : MonoBehaviour {
	private void OnTriggerEnter2D(Collider2D other) {
		var goat = other.GetComponent<GoatController>();

		if ( !goat ) {
			return;
		}
		EventManager.Fire(new Event_AppleCollected { });
		gameObject.SetActive(false);
		SoundManager.Instance.PlaySound("Munch");
	} 
}
