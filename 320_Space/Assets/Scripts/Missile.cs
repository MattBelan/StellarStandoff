using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour {

    //Fields
    public bool inFlight; //Arc updates if this is true
    public PlayerStation origin; //Station at start of arc
    public PlayerStation destination; //Station at end of arc
    public float radius; //radius of asteroid

    public int bounces; //number of times missile has been reflected
    public int maxBounces; //number of players plus one
    public float maxSpeed; //number of seconds missile takes to go 180

    private float arc; //fraction of max height over surface; 0 to 1
    private Vector3 slerp; //normalized direction vector from center of asteroid to missile
    private float fracComplete; //fraction of the arc that has been completed; 0 to 1
    private float angle; //angle to rotate missile up vector from slerp

    public float speed; //seconds it takes to complete arc
    public float timer; //counts from zero to speed
    public float maxHeight; //distance away from surface at peak of arc

    public GameObject explosion;

    /// <summary>
    /// Sets up the missile arc and puts it into play
    /// </summary>
    /// <param name="origin">Station at beginning of arc</param>
    /// <param name="destination">Station at end of arc</param>
    public void Launch(PlayerStation origin, PlayerStation destination)
    {
        this.origin = origin;
        this.destination = destination;

        //Distance is set to the angle between origin/destination stations
        float distance = Vector3.SignedAngle(origin.Direction, destination.Direction, Vector3.forward);

        //Tells missile which direction is forward
        if (distance > 0) angle = 90;
            else angle = -90;

        //Sets speed based on distance between origin/destination
        if (maxSpeed <= 0)
        {
            maxSpeed = 3.5f;
        }

        speed = (Mathf.Abs(distance) / 180) * maxSpeed;

        //Changes the destination station's direction by half a degree to avoid 180 arc problem
        if (distance == 180) destination.Direction = Quaternion.AngleAxis(-0.5f, Vector3.forward) * destination.Direction;
        else if (distance == -180) destination.Direction = Quaternion.AngleAxis(0.5f, Vector3.forward) * destination.Direction;

        timer = 0;
        inFlight = true;
    }

    // Use this for initialization
    void Start ()
    {
        bounces = 0;
        maxBounces = GameManager.Instance.numPlayers + 1;
        //maxSpeed = 3.5f;
        //radius = 5.0f;
        //maxHeight = 1.0f;
	}
	
	// Update is called once per frame
	void Update ()
    {
		if(inFlight)
        {
            TravelArc();
        }

        if(fracComplete >= 1)
        {
            CheckStation(destination);
        }

        if (bounces > maxBounces)
        {
            //GameManager.Instance.missiles.Remove(this);
            GameManager.Instance.numMissiles--;
            Explode();
            origin.TakeDamage();
            Destroy(gameObject);
        }
	}

    /// <summary>
    /// Updates transform of missile over arc
    /// </summary>
    public void TravelArc()
    {
        //Increment arc
        timer += Time.deltaTime;
        fracComplete = timer / speed;

        //Calculate position
        slerp = Vector3.Slerp(origin.Direction, destination.Direction, fracComplete).normalized; //controls distance around asteroid
        arc = Mathf.Sin(fracComplete * Mathf.PI); //controls distance away from surface
        transform.position = (radius + (arc * maxHeight)) * slerp; //sets to transform

        //Calculate rotation
        transform.up = Quaternion.AngleAxis(angle, Vector3.forward) * slerp;
    }

    /// <summary>
    /// Checks state of station, changes missile and station variables accordingly, and relaunches missile if necessary
    /// </summary>
    /// <param name="target">Station to check for impact</param>
    public void CheckStation(PlayerStation target)
    {
        if (target.Action == "Reflect")
        {
            //missle boop
            bounces++;
            Launch(target, target.Target);
			AudioManager.Instance.PlayOneShot("radar_Beep", 1);
		}
		else if(target.Action == "Shield")
        {
            //missle explosion
            //GameManager.Instance.missiles.Remove(this);
            GameManager.Instance.numMissiles--;
			AudioManager.Instance.PlayOneShot("forcefield_Hit", 1);
            Destroy(gameObject);
		}
        else
        {
            //missle and base explosion
            target.TakeDamage();
            GameManager.Instance.numMissiles--;
			AudioManager.Instance.PlayOneShot("missile_Hit", 1);
			Explode();
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initiate visuals and audio before the object is destroyed
    /// Potentially make an area of effect?
    /// </summary>
    public void Explode()
    {
        GameObject e = (GameObject)Instantiate(explosion);
        e.transform.position = transform.position;
    }
}
