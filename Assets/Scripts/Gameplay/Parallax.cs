using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour {
	public Transform Camera = null;
	public List<Transform> Frames = new List<Transform>();
	public float ScrollSpeed = 0.1f;
	public float LeftBound = - 200f;

	protected const float minMoveDistance = 0.001f;

	void Start() {

	}


	void LateUpdate() {
		if ( Camera == null ) {
			Camera = GameState.Instance.CamControl.gameObject.transform; 
		} else {
			if ( GameState.Instance.TimeController.IsPause ) {
				return;
			}
			float dX = Camera.position.x - transform.position.x;
			if ( dX > minMoveDistance ) {
				transform.position = new Vector2(Camera.position.x, transform.position.y);
				Vector3 shift = new Vector3(dX * ScrollSpeed, 0);
				Frames[0].localPosition -= shift;
				Frames[1].localPosition -= shift;
				for ( int i = 0; i < Frames.Count; i++ ) {
					if ( Frames[i].localPosition.x <= LeftBound ) {
						int index = (Frames.Count + (i - 1)%Frames.Count) % Frames.Count;
						Frames[i].localPosition = Frames[index].localPosition + new Vector3(Frames[index].GetComponent<Renderer>().bounds.size.x, 0, 0);
						break;
					}
				}
			}

		}
	}
}
