using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileData {

    //Properties
    public PlayerStation Origin { get; set; }
    public PlayerStation Destination { get; set; }
    public bool InFlight { get; set; }

    //Constructor
    public MissileData(PlayerStation origin, PlayerStation destination)
    {
        InFlight = false;
        this.Origin = origin;
        this.Destination = destination;
    }
}
