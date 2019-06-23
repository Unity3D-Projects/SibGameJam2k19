using UnityEngine;

using SMGCore.EventSys;
using KOZA.Events;

public sealed class ControlButton : MonoBehaviour {
	public enum ControlButtonType {
		Jump,
		Slide,
		Yell
	}

	public ControlButtonType ButtonType;
	bool _pushed = false;

	public void ControlButtonClick() {
		_pushed = true;
	}
	public void ControlButtonRelease() {
		_pushed = false;
		if ( ButtonType == ControlButtonType.Slide ) {
			EventManager.Fire(new Event_SlideButtonReleased { });
		};
	}

	void Update() {
		if ( !_pushed ) {
			return;
		}
		if ( ButtonType == ControlButtonType.Jump ) {
			EventManager.Fire(new Event_JumpButtonPushed { });
		} else if ( ButtonType == ControlButtonType.Slide ) {
			EventManager.Fire(new Event_SlideButtonPushed { });
		} else if ( ButtonType == ControlButtonType.Yell ) {
			EventManager.Fire(new Event_YellButtonPushed { });
		};
	}
}
