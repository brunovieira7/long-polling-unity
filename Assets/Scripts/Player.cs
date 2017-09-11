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

public class Player : MonoBehaviour {

	private bool isBlocked = false;
	public float playerSpeed;
	public GameObject kunai;

	private Vector3 walkDestination;

	private string player;
	private bool me = false;
	private string baseUrl;

	private Animator animator;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator>();
		//baseUrl = "https://long-polling-dot-tpserver-dev-env.appspot.com";
		baseUrl = "http://localhost:8080";
	}
	
	// Update is called once per frame
	void Update () {
		/*if (isMoving) { 
			transform.position = Vector3.MoveTowards (transform.position, walkDestination, playerSpeed * Time.deltaTime);
			if (transform.position == walkDestination) {
				isMoving = false;
				sentMove = false;
			}
		}*/
	}
		
	public void setPlayer(string p, bool self) {
		player = p;
		me = self;
	}

	public string getPlayer() {
		return player;
	}

	public bool getMe() {
		return me;
	}

	public void tryMoving(Vector2 dir) {
		if (!isBlocked) {
			SendAction ("move", dir.x, dir.y);
		}
	}

	public IEnumerator kunaiAttack() {
		if (!isBlocked) {
			isBlocked = true;
			animator.SetInteger ("state", 2);
			yield return new WaitForSeconds (0.3f);

			Vector3 start = new Vector3 (transform.localPosition.x, transform.localPosition.y, 0f);
			GameObject instance = Instantiate (kunai, start, Quaternion.identity) as GameObject;
			instance.transform.DOMove (new Vector3 (7, transform.localPosition.y, 0), 2, false);

			yield return new WaitForSeconds (0.5f);
			animator.SetInteger ("state", 0);
			isBlocked = false;
		}
	}

	public void move(float x , float y) {
		Debug.Log ("movin:" + x + "," + y);
		isBlocked = true;
		animator.SetInteger ("state", 1);

		if ( (x < transform.localPosition.x && transform.localScale.x > 0) || (x > transform.localPosition.x && transform.localScale.x < 0)){
			Vector3 scale = transform.localScale;
			scale.x *= -1;
			transform.localScale = scale;
		}

		transform.DOMove (new Vector3 (x, y, 0), 1, false).OnComplete(stopWalk);
		/*if (!isMoving) {
			isMoving = true;
			walkDestination = new Vector3 (x, y, 0);
			SendMove (transform.position.x + x,  transform.position.y + (y * 0.5f));
		}*/
	}

	public void stopWalk() {
		isBlocked = false;
		animator.SetInteger ("state", 0);
	}

	public Vector3 getPosition() {
		return transform.position;
	}

	IEnumerator WaitForWWW(WWW www) {
		yield return www;


		string txt = "";
		if (string.IsNullOrEmpty(www.error)) {
			txt = www.text;  //text of success
		}
		else
			txt = www.error;  //error
		
	}

	private void SendAction(string action, float x , float y) {
		try	{
			string ourPostData = "{\"player\":\""+ player +"\", \"action\":\""+ action + "\", \"x\": " + x + ", \"y\": " + y + " }";

			Debug.Log("sending: "+ourPostData);

			Dictionary<string,string> headers = new Dictionary<string, string>();
			headers.Add("Content-Type", "application/json");

			byte[] pData = System.Text.Encoding.ASCII.GetBytes(ourPostData.ToCharArray());

			WWW api = new WWW(baseUrl + "/action", pData, headers);
			StartCoroutine(WaitForWWW(api));
		}
		catch (UnityException ex) { Debug.Log(ex.Message);
		}
	}
}
