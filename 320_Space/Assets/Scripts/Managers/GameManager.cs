using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState { Begin, Tutorial, Paused, Playing, End }
enum RoundState { Begin, TurnOrder, PlayChoices, End }
enum TurnState { Begin, Choosing, End }

public class GameManager : Singleton<GameManager> {

    /// <summary>
    /// The current GameState
    /// </summary>
    public GameState State { get; set; }

    //menu stuff
    public Canvas mainMenu;
    public Canvas playerMenu;
    public Canvas enemyMenu;
    public Canvas turnCanvas;
    public Canvas animationCanvas;
    public Canvas tutorialCanvas;
    public Canvas resourceCanvas;
    public Canvas endCanvas;
    public Text playerTotal;
    public Text currentPlayer;
    public Text winningPlayer;
    private Color activeColor;

    //Camera
    public CameraScript cam;

    public GameObject stationPre;
    public GameObject resourcePre;
    public GameObject healthPre;
    public List<PlayerStation> players { get; set; }
    public List<MissileData> missiles { get; set; }
    public List<Image> healthList;
    public List<Image> resourceList;
    public List<Image> tutorialList;
    public int numMissiles;
    //public int numShields

    public float radius;

    //Private Fields
    RoundState roundState;
    short currPlayer;
    bool readyToPlay;
    public int numPlayers;
    bool missilesLaunched;
    public GameObject missilePrefab;
    bool zoomed;
    int camBase;
    //Color baseColor;
    bool playerReady;
    bool actionsReady;
    int tutCounter;
    bool tutCompleted;

    float resultsTimer;
    float maxResultsTime;

    float postResultsTimer;
    float maxPostResultsTime;

    //For generating "unique" stations
    public Color[] baseColors;
    public List<Sprite> baseSprites;

    // Called before start
    void Awake() {
        State = GameState.Begin;
        roundState = RoundState.Begin;
        players = new List<PlayerStation>();
        missiles = new List<MissileData>();
        currPlayer = 0;
        readyToPlay = false;
        numPlayers = 2;
        mainMenu.enabled = true;
        playerMenu.enabled = false;
        enemyMenu.enabled = false;
        missilesLaunched = false;
        playerTotal.enabled = true;
        zoomed = false;
        camBase = 0;
        //baseColor = stationPre.GetComponentInChildren<SpriteRenderer>().color;
        playerReady = false;
        actionsReady = false;
        animationCanvas.enabled = false;
        turnCanvas.enabled = false;
        tutCounter = 0;
        tutCompleted = false;
        tutorialCanvas.enabled = false;
        resourceCanvas.enabled = false;
        endCanvas.enabled = false;
        resultsTimer = 0;
        maxResultsTime = 3.0f;
        postResultsTimer = 0.0f;
        maxPostResultsTime = 3.0f;
    }

	//Initialize values here
	private void Start()
	{
		//Play all audio
		AudioManager.Instance.PlayLayer("m_tutorial_Layer");
		AudioManager.Instance.SetLayerVolume("m_tutorial_Layer", 0);
		AudioManager.Instance.FadeLayer("m_menu_Layer", 0.5f, 5f);
	}

	// Update is called once per frame
	void Update() {
        switch (State)
        {
            //Game is starting
            case GameState.Begin:
                //set up for number of players
                playerTotal.text = numPlayers.ToString();
                endCanvas.enabled = false;
                mainMenu.enabled = true;
                if (readyToPlay)
                {

                    float xPos;
                    float yPos;
                    float angle = 90;
                    float interval = 360 / numPlayers;
                    Vector3 position;
                    Vector3 direction;

                    for (int i = 0; i < numPlayers; i++)
                    {
                        //Instantiate station prefab
                        GameObject newPlayer = Instantiate(stationPre);

                        //Calculate direction and position of station
                        xPos = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
                        yPos = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
                        position = new Vector3(xPos, yPos, 0);
                        direction = (new Vector3(position.x, position.y, 0) - Vector3.zero).normalized;
                        angle -= interval;

                        //Assign direction and position to new prefab
                        newPlayer.GetComponent<PlayerStation>().Direction = direction;
                        newPlayer.GetComponent<PlayerStation>().Position = position;

                        //Orient the stations
                        if (direction == new Vector3(0, -1, 0))
                            newPlayer.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
                        else
                            newPlayer.transform.up = direction;

                        //Set station color, number, and sprite
                        newPlayer.GetComponent<PlayerStation>().Hue = baseColors[i];
                        newPlayer.GetComponent<PlayerStation>().PlayerNumber = i + 1;
                        newPlayer.GetComponent<PlayerStation>().mainRender.sprite = GetBaseSprite();

                        //Add prefab's PlayerStation component to list
                        players.Add(newPlayer.GetComponent<PlayerStation>());
                    }
                    mainMenu.enabled = false;
                    tutorialCanvas.enabled = true;
                    State = GameState.Tutorial;
					AudioManager.Instance.FadeLayer("m_tutorial_Layer", 0.5f, 0.5f);
					cam.UseManagerStart();
                }
                break;

            //Tutorial in progress
            case GameState.Tutorial:

                if (tutCompleted)
                {
                    turnCanvas.enabled = true;
                    State = GameState.Playing;
					AudioManager.Instance.FadeLayer("m_tutorial_Layer", 0f, 1f);
					AudioManager.Instance.FadeLayer("m_menu_Layer", 0f, 1f);
					AudioManager.Instance.FadeLayer("m_gameplay_bg_Layer", 0.5f, 1f);
					AudioManager.Instance.PlayLayer("m_gameplay_combat_Layer");
					AudioManager.Instance.SetLayerVolume("m_gameplay_combat_Layer", 0);
				}
				else
                {
                    for (int i = 0; i < tutorialList.Count; i++)
                    {
                        if (tutCounter == i)
                        {
                            tutorialList[i].enabled = true;
                        }
                        else
                        {
                            tutorialList[i].enabled = false;
                        }
                    }
                }

                break;
            //Game is paused
            case GameState.Paused:

                break;

            //Playing the game
            case GameState.Playing:
                switch (roundState)
                {
                    //Clear previous player actions
                    case RoundState.Begin:
                        currPlayer = 0;
                        missilesLaunched = false;
                        foreach (PlayerStation player in players)
                        {
                            player.ActionChosen = false;
                            player.Action = "";
                            player.Target = null;
                        }
						players[currPlayer].SetLabel("You!");
						roundState = RoundState.TurnOrder;
                        break;

                    //Players are taking their turns
                    case RoundState.TurnOrder:
                        //Should never be less than 2 players
                        if (players.Count < 2)
                        {
                            Debug.LogError("Should never be less than 2 players!");
                            return;
                        }

                        //End the turn order if we are past the max players
                        if (currPlayer == players.Count)
                        {
                            roundState = RoundState.PlayChoices;
							AudioManager.Instance.FadeLayer("m_gameplay_combat_Layer", 0.5f, 0.5f);
							currPlayer = 0;
                            return;
                        }

                        if (playerReady)
                        {
                            //displaying menu and resources
                            turnCanvas.enabled = false;
                            resourceCanvas.enabled = true;
                            if (zoomed)
                            {
                                if (camBase == currPlayer)
                                {
                                    playerMenu.enabled = true;
                                }
                                else
                                {
                                    enemyMenu.enabled = true;
                                }
                            }
                            else
                            {
                                playerMenu.enabled = false;
                                enemyMenu.enabled = false;
                            }

                            //setting home base color
                            for (int i = 0; i < players.Count; i++)
                            {
                                if (i == currPlayer)
                                {
                                    //players[i].GetComponentInChildren<SpriteRenderer>().color = new Color(0.0f, 1.0f, 1.0f);
                                }
                                else
                                {
                                    //players[i].GetComponentInChildren<SpriteRenderer>().color = baseColor;
                                }
                            }
                            
                            DisplayHealth(players[currPlayer]);
                            DisplayResources(players[currPlayer]);

                            CheckForStationTouch();

                            //Player chooses their action
                            if (players[currPlayer].IsAlive && !players[currPlayer].ActionChosen)
                            {
                                players[currPlayer].ChooseAction();
                            }
                            else if ((players[currPlayer].Action == "Shoot" || players[currPlayer].Action == "Reflect") && players[currPlayer].Target == null)
                            {
                                players[currPlayer].ChooseTarget();
                            }
                            else
                            {
                                cam.currState = CameraScript.CameraState.Start;
                                zoomed = false;
								players[currPlayer].SetLabel();
								currPlayer++;
								if(currPlayer < players.Count)
									players[currPlayer].SetLabel("You!");
								playerReady = false;
                            }
                        }
                        else
                        {
                            //Set UI color to current player color
                            activeColor = players[currPlayer].Hue;

                            //PAUL
                            //Set rotation to current player
                            cam.world.transform.rotation = Quaternion.AngleAxis(currPlayer * (360 / numPlayers), Vector3.forward);
                            cam.currRot = cam.world.transform.rotation;
                            cam.baseRot = cam.currRot;
                            cam.startRot = cam.currRot;

                            enemyMenu.enabled = false;
                            playerMenu.enabled = false;
                            resourceCanvas.enabled = false;
                            turnCanvas.enabled = true;
                            currentPlayer.text = "Player " + (currPlayer + 1);
                            currentPlayer.color = players[currPlayer].Hue;
                        }

                        break;

                    //Play all the choices out
                    case RoundState.PlayChoices:

                        resultsTimer += Time.deltaTime;

                        //PAUL
                        //Puts player one on top
                        cam.world.transform.rotation = Quaternion.identity;
                        //cam.currRot = cam.world.transform.rotation;
                        //cam.baseRot = cam.currRot;
                        //cam.startRot = cam.currRot;

                        playerMenu.enabled = false;
                        enemyMenu.enabled = false;
                        if (actionsReady)
                        {
                            animationCanvas.enabled = false;
                            if (!cam.moving)
                            {
                                if (!missilesLaunched)
                                {
                                    foreach (PlayerStation player in players)
                                    {
                                        player.PerformAction();
                                    }
                                    missilesLaunched = true;
                                }

                                foreach (MissileData missile in missiles)
                                {
                                    if (!missile.InFlight)
                                    {
                                        GameObject m = (GameObject)Instantiate(missilePrefab, cam.world.transform);
                                        m.GetComponent<Missile>().Launch(missile.Origin, missile.Destination);
                                        missile.InFlight = true;
                                    }
                                }

                                if (resultsTimer >= maxResultsTime && missilesLaunched && GameManager.Instance.numMissiles <= 0)
                                {
                                    resultsTimer = 0.0f;
                                    GameManager.Instance.missiles.Clear();
                                    actionsReady = false;
                                    roundState = RoundState.End;
                                }
                            }
                        }
                        else
                        {
                            resourceCanvas.enabled = false;
                            animationCanvas.enabled = true;
                        }
                        break;

                    //End the round
                    case RoundState.End:

                        postResultsTimer += Time.deltaTime;

                        int alivePlayers = 0;
                        foreach (PlayerStation player in players)
                        {
                            if (player.IsAlive)
                            {
                                alivePlayers++;
                            }

                            if(player.ShieldOn)
                            {
								player.TurnOffShield();
                            }

                            if (player.DrillOn)
                            {
                                player.TurnOffDrill();
                            }

                            if (player.DishOn)
                            {
                                player.TurnOffDish();
                            }
                        }

                        if (alivePlayers <= 1)
                        {
                            postResultsTimer = 0.0f;
                            State = GameState.End;
                            break;
                        }
						AudioManager.Instance.FadeOutLayer("drill", 0, 1f, true);
						AudioManager.Instance.FadeOutLayer("m_gameplay_combat_Layer", 0f, 0.5f);
						roundState = RoundState.Begin;
                        break;
                }
                break;

            //Ending the game
            case GameState.End:
                endCanvas.enabled = true;

                int playerWon = -1;

                for (int i = 0; i < players.Count; i++)
                {
                    if (players[i].IsAlive)
                    {
                        playerWon = i + 1;
                    }
                }

                if(playerWon >= 0)
                {
                    winningPlayer.text = "Player " + playerWon + " Wins";
                }
                else
                {
                    winningPlayer.text = "All Stations Destroyed";
                }
                break;
        }
    }

    public Sprite GetBaseSprite()
    {
        int rnd = Random.Range(0, baseSprites.Count);
        Sprite baseSprite = baseSprites[rnd];
        baseSprites.Remove(baseSprite);
        return baseSprite;
    }

    /// <summary>
    /// Adding players from menu
    /// </summary>
    public void AddPlayer()
    {
        if (numPlayers < 8)
        {
            numPlayers++;
        }
    }

    /// <summary>
    /// Removing player from menu
    /// </summary>
    public void RemovePlayer()
    {
        if (numPlayers > 2)
        {
            numPlayers--;
        }
    }

    /// <summary>
    /// Hitting the play button on main menu
    /// </summary>
    public void ToggleReady()
    {
        readyToPlay = !readyToPlay;
    }

    /// <summary>
    /// Checking for touch input on stations
    /// </summary>
    void CheckForStationTouch()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint((Input.GetTouch(0).position)), Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject.tag == "Station")
            {

                //Change position of label
                //if (hit.collider.gameObject.GetComponent<PlayerStation>().labelOffset == 1)
                //    hit.collider.gameObject.GetComponent<PlayerStation>().labelOffset = 2.0f;
                //else
                //    hit.collider.gameObject.GetComponent<PlayerStation>().labelOffset = 1;

                for (int i = 0; i < players.Count; i++)
                {
                    if (hit.collider.gameObject == players[i])
                    {
                        cam.SetNextPlayer(i);

                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Checking if station was clicked
    /// </summary>
    public void CheckForStationClicked(PlayerStation station)
    {
        if (!cam.moving && playerReady)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] == station)
                {
                    if (cam.currState == CameraScript.CameraState.Start)
                    {
                        cam.SetNextPlayer(i);
                        cam.currState = CameraScript.CameraState.Base;
                        players[i].GetComponent<PlayerStation>().labelOffset = 2;
                        zoomed = true;
                        camBase = i;
                    }
                    else
                    {
						ZoomOut();
                    }
                    return;
                }
            }
        }
    }

	/// <summary>
	/// Set the proper vars to zoom the camera out
	/// </summary>
	public void ZoomOut()
	{
		cam.currState = CameraScript.CameraState.Start;
		zoomed = false;
        foreach(PlayerStation p in players)
        {
            p.GetComponent<PlayerStation>().labelOffset = 1;
        }
    }

    /// <summary>
    /// Shoot for menu functionality
    /// </summary>
    public void Shoot()
    {
        if (players[currPlayer].Resources > 0)
        {
            players[currPlayer].ActionChosen = true;
            players[currPlayer].Action = "Shoot";
            players[currPlayer].Target = players[camBase];
        }
    }

    /// <summary>
    /// Reflect for menu functionality
    /// </summary>
    public void Reflect()
    {
        if (players[currPlayer].Resources > 0)
        {
            players[currPlayer].ActionChosen = true;
            players[currPlayer].Action = "Reflect";
            players[currPlayer].Target = players[camBase];
        }
    }

    /// <summary>
    /// Load for menu functionality
    /// </summary>
    public void Load()
    {
        players[currPlayer].ActionChosen = true;
        players[currPlayer].Action = "Load";
    }

    /// <summary>
    /// Shield for menu functionality
    /// </summary>
    public void Shield()
    {
        players[currPlayer].ActionChosen = true;
        players[currPlayer].Action = "Shield";
    }


    void DisplayHealth(PlayerStation player)
    {
        for (int i = 0; i < healthList.Count; i++)
        {
            if (i < player.Health)
            {
                healthList[i].enabled = true;
            }
            else
            {
                healthList[i].enabled = false;
            }
        }
    }


    void DisplayResources(PlayerStation player)
    {
        for (int i = 0; i < resourceList.Count; i++)
        {
            if (i < player.Resources)
            {
                resourceList[i].enabled = true;
            }
            else
            {
                resourceList[i].enabled = false;
            }
        }
    }


    public void TogglePlayerReady()
    {
        playerReady = true;
    }

    public void ToggleAnimationReady()
    {
        actionsReady = true;
    }

    public void NextTutorial()
    {
        tutCounter++;
        if (tutCounter >= 5)
        {
            tutCounter = 0;
            tutorialCanvas.enabled = false;
            tutCompleted = true;
        }
    }

    public void StartTutorial()
    {
        tutCompleted = false;
        tutorialCanvas.enabled = true;
        tutCounter = 0;
        State = GameState.Tutorial;
    }

    public void SkipTutorial()
    {
        tutCompleted = true;
        tutorialCanvas.enabled = false;
        tutCounter = 0;
    }


    public void ResetGame()
    {
        SceneManager.LoadScene("Playtest_2");
        //Application.LoadLevel(Application.loadedLevel);
    }
}
