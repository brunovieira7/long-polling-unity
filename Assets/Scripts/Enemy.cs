using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Mob {

	public GameObject snakeSpell;
	public GameObject lifeBar;
	//private Animator animator;

	void Start () {
		animator = GetComponent<Animator>();
		flipped = true;
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
		animator.SetInteger ("state", 2);
		yield return new WaitForSeconds (1.3f);
		animator.SetInteger ("state", 0);
	}

	public void act(ActionMsg mm) {
		if (mm.action == "move") {
			move (mm.x, mm.y);
		} else if (mm.action == "hp") {
			Vector3 scale = lifeBar.transform.localScale;
			scale.x = mm.x;
			lifeBar.transform.localScale = scale;
			//StartCoroutine (kunaiAttack (mm));
		}
	}
}
