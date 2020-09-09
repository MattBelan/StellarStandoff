using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerStation : MonoBehaviour {

    /// <summary>
    /// Whether the station is alive
    /// </summary>
    public bool IsAlive { get { return Health > 0; } }

    /// <summary>
    /// Whether the station has chosen their action or not
    /// </summary>
    public bool ActionChosen { get; set; }

    /// <summary>
    /// What station is being targeted
    /// </summary>
    public PlayerStation Target { get; set; }

    /// <summary>
    /// What action the station prepared
    /// </summary>
    public string Action { get; set; }

    /// <summary>
    /// Coordinates of station (local to asteroid)
    /// </summary>
    public Vector3 Position { get; set; }

    /// <summary>
    /// Normal vector pointing from center of asteroid to station
    /// </summary>
    public Vector3 Direction { get; set; }

    /// <summary>
    /// Player Number
    /// </summary>
    public int PlayerNumber { get; set; }

    /// <summary>
    /// Color to tint this station and it's related assets
    /// </summary>
    public Color Hue { get; set; }

    /// <summary>
    /// True if shield is active
    /// </summary>
    public bool ShieldOn { get; set; }

    public bool DrillOn { get; set; }

    public bool DishOn { get; set; }

    public GameObject missile;

    public SpriteRenderer shieldRender;
    public SpriteRenderer mainRender;

    public GameObject label;
    public float labelOffset;
	public string labelStr;
    public ParticleSystem debris;
    public Transform dishPosition;

    //Private fields
    public short Health { get; set; }
    public short Resources { get; set; }

    //Force fields
    public float shieldTimer; //Controls exact opacity during fade
    private float shieldTime; //Max amount of second to fade in/out
    public bool shieldVisible; // True if shield is at max (permitted) opacity
    private float maxOpacity; //most opaque (on 0-1 scale) that shield can be

	// Called before start
	void Awake () {
        Health = 3;
        Resources = 1;
        ActionChosen = false;
        Action = "";
        Target = null;
	}

    //Called on startup
    void Start()
    {
        //GameManager.Instance.players.Add(this);
        transform.SetParent(GameManager.Instance.cam.world.transform);
        transform.position = Position;
        GetComponentInChildren<SpriteRenderer>().color = Hue;
        shieldVisible = false;
        shieldTimer = 0;
        shieldTime = 1.5f;
        maxOpacity = 0.75f;
		labelStr = "Player " + PlayerNumber;
		label.GetComponent<TextMeshPro>().text = labelStr;
        label.GetComponent<TextMeshPro>().outlineColor = Hue;
    }

    // Update is called once per frame
    void Update ()
    {
        //Runs shield fade-in/fade-out
        if (ShieldOn && !shieldVisible)
        {
            ActivateShield();
        }
        else if (!ShieldOn && shieldVisible)
        {
            DeactivateShield();
        }

        label.transform.position = transform.position + new Vector3(0, labelOffset, 0);
        label.transform.rotation = Quaternion.identity;
    }

    /// <summary>
    /// Logic for the player choosing their action
    /// </summary>
    public void ChooseAction()
    {
        if (!ActionChosen)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)&&Resources>0)
            {
                Action = "Shoot";
                ActionChosen = true;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2)&&Resources>0)
            {
                Action = "Reflect";
                ActionChosen = true;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Action = "Shield";
                ActionChosen = true;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Action = "Load";
                ActionChosen = true;
            }
        }
    }

    /// <summary>
    /// Logic for the player choosing their target
    /// </summary>
    public void ChooseTarget()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Target = GameManager.Instance.players[0];
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Target = GameManager.Instance.players[1];
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (GameManager.Instance.players[2] != null)
            {
                Target = GameManager.Instance.players[2];
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (GameManager.Instance.players[3] != null)
            {
                Target = GameManager.Instance.players[3];
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            if (GameManager.Instance.players[4] != null)
            {
                Target = GameManager.Instance.players[4];
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            if (GameManager.Instance.players[5] != null)
            {
                Target = GameManager.Instance.players[5];
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            if (GameManager.Instance.players[6] != null)
            {
                Target = GameManager.Instance.players[6];
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            if (GameManager.Instance.players[7] != null)
            {
                Target = GameManager.Instance.players[7];
            }
        }
    }

    /// <summary>
    /// Logic for the player performing their action
    /// </summary>
    public void PerformAction()
    {
        Direction = (new Vector3(transform.position.x, transform.position.y, 0) - Vector3.zero).normalized;

        switch (Action)
        {
            case "Shoot":
                if(Target == null)  //target shouldn't be null if shooting
                {
                    break;
                }
                //GameObject newMissile = Instantiate(missile, transform);
                MissileData newMissileData = new MissileData(this, Target);
                GameManager.Instance.missiles.Add(newMissileData);
                GameManager.Instance.numMissiles++;
				AudioManager.Instance.PlayOneShot("missile_Launch", 1);
                //newMissile.GetComponent<Missile>().Launch(this, Target);
                //newMissile.GetComponent<Missile>().origin = this;
                //newMissile.GetComponent<Missile>().destination = Target;

                Resources -= 1;
                break;
            case "Reflect":
                Resources -= 1;
                TurnOnDish();
                break;
            case "Shield":
                ShieldOn = true;
				AudioManager.Instance.FadeInLayer("forcefield_Idle", 0.7f, shieldTime);
				break;

            case "Load":
                Resources++;
                DrillOn = true;
                debris.Play();
				AudioManager.Instance.FadeInLayer("drill", 0.7f, 1f);
				break;
        }
    }

    /// <summary>
    /// Finds the target of attack, accounting for target reflections
    /// Preventing Reflection loops
    /// </summary>
    /// <returns> Whether a target was found before missle dies off </returns>
    public bool FindTarget()
    {
        int reflections = 0;
        while(Target.Action == "Reflect" && reflections < 8)
        {
            reflections++;
            PlayerStation newTarget = Target.Target;
            Target = newTarget;
        }

        if (reflections < 8)
            return true;
        else
            Debug.Log("Missle ran out of fuel!");
            return false;
    }

    /// <summary>
    /// When the player is hit by missle, take damage
    /// </summary>
    public void TakeDamage()
    {
        Health--;
    }

    //Fades shield in to partial opacity
    private void ActivateShield()
    {
        shieldTimer += Time.deltaTime;
        float increment = shieldTimer / shieldTime;
        increment *= maxOpacity;
        shieldRender.color = new Color(Hue.r, Hue.g, Hue.b, increment);
        if (shieldTimer >= shieldTime)
        {
            shieldTimer = 0;
            shieldVisible = true;
        }

	}

	public void TurnOffShield()
	{
		ShieldOn = false;
		AudioManager.Instance.FadeOutLayer("forcefield_Idle", 0, shieldTime, true);
	}

    public void TurnOffDrill()
    {
        DrillOn = false;
        debris.Stop();
    }

    public void TurnOnDish()
    {
        dishPosition.localPosition = Vector3.zero;
        DishOn = true;
    }

    public void TurnOffDish()
    {
        dishPosition.localPosition = new Vector3(0, -1, 0);
        DishOn = false;
    }

    //Fades shield out until full transparency
    private void DeactivateShield()
    {
        shieldTimer += Time.deltaTime;
        float increment = shieldTimer / shieldTime;
        increment = maxOpacity - (increment * maxOpacity);
        shieldRender.color = new Color(Hue.r, Hue.g, Hue.b, increment);
        if (shieldTimer >= shieldTime)
        {
            shieldTimer = 0;
            shieldVisible = false;
		}
    }

	/// <summary>
	/// Set the label text for the station
	/// </summary>
	/// <param name="text">The string to set it to</param>
	public void SetLabel(string text = null)
	{
		if(text == null)
		{
			label.GetComponent<TextMeshPro>().text = labelStr;
		}
		else
		{
			label.GetComponent<TextMeshPro>().text = text;
		}
	}

    public void DebugInfo()
    {
        Debug.Log("Resource count: " + Resources);
        Debug.Log("Health: " + Health);
    }

    private void OnMouseDown()
    {
        GameManager.Instance.CheckForStationClicked(this);
    }
}
