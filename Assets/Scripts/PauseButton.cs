using UnityEngine;

public class PauseButton : MonoBehaviour {
	public void OnClickPause() {
		GameState.Instance.TimeController.AddOrRemovePause(this);
	}
}
