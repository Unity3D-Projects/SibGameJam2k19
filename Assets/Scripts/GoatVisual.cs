using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoatVisual : MonoBehaviour {
	public GameObject NormalGoat  = null;
	public GameObject DeadGoat    = null;
	public GameObject SlidingGoat = null;
	public GameObject JumpingGoat = null;
	public GameObject ObstacleGoat = null;

	public void SetState(GoatState state) {
		switch ( state ) {
			case GoatState.Run:
				SetNormalState();
				break;
			case GoatState.Jump:
				SetJumpingState();
				break;
			case GoatState.Obstacle:
				SetObstacleGoat();
				break;
			case GoatState.SlowDown:
				SetNormalState();
				break;
			case GoatState.Yell:
				SetNormalState();
				break;
			case GoatState.Die:
				SetDeadGoat();
				break;
			case GoatState.Slide:
				SetSlidingGoat();
				break;
			case GoatState.None:
				SetNormalState();
				break;
		}
	}

	void TurnOffAllStates() {
		NormalGoat.SetActive(false);
		DeadGoat.SetActive(false);
		SlidingGoat.SetActive(false);
		JumpingGoat.SetActive(false);
		ObstacleGoat.SetActive(false);
	}

	void SetNormalState() {
		TurnOffAllStates();
		NormalGoat.SetActive(true);
		NormalGoat.GetComponent<Animation>().Play();
	}

	void SetJumpingState() {
		TurnOffAllStates();
		JumpingGoat.SetActive(true);
		JumpingGoat.GetComponent<Animation>().Play();
	}

	void SetSlidingGoat() {
		TurnOffAllStates();
		SlidingGoat.SetActive(true);
		SlidingGoat.GetComponent<Animation>().Play();
	}

	void SetDeadGoat() {
		TurnOffAllStates();
		DeadGoat.SetActive(true);
		DeadGoat.GetComponent<Animation>().Play();
	}

	void SetObstacleGoat() {
		TurnOffAllStates();
		ObstacleGoat.SetActive(true);
		ObstacleGoat.GetComponent<Animation>().Play();
	}

}
