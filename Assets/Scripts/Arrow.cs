using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour {

	public float x;
	public float y;

	public GameManager manager;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public Vector2 getMovePos() {
		return new Vector2(x, y);
	}

	private void OnMouseDown()
	{
		manager.SendAction ("move", x, y);
	}
}
