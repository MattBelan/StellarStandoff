using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionBehavior : MonoBehaviour
{

    //Fields
    public ParticleSystem fire;
    public ParticleSystem smoke;
    private float timer;
    public float timerMax;

    // Use this for initialization
    void Start ()
    {
        timer = 0;
	}
	
	// Update is called once per frame
	void Update ()
    {
        //Destroys itself after timer
        timer += Time.deltaTime;
        if (timer > timerMax) Destroy(gameObject);
    }
}
