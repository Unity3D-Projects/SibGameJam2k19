using UnityEngine;
using EventSys;

public sealed class HelpScreen : MonoBehaviour {
	public float MinDelay = 0.5f;

	float _startTime = 0f;

	void Awake() {
		_startTime = Time.time;
	}

	public void OnClickHelpScreen() {
		if ( Time.time < _startTime + MinDelay ) {
			return;
		}
		EventManager.Fire(new Event_HelpScreenClosed());
		gameObject.SetActive(false);
	}
}
