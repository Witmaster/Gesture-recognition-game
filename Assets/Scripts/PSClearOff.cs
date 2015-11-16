using UnityEngine;
using System.Collections;

public class PSClearOff : MonoBehaviour {

    private float deathTime = 0.0f;
	// Use this for initialization
	void Start () {
        deathTime = Time.time + 2;
	}
	
	// Update is called once per frame
	void Update () {
	 if (deathTime < Time.time)
        {
            Destroy(gameObject);
        }
	}
}
