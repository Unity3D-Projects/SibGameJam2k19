using UnityEngine;

using SMGCore;

public class PoolItem : MonoBehaviour, IPoolItem {
	Vector3 _restartPosition = new Vector3(0, -1, 0);
	public virtual void DeInit() {
		transform.position = _restartPosition;
	}
}
