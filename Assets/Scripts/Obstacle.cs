using UnityEngine;
using EventSys;

public enum ObstacleType {
	Bush,
	Stump,
	Bee,
	Hedgehog
}

public class Obstacle : MonoBehaviour {
	public ObstacleType Type = ObstacleType.Bush;
	public float CoolDownTime = 1f;

	float _lastHitTime = 0f;

	private void OnTriggerEnter2D(Collider2D other) {
		var curTime = GameState.Instance.TimeController.CurrentTime;
		if ( _lastHitTime + CoolDownTime > curTime ) {
			return;
		}
		var goat = other.GetComponent<GoatController>();

		if ( !goat ) {
			return;
		}

		_lastHitTime = curTime;
		Debug.Log("Obstacle enter");
		EventManager.Fire(new Event_Obstacle_Collided { Obstacle = this });

	}
}
