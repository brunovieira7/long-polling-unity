using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

	public GameObject snakeSpell;
	private Animator animator;

	void Start () {
		animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public IEnumerator castSpell(ActionMsg mm) { 
		Vector3 start = new Vector3 (mm.x, mm.y, 0f);
		GameObject instance = Instantiate (snakeSpell, start, Quaternion.identity) as GameObject;

		yield return new WaitForSeconds (1.2f);
		Destroy (instance);
	}

	public IEnumerator beginCast() {
		Debug.Log ("why no cast");
		animator.SetInteger ("state", 2);
		yield return new WaitForSeconds (1.5f);
		animator.SetInteger ("state", 0);
	}
}
