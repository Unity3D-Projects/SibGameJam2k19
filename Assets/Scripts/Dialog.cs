using UnityEngine;
using UnityEngine.UI;

using System;

public class Dialog : MonoBehaviour
{

	public Text TextLeft = null;
	public Text TextRight = null;
	public Transform BubbleLeft = null;
	public Transform BubbleRight = null;
	public int CurrentPhrase = 0;
	public float LastPhraseTime = 0;
	public float TimeDeltaToComplete = 2;

	enum Actors { 
		Farmer,
		Goat 
	}

	Tuple<Actors, string>[] Dialog1 = {
		Tuple.Create(Actors.Farmer, "FarmerPhrase1" ),
		Tuple.Create(Actors.Farmer, "FarmerPhrase2" ),
		Tuple.Create(Actors.Goat, "GoatPhrase1" ),
		Tuple.Create(Actors.Farmer, "FarmerPhrase3" ),
		Tuple.Create(Actors.Goat, "GoatPhrase2" ),
	};





	// Start is called before the first frame update
	void Start() {
		GameState.Instance.TimeController.AddPause(this);
		if ( BubbleLeft != null ) {
			TextLeft = BubbleLeft.GetChild(0).GetComponent<Text>();
		}
		if ( BubbleRight != null ) {
			TextRight = BubbleRight.GetChild(0).GetComponent<Text>();
		}
		ShowNextPhrase();
        
    }

	void ShowNextPhrase() {
		if ( Dialog1[CurrentPhrase].Item1 == Actors.Farmer ) {
			TextLeft.text = Dialog1[CurrentPhrase].Item2;
			BubbleLeft.gameObject.SetActive(true);
			BubbleRight.gameObject.SetActive(false); 
			SoundManager.Instance.PlaySound("Mumbling");
		} else {
			TextRight.text = Dialog1[CurrentPhrase].Item2;
			BubbleRight.gameObject.SetActive(true);
			BubbleLeft.gameObject.SetActive(false);
			SoundManager.Instance.PlaySound("Goat1");
		};
		CurrentPhrase++;
		if ( CurrentPhrase == Dialog1.Length ) {
			LastPhraseTime = Time.time;
		}
	}
	void UpdateDialog() {
		if ( LastPhraseTime > 0 ) {
			if ( Time.time > LastPhraseTime + TimeDeltaToComplete || Input.GetKeyDown(KeyCode.Mouse0)) {
				CompleteDialog();
			}
		} else {
			if ( Input.GetKeyDown(KeyCode.Mouse0) ) {
				ShowNextPhrase();
			}; 
		}
	}

	void CompleteDialog() {
		Debug.Log("Dialog completed");
		this.gameObject.SetActive(false);
		GameState.Instance.TimeController.RemovePause(this); 
	}

    // Update is called once per frame
    void Update()
    {
		UpdateDialog(); 
    }
}
