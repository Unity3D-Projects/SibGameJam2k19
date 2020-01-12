using UnityEngine;

public class Cloud : MonoBehaviour {

	public Vector2 _bounds;
	public float _speed;
	public Vector2 SpeedBound = new Vector2(-1, 0.3f);
	

	void Start() {
		//_bounds = new Vector2(-transform.root.GetComponent<Renderer>().bounds.extents.x, transform.root.GetComponent<Renderer>().bounds.extents.x);
		//Debug.Log(transform.root.GetComponent<Renderer>().bounds.size.x);
		//Debug.Log(transform.root.GetComponent<SpriteRenderer>().size.x);
		_bounds = new Vector2(-6.6f, 6.6f);
		_speed = Random.Range(SpeedBound.x, SpeedBound.y);
	}

	void Update() {
		transform.localPosition += new Vector3(_speed * Time.deltaTime, 0);
		if ( transform.localPosition.x < _bounds.x ) {
			transform.localPosition = new Vector3(_bounds.y, transform.localPosition.y); 
		} else if (transform.localPosition.x > _bounds.y ) {
			transform.localPosition = new Vector3(_bounds.x, transform.localPosition.y); 
		}
	}
}
