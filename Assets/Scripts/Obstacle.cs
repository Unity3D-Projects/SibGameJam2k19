using UnityEngine;
using EventSys;


public class Obstacle : MonoBehaviour
{ 
	private void OnCollisionEnter2D(Collision2D collision) { 
		EventManager.Fire(new Event_Obstacle_Collided { Obstacle = this }); 
	}
}
