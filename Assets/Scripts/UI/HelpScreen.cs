using UnityEngine;

using SMGCore.EventSys;
using KOZA.Events;

public sealed class HelpScreen : MonoBehaviour {
	public float MinDelay = 0.5f;

	float _startTime = 0f;

	void Awake() {
		GameState.Instance.TimeController.AddPause(this);
		_startTime = Time.time;
	}

	public void OnClickHelpScreen() {
		if ( Time.time < _startTime + MinDelay ) {
			return;
		}
		GameState.Instance.TimeController.RemovePause(this);
		EventManager.Fire(new Event_HelpScreenClosed());
		gameObject.SetActive(false);
	}
}
