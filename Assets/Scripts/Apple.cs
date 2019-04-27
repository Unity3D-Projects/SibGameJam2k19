using UnityEngine;
using EventSys;

public class Apple : MonoBehaviour {
	private void OnTriggerEnter2D() {
		EventManager.Fire(new Event_AppleCollected { });
		this.gameObject.SetActive(false);
	} 
}
