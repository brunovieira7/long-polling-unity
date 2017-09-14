using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Mob : MonoBehaviour {

	protected bool blocked = false;
	protected Animator animator;
	protected string nameId;
	protected bool me = false;
	protected bool flipped = false;

	// Use this for initialization
	public void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void move(float x , float y) {
		blocked = true;
		animator.SetInteger ("state", 1);

		if (needsFlip(x)){
			Vector3 scale = transform.localScale;
			scale.x *= -1;
			transform.localScale = scale;
		}
		transform.DOMove (new Vector3 (x, y, 0), 1, false).OnComplete(stopWalk);
	}

	public bool needsFlip(float x) {
		if (((x < transform.localPosition.x && transform.localScale.x > 0) || (x > transform.localPosition.x && transform.localScale.x < 0)) && !flipped)
			return true;
		else if (((x < transform.localPosition.x && transform.localScale.x < 0) || (x > transform.localPosition.x && transform.localScale.x > 0)) && flipped)
			return true;

		return false;
	}

	public void stopWalk() {
		blocked = false;
		animator.SetInteger ("state", 0);
	}

	public void setName(string p, bool self) {
		nameId = p;
		me = self;
	}

	public string getName() {
		return nameId;
	}

	public bool isMe() {
		return me;
	}
}
