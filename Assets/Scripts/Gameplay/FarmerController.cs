using UnityEngine;

using System.Collections.Generic;

using SMGCore.EventSys;
using KOZA.Events;

public sealed class FarmerController : MonoBehaviour {
	public float         MoveSpeed  = 3f;
	public float         JumpPower  = 6f;
	public float         StartDelay = 2f;
	public PhysicsObject Controller = null;
	public Animation     Anim       = null;

	//хз как это правильно оформить
	ContactFilter2D contactFilter;
	RaycastHit2D[] hitBuffer = new RaycastHit2D[2];
	Rigidbody2D rb2d;
	Vector2 _cast = new Vector2(1.2f, 0);
	float distance = 1.2f;

	bool _started = false;

	void Start() {
		Controller.SetMoveSpeed(0f);
		EventManager.Subscribe<Event_FarmerActionTrigger>(this, OnFarmerActionTrigger);
	}
	void OnEnable() {
		rb2d = Controller.GetComponent<Rigidbody2D>();
	}

	void OnDestroy() {
		EventManager.Unsubscribe<Event_FarmerActionTrigger>(OnFarmerActionTrigger);
	}

	void UpdateObstacleJumping() {
		var count = rb2d.Cast(_cast, contactFilter, hitBuffer, distance);
		if ( count != 0  ) {
			for ( int i = 0; i < hitBuffer.Length; i++ ) {
				if ( hitBuffer[i] ) {
					if ( hitBuffer[i].transform.GetComponent<GoatController>() != null ) {
						return;
					} 
				} 
			}
			Jump();
		}
	}

	void Update() {
		var curTime = GameState.Instance.TimeController.CurrentTime;

		if ( !_started ) {
			_started = curTime > StartDelay;
			if ( _started ) {
				Anim.Play("FarmerRun");
			}
			return;
		}

		var dist = GameState.Instance.Goat.transform.position.x - transform.position.x;
		if ( !Controller.Grounded && dist < 0 ) {
			Controller.SetMoveSpeed(0);
		} else {
			Controller.SetMoveSpeed(MoveSpeed);
		}

	}

	void FixedUpdate() {
		var cols = Physics2D.OverlapCircleAll(transform.position, 1.3f);
		foreach ( var col in cols ) {
			var goat = col.gameObject.GetComponent<GoatController>();
			if ( goat ) {
				goat.CurrentState.ChangeState(new DeadState(goat));
				EventManager.Fire(new Event_GoatDies());
				break;
			}
		}
		//if ( LevelSettings.Instance.Endless & Controller.Grounded ) {
		if ( Controller.Grounded ) {
			UpdateObstacleJumping(); 
		}
	}

	void Jump() {
		Controller.Jump(JumpPower);
	}

	void OnFarmerActionTrigger(Event_FarmerActionTrigger e) {
		if ( e.Trigger.Type == FarmerActionType.Jump ) {
			if ( e.Trigger.OverrideJumpPower ) {
				float _jumpPower = JumpPower;
				JumpPower = e.Trigger.NewJumpPower;
				Jump();
				JumpPower = _jumpPower;
			} else {
				Jump();
			}
		}
		if ( e.Trigger.Type == FarmerActionType.SlowDown ) {
			//TODO:PlayAnim
		}
	}
}
