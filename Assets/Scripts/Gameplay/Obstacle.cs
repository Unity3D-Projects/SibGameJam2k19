using UnityEngine;

using SMGCore;
using SMGCore.EventSys;
using KOZA.Events;

public enum ObstacleType {
	Bush,
	Stump,
	Bee,
	Hedgehog
}

public class Obstacle : MonoBehaviour {
	public ObstacleType Type         = ObstacleType.Bush;
	public float        CoolDownTime = 1f;

	float _lastHitTime = 0f;

	protected virtual void OnTriggerEnter2D(Collider2D other) {
		var curTime = GameState.Instance.TimeController.CurrentTime;
		if ( _lastHitTime + CoolDownTime > curTime ) {
			return;
		}
		var goat = other.GetComponent<GoatController>();

		if ( !goat ) {
			return;
		}

		if ( Type == ObstacleType.Bush ) {
			SoundManager.Instance.PlaySound("Bush");
		}

		if ( Type == ObstacleType.Bee ) {
			SoundManager.Instance.PlaySound("Buzz");
		}

		_lastHitTime = curTime;
		EventManager.Fire(new Event_Obstacle_Collided { Obstacle = this });
	}
}
