using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class GoatController : MonoBehaviour {
	public float JumpForce = 7f;

	Sequence _seq = null;

	public PhysicsObject CharController = null;
	private void Start() {
		CharController.SetMoveSpeed(3f);
	}

	private void Update() {
		if ( Input.GetKeyDown(KeyCode.Space) ) {
			Jump();
		}
	}

	void Jump() {
		CharController.Jump(JumpForce);
	}
}
