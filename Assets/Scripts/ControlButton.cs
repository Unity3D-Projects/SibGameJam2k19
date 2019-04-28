using UnityEngine;
using EventSys;

public class ControlButton : MonoBehaviour
{
	public enum ControlButtonType {
		Jump,
		Slide,
		Yell
	}

	public ControlButtonType ButtonType; 

	public void ControlButtonClick() {
		if ( ButtonType == ControlButtonType.Jump ) {
			EventManager.Fire(new Event_JumpButtonPushed { });
		} else if ( ButtonType == ControlButtonType.Slide ) {
			EventManager.Fire(new Event_SlideButtonPushed { });
		} else if ( ButtonType == ControlButtonType.Yell ) {
			EventManager.Fire(new Event_YellButtonPushed { });
		};
	}
	public void ControlButtonRelease() {
		if ( ButtonType == ControlButtonType.Slide ) {
			EventManager.Fire(new Event_SlideButtonReleased { });
		};
	}
}
