using UnityEngine;
using UnityEngine.UI;

using SMGCore;

public class FinishWindow : MonoBehaviour {
	public GameObject ApplesCounter = null;
	public GameObject Star          = null;
	public GameObject ResetButton   = null;
 
	public void OnContinueClick() {
		var nextLevel = LevelSettings.Instance.NextSceneName;
		LevelManager.Instance.LoadLevel(nextLevel); 
	} 
	public void GoToStart() {
		LevelManager.Instance.LoadMainMenu();
	}
	public void FastRestart() {
		var persistence = ScenePersistence.Instance.Data as KOZAPersistence;
		persistence.FastRestart = true;
		//Fader.OnFadeToBlackFinished.AddListener(() => LevelManager.Instance.LoadLevel(persistence.LastLevelName));
		LevelManager.Instance.LoadLevel(persistence.LastLevelName);
		AdvertisementController.HideBannerAd();
	}

	public void UpdateApplesCounter() {
		ApplesCounter.GetComponent<Text>().text = string.Format("{0}|{1}",GameState.Instance.ApplesCounter.ToString("0"), LevelSettings.Instance.ApplesNumber.ToString("0"));
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
		} else {
			ResetButton.SetActive(true);
		}
	}
}
