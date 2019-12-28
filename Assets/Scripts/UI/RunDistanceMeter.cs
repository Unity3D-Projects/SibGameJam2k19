using UnityEngine;

using SMGCore;

using TMPro;

public class RunDistanceMeter : MonoBehaviour {
	public TMP_Text DistanceText = null;

	LocalizationController _lc   = null;
	GoatController         _goat = null;

	void Start() {
		_lc = LocalizationController.Instance;
		_goat = GameState.Instance.Goat;
		var ls = LevelSettings.Instance;
		if ( !ls.Endless ) {
			gameObject.SetActive(false);
			return;
		}
	}

	void Update() {
		var distString = _lc.Translate("Gameplay.RunDistanceMeters");
		DistanceText.text = string.Format(distString, _goat.RunDistance.ToString("0.0"));
	}
}
