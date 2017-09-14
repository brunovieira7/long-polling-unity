using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class blinkred : MonoBehaviour {

	SpriteRenderer spriteRenderer;
	private float alphaLevel;
	private bool up;
	private float timeToDie;
	private float time;

	void Start () {
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		alphaLevel = 0.4f;
		up = false;
		time = 0f;
	}

	public void setTimeToDie(float time) {
		timeToDie = time;
	}

	// Update is called once per frame
	void Update () {
		time += Time.deltaTime;
		if (time > timeToDie) {
			Destroy (gameObject);
		}

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
