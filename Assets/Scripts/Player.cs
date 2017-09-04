using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Pathfinding.Serialization.JsonFx;
using System.Text.RegularExpressions;
using SimpleJSON;

public class Player : MonoBehaviour {

	private bool isMoving = false;
	private bool sentMove = false;
	public float playerSpeed;

	private Vector3 walkDestination;

	private string player;
	private bool me = false;

	// Use this for initialization
	void Start () {
		playerSpeed = 2f;
	}
	
	// Update is called once per frame
	void Update () {
		if (isMoving) { 
			transform.position = Vector3.MoveTowards (transform.position, walkDestination, playerSpeed * Time.deltaTime);
			if (transform.position == walkDestination) {
				isMoving = false;
				sentMove = false;
			}
		}
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
		sentMove = true;
		///LOCAL ===============================
		SendMove (transform.position.x + dir.x,  transform.position.y + dir.y);
	}

	public void move(float x , float y) {
		Debug.Log ("movin:" + x + "," + y);
		if (!isMoving) {
			isMoving = true;
			walkDestination = new Vector3 (x, y, 0);
			//SendMove (transform.position.x + x,  transform.position.y + (y * 0.5f));
		}
	}

	public Vector3 getPosition() {
		return transform.position;
	}

	IEnumerator WaitForWWW(WWW www)
	{
		yield return www;


		string txt = "";
		if (string.IsNullOrEmpty(www.error)) {
			txt = www.text;  //text of success
		}
		else
			txt = www.error;  //error
		
	}

	IEnumerator WaitForWWWP(WWW www)
	{
		yield return www;


		string txt = "";
		if (string.IsNullOrEmpty(www.error)) {
			txt = www.text;  //text of success

			JSONNode node = new JSONArray();
			node = JSON.Parse(txt);

			Debug.Log ("Got playerID " + node["name"]);
			player = node ["name"];
		}
		else
			txt = www.error;  //error

	}

	private void SendMove(float x , float y) {
		try	{
			string ourPostData = "{\"player\":\""+ player +"\", \"x\": " + x + ", \"y\": " + y + " }";

			Debug.Log("sending: "+ourPostData);

			Dictionary<string,string> headers = new Dictionary<string, string>();
			headers.Add("Content-Type", "application/json");

			byte[] pData = System.Text.Encoding.ASCII.GetBytes(ourPostData.ToCharArray());

			WWW api = new WWW("https://long-polling-dot-tpserver-dev-env.appspot.com/move", pData, headers);
			StartCoroutine(WaitForWWW(api));
		}
		catch (UnityException ex) { Debug.Log(ex.Message);
		}
	}

	[System.Serializable]
	public class MoveMsg {
		public string player;
		public float x;
		public float y;
	}
}
