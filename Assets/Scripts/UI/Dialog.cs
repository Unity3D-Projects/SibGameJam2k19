using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using SMGCore;
using SMGCore.EventSys;
using KOZA.Events;

using DG.Tweening;

public sealed class Dialog : MonoBehaviour {
	public List<DialogPhrase> DialogDescription   = new List<DialogPhrase>();
	public DialogActor        LeftActor           = null;
	public DialogActor        RightActor          = null;
	public float              TimeDeltaToComplete = 2;

	Text TextLeft     = null;
	Text TextRight    = null;
	Text NameTagLeft  = null;
	Text NameTagRight = null; 

	int   _currentPhrase  = 0;
	float _lastPhraseTime = 0;

	public enum Sides {
		Left,
		Right
	}

	[System.Serializable]
	public class DialogPhrase {
		public Sides  Side     = Sides.Left;
		public string PhraseId = null;
	}

	[System.Serializable]
	public class DialogActor {
		public string NameId = null;
		public string Sound  = null;
		public GameObject Image = null;
		public Vector2 ImagePos;
		public bool FlipOverY;

		public void SetUp(Transform t) {
			Image = Instantiate(Image, t);
			Image.transform.localPosition = ImagePos;
			Image.transform.SetAsFirstSibling();
			if ( FlipOverY ) {
				Image.transform.Rotate(new Vector3(0, 180, 0));
			} 
		}
	}

	void Start() {

		LeftActor.SetUp(transform);
		RightActor.SetUp(transform);

		var p = transform.parent;
		var pp = p.Find("Plaque");

		TextLeft = p.Find("TextLeft").GetComponent<Text>();
		TextRight = p.Find("TextRight").GetComponent<Text>();
		NameTagLeft = pp.Find("NameTagLeft").Find("Text").GetComponent<Text>();
		NameTagRight = pp.Find("NameTagRight").Find("Text").GetComponent<Text>(); 

		var lc = LocalizationController.Instance;
		NameTagLeft.text = lc.Translate(LeftActor.NameId);
		NameTagRight.text = lc.Translate(RightActor.NameId); 

		GameState.Instance.TimeController.AddPause(this);
		ShowNextPhrase();
    }

	void Update() {
		UpdateDialog();
	}

	void ShowNextPhrase() {
		var lc = LocalizationController.Instance;

		Sides side = DialogDescription[_currentPhrase].Side;
		if ( side== Sides.Left ) {
			TextLeft.text = lc.Translate(DialogDescription[_currentPhrase].PhraseId);
			TextLeft.gameObject.SetActive(true);
			TextRight.gameObject.SetActive(false);
			SoundManager.Instance.PlaySound(LeftActor.Sound, Random.Range(0.85f, 1f), Random.Range(0.9f, 1.1f));
			LeftActor.Image.GetComponent<RectTransform>().DOShakeAnchorPos(1f, 10f, 10);
		} else {
			TextRight.text = lc.Translate(DialogDescription[_currentPhrase].PhraseId);
			TextRight.gameObject.SetActive(true);
			TextLeft.gameObject.SetActive(false);
			SoundManager.Instance.PlaySound(RightActor.Sound, Random.Range(0.85f, 1f), Random.Range(0.9f, 1.1f));
			RightActor.Image.GetComponent<RectTransform>().DOShakeAnchorPos(1f, 10f, 10);

		}
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
		transform.parent.gameObject.SetActive(false);
		GameState.Instance.TimeController.RemovePause(this);
		EventManager.Fire(new Event_StartDialogComplete());
	}
}
