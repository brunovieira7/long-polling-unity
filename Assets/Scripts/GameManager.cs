using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using SimpleJSON;

using Colyseus;
using Colyseus.Schema;

using GameDevWare.Serialization;

public class GameManager : MonoBehaviour {

	private List<Player> players;
	private Dictionary<string, Player> playerz = new Dictionary<string, Player>();
	private List<Enemy> enemies;

	private Player me;

	public GameObject player;
	public GameObject enemy;
	public GameObject redfloor;

	private bool timedOut;

	private int indexMessage;

	private string baseUrl;
	protected Room<State> room;

	private string clientId;

	// Use this for initialization
	IEnumerator Start () {
		

		baseUrl = "https://long-polling-dot-tpserver-dev-env.appspot.com";
		//baseUrl = "http://localhost:8080";
		players = new List<Player> ();
		enemies = new List<Enemy> ();
		timedOut = true;
		indexMessage = 0;

		playerz = new Dictionary<string, Player>();

		room = RoomManager.room;
		room.State.entities.OnChange += OnEntityMove;
		room.State.entities.OnAdd += OnEntityAdd;
		room.State.entities.OnRemove += OnEntityRemove;

		clientId = RoomManager.client.Id;

		room.OnStateChange += OnStateChangeHandler;

		foreach (string key in room.State.entities.Items.Keys)
		{
			Entity ent = (Entity) room.State.entities.Items[key];

			Debug.Log("has ent " + ent.type);
			Vector3 start = new Vector3 (ent.x, ent.y, 0f);
			GameObject instance = Instantiate (player, start, Quaternion.identity) as GameObject;

			Player scr = instance.GetComponent<Player> ();
			scr.setName (ent.type, true);

			players.Add (scr);
			playerz.Add(key, scr);

			if (key == clientId) {
				me = scr;
			}
		}

		//StartCoroutine (GetPlayer ());

		///LOCAL ===============================
		//timedOut = false;
		//localGetPlayer();

		while (true)
		{
			if (RoomManager.client != null)
			{
				RoomManager.client.Recv();
			}
			yield return 0;
		}

	}

	void OnStateChangeHandler (object sender, StateChangeEventArgs<State> e)
	{
		// Setup room first state
		Debug.Log("State has been updated!");
	}

	void OnEntityAdd(object sender, KeyValueEventArgs<Entity, string> item)
	{
		Debug.Log("add --- " + item.Value.x);
	}

	void OnEntityRemove(object sender, KeyValueEventArgs<Entity, string> item)
	{
		Debug.Log("remove --- " + item.Value.x);
	}

	public void OnEntityMove (object sender, KeyValueEventArgs<Entity, string> item)
	{
		Debug.Log("changed --- " + item.Key);
		playerz[item.Key].move(item.Value.x, item.Value.y);
	}

	public void SendMessage(string action, float x , float y)
	{
		if (!me.isBlocked()){
			if (room != null)
			{
				room.Send(new { action = action, x = x, y = y });
			}
			else
			{
				Debug.Log("Room is not connected!");
			}
		}
	}

	// Update is called once per frame
	void Update () {
		if (timedOut) {
			StartCoroutine (GetActions ());
			timedOut = false;
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
		if (action == "kunai")
			x = me.transform.localScale.x;

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
		//pegar no client o id, usar como indice do mapa de entities
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
