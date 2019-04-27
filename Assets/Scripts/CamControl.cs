using UnityEngine;

public sealed class CamControl : MonoBehaviour {

	public static CamControl Instance;

	public AnimationCurve lerpCoef;
	public Transform player;

	public float initDelta = 10f;

	Camera _camera;

	float _initZ = 0;
	float _moveError;

	void Awake() {
		Instance = this;
	}

	void Start() {
		_initZ = transform.position.z;
		_camera = GetComponent<Camera>();
	}


	void LateUpdate() {
		_moveError = Vector3.Distance(transform.position, player.position) - initDelta;
		float cLerp = lerpCoef.Evaluate(_moveError);
		Vector3 newPos = Vector3.Lerp(transform.position, player.position, cLerp);
		newPos.z = _initZ;
		transform.position = newPos;
	}

	public float Map(float value, float fromSource, float toSource, float fromTarget, float toTarget, bool clamp = false) {
		var val =  (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
		if ( clamp ) {
			val = Mathf.Clamp(val, fromTarget, toTarget);
		}
		return val;
	}

	public void ReplaceTargetByDummy() {
		var dummy = new GameObject("[camDummy]");
		dummy.transform.position = player.transform.position;
		player = dummy.transform;
	}
}
