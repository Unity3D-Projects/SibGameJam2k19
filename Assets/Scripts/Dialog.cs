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

	enum Faces { 
		Farmer,
		Goat 
	}

	Tuple<Faces, string>[] Dialog1 = {
		Tuple.Create(Faces.Farmer, "Phrase1" ),
		Tuple.Create(Faces.Farmer, "Phrase2" ),
		Tuple.Create(Faces.Goat, "Phrase1" ),
		Tuple.Create(Faces.Farmer, "Phrase3" ),
		Tuple.Create(Faces.Goat, "Phrase2" ),
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
		if ( Dialog1[CurrentPhrase].Item1 == Faces.Farmer ) {
			TextLeft.text = Dialog1[CurrentPhrase].Item2;
			BubbleLeft.gameObject.SetActive(true);
			BubbleRight.gameObject.SetActive(false); 
		} else {
			TextRight.text = Dialog1[CurrentPhrase].Item2;
			BubbleRight.gameObject.SetActive(true);
			BubbleLeft.gameObject.SetActive(false); 
		};
		CurrentPhrase++;
		if ( CurrentPhrase == Dialog1.Length ) {
			CompleteDialog();
		}
	}
	void UpdateDialog() {
		if ( Input.GetKeyDown(KeyCode.Mouse0) ) {
			Debug.Log("MOUSE1");
			ShowNextPhrase();
		} ; 
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
