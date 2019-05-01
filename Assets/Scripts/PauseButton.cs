using UnityEngine;

public sealed class PauseButton : MonoBehaviour {
	public void OnClickPause() {
		var result =  GameState.Instance.TimeController.AddOrRemovePause(this);
		Time.timeScale = result ? 0f : 1f;
	}
}
