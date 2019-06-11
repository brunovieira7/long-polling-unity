using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using SimpleJSON;

public class GameManager : MonoBehaviour {

	private List<Player> players;
	private List<Enemy> enemies;

	private Player me;

	public GameObject player;
	public GameObject enemy;
	public GameObject redfloor;

	private bool timedOut;

	private int indexMessage;

	private string baseUrl;

	// Use this for initialization
	void Start () {
		baseUrl = "https://long-polling-dot-tpserver-dev-env.appspot.com";
		//baseUrl = "http://localhost:8080";
		players = new List<Player> ();
		enemies = new List<Enemy> ();
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
			StartCoroutine (GetActions ());
			timedOut = false;
		}

//		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
//			if (Input.touchCount > 0) {
//				if (Input.GetTouch (0).phase == TouchPhase.Began) {
//					checkTouch (Input.GetTouch (0).position);
//				}
//			}
//		} else if (Application.platform == RuntimePlatform.WindowsEditor) {
//			if (Input.GetMouseButtonDown (0)) {
//				checkTouch (Input.mousePosition);
//			}
//		}
	}

	public void checkTouch(Vector3 pos){
		Vector3 wp = Camera.main.ScreenToWorldPoint(pos);
		Vector2 touchPos = new Vector2(wp.x, wp.y);
	     var hit = Physics2D.OverlapPoint(touchPos);
	     
	     if (hit) {
			if (!me.isBlocked ()) {
				if (hit.tag == "kunaiatk") {
					//StartCoroutine (me.kunaiAttack ());
					SendAction ("kunai", me.transform.localScale.x, 0f);
					return;
				}

				Vector2 movePos = hit.GetComponent<Arrow> ().getMovePos ();
				SendAction ("move", movePos.x, movePos.y);
			}
	     }
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

	public void SendAction(string action, float x , float y) {
		try	{
			string ourPostData = "{\"player\":\""+ me.getName() +"\", \"action\":\""+ action + "\", \"x\": " + x + ", \"y\": " + y + " }";

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

	IEnumerator GetPlayer() {
		//Debug.Log("trying to get message");
		//string text = "x:" + x + "y:" + y;
		using (UnityWebRequest www = UnityWebRequest.Get(baseUrl + "/player"))
		{
			www.timeout = 10;
			yield return www.Send();

			if (www.isNetworkError) {
				Debug.Log(www.error + " " + www.responseCode);
			}
			else {

				Debug.Log("response GETPLAYER ["+www.downloadHandler.text+"]");

				if (www.downloadHandler.text != "") {
					JSONNode node = new JSONArray();
					node = JSON.Parse(www.downloadHandler.text);
					ActionMsg mm = new ActionMsg(node["player"], node["action"], node["x"], node["y"]);
					//for (int i = 0; i < node.Count; i++) {
					//	Debug.Log (node[i]);
					//}
					Vector3 start = new Vector3 (mm.x, mm.y, 0f);
					GameObject instance = Instantiate (player, start, Quaternion.identity) as GameObject;

					Player scr = instance.GetComponent<Player> ();
					scr.setName (mm.player, true);

					players.Add (scr);
					me = scr;
				}
			}
		}
	}

	IEnumerator GetActions() {
		//Debug.Log("trying to get message");
		//string text = "x:" + x + "y:" + y;
		using (UnityWebRequest www = UnityWebRequest.Get(baseUrl + "/action?index=" + indexMessage))
		{
			www.timeout = 50;
			yield return www.Send();

			if (www.isNetworkError) {
				Debug.Log(www.error + " " + www.responseCode);
			}
			else {

				//Debug.Log("response ["+www.downloadHandler.text+"]");

				if (www.downloadHandler.text != "" && www.downloadHandler.text != "[]") {
					JSONNode node = new JSONArray();
					node = JSON.Parse(www.downloadHandler.text);
					for (int i = 0; i < node.Count; i++) {
						Debug.Log ("NEW msg: " + node[i]["player"] +","+ node[i]["action"] +","+node[i]["x"]+","+  node[i]["y"]);
						ActionMsg mm = new ActionMsg(node[i]["player"], node[i]["action"], node[i]["x"], node[i]["y"]);
						processAction (mm);
					}

					indexMessage += node.Count;

					//indexMessage++;
					//Debug.Log("addind index "+node.Count);
				}

			}

			timedOut = true;
		}
	}

	private void processAction(ActionMsg mm) {
		foreach(Player scr in players) {
			//Debug.Log("---->"+scr.getPlayer()+" == "+mm.player);
			if (scr.getName() == mm.player) {
				scr.act (mm);
				return;
			}
		}
		foreach(Enemy scr in enemies) {
			//Debug.Log("---->"+scr.getPlayer()+" == "+mm.player);
			if (scr.getName() == mm.player) {
				scr.act (mm);
				return;
			}
		}

		//cant find player, instantiate new
		if (mm.player.StartsWith ("p")) {
			Vector3 start = new Vector3 (mm.x, mm.y, 0f);
			GameObject instance = Instantiate (player, start, Quaternion.identity) as GameObject;

			Player play = instance.GetComponent<Player> ();
			play.setName (mm.player, false);

			players.Add (play);
		} else if (mm.player.StartsWith ("x")) {
			Vector3 start = new Vector3 (mm.x, mm.y-0.5f, 0f);
			GameObject instance = Instantiate (redfloor, start, Quaternion.identity) as GameObject;
			Vector3 scale = instance.transform.localScale;
			scale.x *= 3;
			scale.y *= 3;
			instance.transform.localScale = scale;
			instance.GetComponent<blinkred> ().setTimeToDie (3f);

			StartCoroutine (enemies [0].beginCast ());
		} else if (mm.player.StartsWith ("e")) {
			Vector3 start = new Vector3 (mm.x, mm.y, 0f);
			GameObject instance = Instantiate (enemy, start, Quaternion.identity) as GameObject;

			Enemy ene = instance.GetComponent<Enemy> ();
			ene.setName (mm.player, false);

			enemies.Add (ene);

			enemies.Add(instance.GetComponent<Enemy> ());
		} else if (mm.player.StartsWith ("s")) {
			foreach(Enemy scr in enemies) {
				//Debug.Log("---->"+scr.getPlayer()+" == "+mm.player);
				//if (scr.getPlayer() == mm.player) {
				StartCoroutine (scr.castSpell(mm));
					return;
				//}
			}
		}


	}
}
