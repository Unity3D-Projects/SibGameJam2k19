using UnityEngine;

using SMGCore.EventSys;
using KOZA.Events;

public sealed class Hedgehog : Obstacle {
	public float      TurnOffDistance = 5f;
	public GameObject NormalVisual    = null;
	public GameObject ScaredVisual    = null;

	bool _active = true;

	void OnEnable() {
		EventManager.Subscribe<Event_GoatYell>(this, OnGoatYell);
	}

	void OnDisable() {
		EventManager.Unsubscribe<Event_GoatYell>(OnGoatYell);
	}

	void OnGoatYell(Event_GoatYell e ) {
		var goat = GameState.Instance.Goat;
		var distance = transform.position.x - goat.transform.position.x;
		if ( _active && distance < TurnOffDistance ) {
			_active = false;
			PlayScareAnimation();
		}
	}

	void PlayScareAnimation() {
		ScaredVisual.SetActive(true);
		NormalVisual.SetActive(false);
	}

	protected override void OnTriggerEnter2D(Collider2D other) {
		if ( _active ) {
			base.OnTriggerEnter2D(other);
		}
	}
}
