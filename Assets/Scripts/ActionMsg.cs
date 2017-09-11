using System;

[System.Serializable]
public class ActionMsg {
	public string player  { get; set; }
	public string action  { get; set; }
	public float x { get; set; }
	public float y { get; set; }

	public ActionMsg() { }

	public ActionMsg(string player, string action, float x, float y) {
		this.player = player;
		this.action = action;
		this.x = x;
		this.y = y;
	}

}

