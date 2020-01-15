using UnityEngine;
using UnityEngine.UI;

using SMGCore;

public class FinishWindow : MonoBehaviour {

	public GameObject ApplesCounter;
	public GameObject Star;
	public GameObject ResetButton;
 
	public void OnContinueClick() {
		var nextLevel = LevelSettings.Instance.NextSceneName;
		LevelManager.Instance.LoadLevel(nextLevel); 
	} 
	public void GoToStart() {
		LevelManager.Instance.LoadMainMenu();
	}
	public void FastRestart() {
		//if ( _closing ) {
		//	return;
		//}
		//_closing = true;
		//Fader.FadeToBlack(1f);
		var persistence = ScenePersistence.Instance.Data as KOZAPersistence;
		persistence.FastRestart = true;
		//Fader.OnFadeToBlackFinished.AddListener(() => LevelManager.Instance.LoadLevel(persistence.LastLevelName));
		LevelManager.Instance.LoadLevel(persistence.LastLevelName); 
	}

	public void UpdateApplesCounter() {
		ApplesCounter.GetComponent<Text>().text = GameState.Instance.ApplesCounter.ToString() + " | " + LevelSettings.Instance.ApplesNumber;
	}
	public void UpdateStar() {
		if ( GameState.Instance.ObstacleHit ) {
			Star.SetActive(false); 
		} else {
			Star.SetActive(true);
		}
	}
	public void UpdateResetButton() {
		if ( GameState.Instance.ApplesCounter == LevelSettings.Instance.ApplesNumber & !GameState.Instance.ObstacleHit ) {
			ResetButton.SetActive(false);
			Debug.Log("TURNING OFF");
		} else {
			ResetButton.SetActive(true);
			Debug.Log("TURNING ON");
		}
	}
}
