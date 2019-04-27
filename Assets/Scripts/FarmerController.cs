using EventSys;
using UnityEngine;

public class FarmerController : MonoBehaviour {
	public float MoveSpeed = 3f;
	public float JumpPower = 6f;
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

		var cols = Physics2D.OverlapCircleAll(transform.position, 1.8f);
		foreach ( var col in cols ) {
			var goat = col.gameObject.GetComponent<GoatController>();
			if ( goat ) {
				Debug.LogWarning("DEAD");
				goat.CurrentState.ChangeState(new DeadState(goat));
				EventManager.Fire(new Event_GoatDies());
				break;
			}
		}
	}

	void Jump() {
		Controller.Jump(JumpPower);
	}

	void OnFarmerActionTrigger(Event_FarmerActionTrigger e) {
		if ( e.Trigger.Type == FarmerActionType.Jump ) {
			Jump();
		}
		if ( e.Trigger.Type == FarmerActionType.SlowDown ) {
			//PlayAnim
		}
	}
}
