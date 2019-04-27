using System.Collections;
using System.Collections.Generic;
using EventSys;
using UnityEngine;

public class FarmerController : MonoBehaviour {
	public float MoveSpeed = 3f;
	public float JumpPower = 8f;
	public float StartDelay = 2f;
	public PhysicsObject Controller = null;

	bool _started = false;

	void Start() {
		Controller.SetMoveSpeed(0f);
		EventManager.Subscribe<Event_FarmerActionTrigger>(this, OnFarmerActionTrigger);
	}

	void OnDestroy() {
		EventManager.Unsubscribe<Event_FarmerActionTrigger>(OnFarmerActionTrigger);
	}

	void Update() {
		var curTime = GameState.Instance.TimeController.CurrentTime;

		if ( !_started ) {
			_started = curTime > StartDelay;
			return;
		}

		Controller.SetMoveSpeed(MoveSpeed);
	}

	void Jump() {
		Controller.Jump(JumpPower);
	}

	void OnFarmerActionTrigger(Event_FarmerActionTrigger e) {
		if ( e.Trigger.Type == FarmerActionType.Jump ) {
			Jump();
		}
	}

}
