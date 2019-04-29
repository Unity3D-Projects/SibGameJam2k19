using UnityEngine;
using UnityEngine.UI;

using System;
using EventSys;

public class Dialog : MonoBehaviour
{

	public Text TextLeft = null;
	public Text TextRight = null;
	public int CurrentPhrase = 0;
	public float LastPhraseTime = 0;
	public float TimeDeltaToComplete = 2;

	enum Actors { 
		Farmer,
		Goat 
	}
    /*Base dialog:
		Tuple.Create(Actors.Farmer, "- Эй, Машка!" ),
		Tuple.Create(Actors.Goat, "- *Чего тебе?*" ),
		Tuple.Create(Actors.Farmer, "- Горилку купить хочу, а денег нема." ),
		Tuple.Create(Actors.Goat, "- А я тут причем?" ),
		Tuple.Create(Actors.Farmer, "- Порежу тебя, и на рынке мясо продам. Вот и куплю горилки." ),
		Tuple.Create(Actors.Goat, "- Чиго блееееееееее? *Убегает*" ),
     */

	Tuple<Actors, string>[] Dialog1 = {
		Tuple.Create(Actors.Farmer, "Hey ya little miss priss!" ),
		Tuple.Create(Actors.Goat, "*Make it quick*" ),
		Tuple.Create(Actors.Farmer, "Ima all out of booze so I need a few bucks." ),
		Tuple.Create(Actors.Goat, "What does it have to do with me?" ),
		Tuple.Create(Actors.Farmer, "I'm gonna sell ya sweety, on the market!" ),
		Tuple.Create(Actors.Goat, "Wha-a-a-t?!" ),
	};

	// Start is called before the first frame update
	void Start() {
		GameState.Instance.TimeController.AddPause(this);
		ShowNextPhrase();
        
    }

	void ShowNextPhrase() {
		if ( Dialog1[CurrentPhrase].Item1 == Actors.Farmer ) {
			TextLeft.text = Dialog1[CurrentPhrase].Item2;
			TextLeft.gameObject.SetActive(true);
			TextRight.gameObject.SetActive(false); 
			SoundManager.Instance.PlaySound("Mumbling");
		} else {
			TextRight.text = Dialog1[CurrentPhrase].Item2;
			TextRight.gameObject.SetActive(true);
			TextLeft.gameObject.SetActive(false);
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
		EventManager.Fire(new Event_StartDialogComplete());
	}

    // Update is called once per frame
    void Update()
    {
		UpdateDialog(); 
    }
}
