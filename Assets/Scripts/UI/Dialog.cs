using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using EventSys;

using DG.Tweening;

public sealed class Dialog : MonoBehaviour {
	public Image              LeftActorImage      = null;
	public Image              RightActorImage     = null;
	public Text               TextLeft            = null;
	public Text               TextRight           = null;
	public List<DialogPhrase> DialogDescription   = new List<DialogPhrase>();
	public float              TimeDeltaToComplete = 2;

	int   _currentPhrase  = 0;
	float _lastPhraseTime = 0;

	public enum DialogActor { 
		Farmer,
		Goat 
	}

	[System.Serializable]
	public class DialogPhrase {
		public DialogActor Actor    = DialogActor.Farmer;
		public string      PhraseId = null;
	}

	void Start() {
		GameState.Instance.TimeController.AddPause(this);
		ShowNextPhrase();
    }

	void Update() {
		UpdateDialog();
	}

	void ShowNextPhrase() {
		var lc = LocalizationController.Instance;
		if ( DialogDescription[_currentPhrase].Actor == DialogActor.Farmer ) {
			TextLeft.text = lc.Translate(DialogDescription[_currentPhrase].PhraseId);
			TextLeft.gameObject.SetActive(true);
			TextRight.gameObject.SetActive(false); 
			SoundManager.Instance.PlaySound("Mumbling", Random.Range(0.85f, 1f), Random.Range(0.9f, 1.1f));
			LeftActorImage.rectTransform.DOShakeAnchorPos(1f, 10f, 10);
		} else {
			TextRight.text = lc.Translate(DialogDescription[_currentPhrase].PhraseId);
			TextRight.gameObject.SetActive(true);
			TextLeft. gameObject.SetActive(false);
			SoundManager.Instance.PlaySound("Goat1", Random.Range(0.85f, 1f), Random.Range(0.9f, 1.1f));
			RightActorImage.rectTransform.DOShakeAnchorPos(1f, 10f, 10);
		};
		_currentPhrase++;
		if ( _currentPhrase == DialogDescription.Count ) {
			_lastPhraseTime = Time.time;
		}
	}

	void UpdateDialog() {
		if ( _lastPhraseTime > 0 ) {
			if ( Time.time > _lastPhraseTime + TimeDeltaToComplete || Input.GetKeyDown(KeyCode.Mouse0)) {
				CompleteDialog();
			}
		} else {
			if ( Input.GetKeyDown(KeyCode.Mouse0) ) {
				ShowNextPhrase();
			}; 
		}
	}

	void CompleteDialog() {
		gameObject.SetActive(false);
		GameState.Instance.TimeController.RemovePause(this);
		EventManager.Fire(new Event_StartDialogComplete());
	}
}
