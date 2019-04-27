using UnityEngine;
using EventSys;


public class Obstacle : MonoBehaviour
{ 
	private void OnTriggerEnter2D() {
		Debug.Log("trigger enter"); 
		EventManager.Fire(new Event_Obstacle_Collided { Obstacle = this }); 
	}
}
