using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceStations : MonoBehaviour {

    //Fields
    public int numStations;
    public float radius;
    public float lastRadius;
    public List<StationData> stations;
    public float angle;
    private float interval;
    private float lastInterval;

    private float xPos;
    private float yPos;
    public GameObject station;
    public GameObject missile;

    // Use this for initialization
    void Start ()
    {
        stations = new List<StationData>();

        angle = 90; //sets first station position as top of asteroid

        if (numStations < 2) numStations = 2;

        interval = 360 / numStations;

        //Iterates over asteroid and sets coordinates for each station
		for(int i = 0; i < numStations; i++)
        {
            xPos = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            yPos = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;

            Vector2 position = new Vector2(xPos, yPos);

            Vector3 direction = (new Vector3(position.x, position.y, 0) - Vector3.zero).normalized;

            StationData s = new StationData(position, direction);

            stations.Add(s);

            angle -= interval;
        }
	}

    private void OnDrawGizmos()
    {
        //Draws asteroid
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(new Vector3(0, 0, 0), radius);

        //Draws stations
        for (int i = 0; i < numStations; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(stations[i].Position, radius: radius / 5);
        }
    }

    // Update is called once per frame
    void Update ()
    {
        //Updates layout if radius or number of stations is changed
        interval = 360 / numStations;

        if(interval != lastInterval || lastRadius != radius)
        {
            angle = 90;
            stations.Clear();

            for (int i = 0; i < numStations; i++)
            {
                xPos = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
                yPos = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;

                Vector2 position = new Vector2(xPos, yPos);

                Vector3 direction = (new Vector3(position.x, position.y, 0) - Vector3.zero).normalized;

                StationData s = new StationData(position, direction);

                stations.Add(s);

                angle -= interval;
            }
        }

        lastInterval = interval;
        lastRadius = radius;

        //Press Button to spawn missile from station 1
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject m = (GameObject)Instantiate(missile);
            m.GetComponent<TestMissile>().asteroid = this;
            m.transform.position = stations[0].Position;
            //Instantiate(missile, stations[0].Position, Quaternion.identity);
        }
    }
}
