using UnityEngine;

using SMGCore.EventSys;
using KOZA.Events;

public sealed class GoatVisual : MonoBehaviour {
	public GameObject NormalGoat   = null;
	public GameObject DeadGoat     = null;
	public GameObject SlidingGoat  = null;
	public GameObject JumpingGoat  = null;
	public GameObject ObstacleGoat = null;
	public GameObject SlowGoat     = null;
	public ParticleSystem SpeedParticles = null;

	private void OnEnable() {
		var emis = SpeedParticles.emission;
		emis.enabled = false;

		EventManager.Subscribe<Event_BoostActivated>(this, OnBoostApply);
		EventManager.Subscribe<Event_BoostEnded>    (this, OnBoostEnd);
	}

	private void OnDisable() {
		EventManager.Unsubscribe<Event_BoostActivated>(OnBoostApply);
		EventManager.Unsubscribe<Event_BoostEnded>    (OnBoostEnd);
	}

	void OnBoostApply(Event_BoostActivated e) {
		if ( e.Type == BoostType.SpeedUp ) {
			var emis = SpeedParticles.emission;
			emis.enabled = true;
		}
	}

	void OnBoostEnd(Event_BoostEnded e) {
		if ( e.Type == BoostType.SpeedUp ) {
			var emis = SpeedParticles.emission;
			emis.enabled = false;
		}
	}

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
				SetSlowState();
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
		SlowGoat.SetActive(false);
	}

	void SetNormalState() {
		TurnOffAllStates();
		NormalGoat.SetActive(true);
		NormalGoat.GetComponent<Animation>().Play();
		var slideshow = NormalGoat.GetComponent<SlideShowAnim>();
		if ( slideshow ) {
			slideshow.Play();
		}
	}

	void SetSlowState() {
		TurnOffAllStates();
		SlowGoat.SetActive(true);
		SlowGoat.GetComponent<Animation>().Play();
		var slideshow = SlowGoat.GetComponent<SlideShowAnim>();
		if ( slideshow ) {
			slideshow.Play();
		}
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
		var slideshow = SlidingGoat.GetComponent<SlideShowAnim>();
		if ( slideshow ) {
			slideshow.Play();
		}
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
		var slideshow = ObstacleGoat.GetComponent<SlideShowAnim>();
		if ( slideshow ) {
			slideshow.Play();
		}
	}
}
