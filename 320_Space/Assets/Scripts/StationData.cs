using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationData {

    //Properties
    public Vector2 Position { get; set; }
    public Vector3 Direction { get; set; }

    //Constructor
    public StationData(Vector2 position, Vector3 direction)
    {
        this.Position = position;
        this.Direction = direction;
    }
}
