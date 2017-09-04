using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using SimpleJSON;

public class GameManager : MonoBehaviour {

	private List<Player> players;

	public GameObject player;

	private bool timedOut;

	private int indexMessage;

	// Use this for initialization
	void Start () {
		players = new List<Player> ();
		timedOut = true;
		indexMessage = 0;

		StartCoroutine (GetPlayer ());

		///LOCAL ===============================
		//timedOut = false;
		//localGetPlayer();
	}
	
	// Update is called once per frame
	void Update () {
		if (timedOut) {
			StartCoroutine (GetMove ());
			timedOut = false;
		}

		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
			if (Input.touchCount > 0) {
				if (Input.GetTouch (0).phase == TouchPhase.Began) {
					checkTouch (Input.GetTouch (0).position);
				}
			}
		} else if (Application.platform == RuntimePlatform.WindowsEditor) {
			if (Input.GetMouseButtonDown (0)) {
				checkTouch (Input.mousePosition);
			}
		}
	}

	private void checkTouch(Vector3 pos){
		Vector3 wp = Camera.main.ScreenToWorldPoint(pos);
		Vector2 touchPos = new Vector2(wp.x, wp.y);
	     var hit = Physics2D.OverlapPoint(touchPos);
	     
	     if (hit) {
			Vector2 movePos = hit.GetComponent<Arrow> ().getMovePos();
			//Debug.Log(movePos);

			foreach (Player p in players) {
				if (p.getMe()) {
					p.tryMoving(movePos);
					//Vector3 ppos = p.getPosition();

					///LOCAL ===============================
					//MoveMsg mm = new MoveMsg("p1", ppos.x + movePos.x, ppos.y + movePos.y);
					//movePlayer (mm);
				}
			}
	     }
	}

/*	void localGetPlayer() {
		Vector3 start = new Vector3 (-7f, 0f);
		GameObject instance = Instantiate (player, start, Quaternion.identity) as GameObject;

		Player scr = instance.GetComponent<Player> ();
		scr.setPlayer ("p1", true);

		players.Add (scr);
	}*/

	IEnumerator GetPlayer() {
		//Debug.Log("trying to get message");
		//string text = "x:" + x + "y:" + y;
		using (UnityWebRequest www = UnityWebRequest.Get("https://long-polling-dot-tpserver-dev-env.appspot.com/player"))
		{
			www.timeout = 10;
			yield return www.Send();

			if (www.isError) {
				Debug.Log(www.error + " " + www.responseCode);
			}
			else {

				Debug.Log("response ["+www.downloadHandler.text+"]");

				if (www.downloadHandler.text != "") {
					JSONNode node = new JSONArray();
					node = JSON.Parse(www.downloadHandler.text);
					MoveMsg mm = new MoveMsg(node["player"], node["x"], node["y"]);
					//for (int i = 0; i < node.Count; i++) {
					//	Debug.Log (node[i]);
					//}
					Vector3 start = new Vector3 (mm.x, mm.y, 0f);
					GameObject instance = Instantiate (player, start, Quaternion.identity) as GameObject;

					Player scr = instance.GetComponent<Player> ();
					scr.setPlayer (mm.player, true);

					players.Add (scr);
				}
			}
		}
	}

	IEnumerator GetMove() {
		//Debug.Log("trying to get message");
		//string text = "x:" + x + "y:" + y;
		using (UnityWebRequest www = UnityWebRequest.Get("https://long-polling-dot-tpserver-dev-env.appspot.com/move?moveIndex=" + indexMessage))
		{
			www.timeout = 50;
			yield return www.Send();

			timedOut = true;
			if (www.isError) {
				Debug.Log(www.error + " " + www.responseCode);
			}
			else {

				//Debug.Log("response ["+www.downloadHandler.text+"]");

				if (www.downloadHandler.text != "" && www.downloadHandler.text != "[]") {
					JSONNode node = new JSONArray();
					node = JSON.Parse(www.downloadHandler.text);
					for (int i = 0; i < node.Count; i++) {
						Debug.Log (node[i]["player"] +","+ node[i]["x"]+","+  node[i]["y"]);
						MoveMsg mm = new MoveMsg(node[i]["player"], node[i]["x"], node[i]["y"]);
						movePlayer (mm);
					}

					indexMessage += node.Count;
					//indexMessage++;
					Debug.Log("addind index "+node.Count);
				}

			}
		}
	}

	private void movePlayer(MoveMsg mm) {
		foreach(Player scr in players) {
			Debug.Log("---->"+scr.getPlayer()+" == "+mm.player);
			if (scr.getPlayer() == mm.player) {
				scr.move (mm.x, mm.y);
				return;
			}
		}

		//cant find player, instantiate new
		Vector3 start = new Vector3 (mm.x, mm.y, 0f);
		GameObject instance = Instantiate (player, start, Quaternion.identity) as GameObject;

		Player play = instance.GetComponent<Player> ();
		play.setPlayer (mm.player, false);

		players.Add (play);
	}

	[System.Serializable]
	public class MoveMsg {
		public string player  { get; set; }
		public float x { get; set; }
		public float y { get; set; }

		public MoveMsg() { }

		public MoveMsg(string player, float x, float y) {
			this.player = player;
			this.x = x;
			this.y = y;
		}

	}
}
