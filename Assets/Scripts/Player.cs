using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SimpleJSON;
using DG.Tweening;

public class Player : Mob {

	public GameObject lifeBar;
	public GameObject kunai;

	private int kunaispeed = 15;
	private float throwheight = 0.4f;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {

	}

	public bool isBlocked() {
		return blocked;
	}

	public void blockMe() {
		blocked = true;
	}

	public IEnumerator kunaiAttack(ActionMsg mm) {
		blocked = true;
		animator.SetInteger ("state", 2);
		yield return new WaitForSeconds (0.3f);

		float yPos = transform.localPosition.y + throwheight;
		Vector3 start = new Vector3 (transform.localPosition.x, yPos, 0f);
		GameObject instance = Instantiate (kunai, start, Quaternion.identity) as GameObject;
		instance.transform.DOMove (new Vector3 (mm.x, yPos, 0), kunaispeed, false).OnComplete(() => Destroy(instance)).SetEase(Ease.Linear).SetSpeedBased();

		yield return new WaitForSeconds (0.5f);
		animator.SetInteger ("state", 0);
		blocked = false;
	}

	public void act(ActionMsg mm) {
		if (mm.action == "move") {
			move (mm.x, mm.y);
		} else if (mm.action == "kunai") {
			StartCoroutine (kunaiAttack (mm));
		} else if (mm.action == "hp") {
			Vector3 scale = lifeBar.transform.localScale;
			scale.x = mm.x;
			lifeBar.transform.localScale = scale;
			//StartCoroutine (kunaiAttack (mm));
		}
	}

	public Vector3 getPosition() {
		return transform.position;
	}
}
