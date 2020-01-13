using UnityEngine;
using UnityEngine.UI;

public class FinishWindow : MonoBehaviour {

	public GameObject ApplesCounter;
	public GameObject Star;
 
	public void OnContinueClick() {
		var nextLevel = LevelSettings.Instance.NextSceneName;
		LevelManager.Instance.LoadLevel(nextLevel); 
	} 
	public void GoToStart() {
		LevelManager.Instance.LoadMainMenu();
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
}
