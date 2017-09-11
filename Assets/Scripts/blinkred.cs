using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class blinkred : MonoBehaviour {

	SpriteRenderer spriteRenderer;
	private float alphaLevel;
	private bool up;

	void Start () {
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		alphaLevel = 0.4f;
		up = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.frameCount % 15 == 0) {
			if (alphaLevel > 0.4f || alphaLevel < 0.2f)
				up = !up;

			if (up)
				alphaLevel += 0.1f;
			else
				alphaLevel -= 0.1f;

			spriteRenderer.color = new Color (1f, 1f, 1f, alphaLevel);
		}
	}
}
