using UnityEngine;
using System.Collections;
using System.Net;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using HTTP;

public class ReadyMatchUnityWeb : MonoBehaviour
{

	//a custom gui skin to use with your game
	public GUISkin customGuiSkin;


	//turn the GUI component off completely.
	public bool useGUI = true;

	//service ID used by matchmaking
	private String serviceID = "498c7b0a2ce84728f850462b32003987";

	//token returned by authorization.
	private String authToken = "";

	//minimum number of players
	public int minPlayers = 2;
	//maximum number of players
	public int maxPlayers = 2;

	//time to wait as the server before trying to join a game.
	public float timeToWaitAsServer = 15;

	//the port the game should play on
	public int port = 8080;

	//The authKey for this game 
	public String applicationID;
	//matchmaking server url
	public String masterServerURL = "https://readymatch.poweredbygamespy.com";
	//auth server url
	public String authServerURL = "https://fieldkit.poweredbygamespy.com";

	//number of players connected
	int players = 1;

	//a list of options the developer can set.
	public Hashtable playerOptions = new Hashtable ();

	//nat
	int nat;


	//whether the registration process has begun
	bool registrationStarted = false;

	//whether the join process has begun
	bool joinStarted = false;

	//when the player has clicked the button and is ready, but http request not sent
	bool registrationReady = false;

	//waiting for the network test to complete
	bool registrationWaiting = false;

	//update session on the next cycle.
	bool updateWaiting = false;

	//waiting for the network test to complete
	bool joinWaiting = false;

	//a match has been found and connected to
	bool matchFound = false;

	//the match has started
	bool matchStarted = false;

	//the network is not connected
	bool networkDisconnected = false;

	//the game is authorized with the service
	bool authorized = false;

	//the player has cancelled the search.
	bool searchCancelled = false;

	//if the matchmaking service is unavailable
	bool serviceUnavailable = false;

	//the current attempt in the joinList
	int joinAttempt = 0;
	//list of potential servers to join
	ArrayList joinList = new ArrayList ();


    //which request is currently running
    string requestFunction = "";
    
    //The current request
    Request request;

    //The current delete request. Separate, because we do not wait on the results.
    Request deleteRequest;
    
    //lets update know that the http request is running
    bool requestRunning = false;

	GameObject messenger = null;

	//whether this person is a server
	bool timerEnabled = false;

	//the time the timer started
	float timerStart = 0;
	//the current time
	float currentTime = 0;

	//the url of the session on the service.
	String sessionID = null;

	//passcode for this string
	String passcode = "passcode";


	//my IP.
	String myIP;

	//whether the Start button in the GUI has been clicked
	private bool mmstarted = false;

	/// <summary>
	/// This private class allows the program to accept certificates from https sites. 
	/// </summary>
	private class TrustAllCertificatePolicy : System.Net.ICertificatePolicy
	{
		public TrustAllCertificatePolicy ()
		{
		}
		public bool CheckValidationResult (ServicePoint sp, X509Certificate cert, WebRequest req, int problem)
		{
			return true;
		}
	}


	/// <summary>
	/// Wrapper for Network.TestConnection() 
	/// </summary>
	/// <returns>
	/// A <see cref="ConnectionTesterStatus"/>
	/// </returns>
	private ConnectionTesterStatus TestConnection ()
	{
		
		return Network.TestConnection ();
		
	}

	/// <summary>
	/// Wrapper for Network.InitializeServer 
	/// </summary>
	/// <param name="maxConnections">
	/// A <see cref="System.Int32"/>
	/// </param>
	/// <param name="port">
	/// A <see cref="System.Int32"/>
	/// </param>
	/// <param name="useNat">
	/// A <see cref="System.Boolean"/>
	/// </param>
	/// <returns>
	/// A <see cref="NetworkConnectionError"/>
	/// </returns>
	private NetworkConnectionError InitializeServer (int maxConnections, int port, bool useNat)
	{
		
		return Network.InitializeServer (maxConnections, port, useNat);
		
	}

	/// <summary>
	/// Logic to disconnect from a server, or to stop a server from running.
	/// Also sends a delete request to matchmaking
	/// </summary>
	private void Disconnect ()
	{
		players = 1;
		
		if (messenger != null) {
			Destroy (messenger);
			messenger = null;
		}
		
		Network.Disconnect ();
		
		if (sessionID != null) {
			Destroy (messenger);
			messenger = null;
			SendUnregisterWebRequest ();
			sessionID = null;
		}
	}

	/// <summary>
	/// Wrapper for Network.Connect. 
	/// </summary>
	/// <param name="ip">
	/// A <see cref="String"/>
	/// </param>
	/// <param name="port">
	/// A <see cref="System.Int32"/>
	/// </param>
	/// <param name="pass">
	/// A <see cref="System.String"/>
	/// </param>
	/// <returns>
	/// A <see cref="NetworkConnectionError"/>
	/// </returns>
	NetworkConnectionError Connect (String ip, int port, string pass)
	{
		return Network.Connect (ip, port, pass);
	}


	/// <summary>
	///Function called before the first Update. Sets our certificate policy to accept all certificates.
	/// </summary>
	public void Awake ()
	{
		
		System.Net.ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy ();
		
		currentTime = Time.realtimeSinceStartup;
		//Time.timeScale = 0;
		//Begins testing the connection		
		TestConnection ();
		if (authToken == "")
			AuthorizeStart ();
	}


	/// <summary>
	/// temporary menu that has a start button 
	/// </summary>
	void OnGUI ()
	{
		if (useGUI) {
			if (customGuiSkin != null)
				GUI.skin = customGuiSkin;
			
			if (!matchStarted && !searchCancelled) {
				//layout start
				
				GUI.BeginGroup (new Rect (Screen.width / 2 - 150, Screen.height / 2 - 150, 300, 300));
				
				//the menu background box
				GUI.Box (new Rect (0, 100, 300, 100), "");
				
				if (!mmstarted) {
					if (GUI.Button (new Rect (20, 135, 260, 30), "Start")) {
						
						mmstarted = true;
						
						
						Join ();
					}
				} else {
					GUI.Label (new Rect (20, 110, 260, 30), GetMatchStatus ());
					
					if (GUI.Button (new Rect (20, 160, 260, 30), "Play Offline")) {
						//Time.timeScale = 1.0f;
						searchCancelled = true;
					}
					
				}
				
				GUI.EndGroup ();
				
			}
		}
	}


	/// <summary>
	///Function that returns a string about the current state in the match making progress. 
	/// </summary>
	/// <returns>
	/// A <see cref="String"/>
	/// </returns>
	public String GetMatchStatus ()
	{
		if (matchStarted)
			return "Match Started!";
		if (matchFound)
			return "Match Found, waiting for " + (minPlayers - players) + " more player" + ((minPlayers - players) > 1 ? "s." : ".");
		if (serviceUnavailable)
			return "ReadyMatch service unavailable";
		if (registrationStarted || joinStarted)
			return "Looking for Match";
		if (registrationReady)
			return "Connecting to ReadyMatch";
		if (!authorized)
			return "Waiting for authorization";
		if (registrationWaiting)
			return "Checking NAT and IP";
		if (networkDisconnected)
			return "Error with Network Connection.";
		
		return "ReadyMatch not started";
	}

	/// <summary>
	///Function that returns an int that represents the current state in the match making progress. 
	/// </summary>
	/// <returns>
	/// A <see cref="System.Int32"/>
	/// </returns>
	public int GetMatchStatusCode ()
	{
		if (matchStarted)
			return 6;
		if (matchFound)
			return 5;
		if (serviceUnavailable)
			return -2;
		if (registrationStarted || joinStarted)
			return 4;
		if (registrationReady)
			return 3;
		if (!authorized)
			return 1;
		if (registrationWaiting)
			return 2;
		if (networkDisconnected)
			return -1;
		
		return 0;
	}

	/// <summary>
	/// Public function to check the number of players in the match already. 
	/// </summary>
	/// <returns>
	/// A <see cref="System.Int32"/>
	/// </returns>
	public int GetPlayerCount ()
	{
		return players;
	}

	/// <summary>
	///Update function called every frame. Used to check if the registration is ready yet. 
	/// </summary>
	public void Update ()
	{
		if (Network.peerType == NetworkPeerType.Client && messenger == null) {
			messenger = GameObject.Find ("ReadyMatchMessenger(Clone)");
		}
		
		if (messenger != null) {
			MessengerBehaviour mb = messenger.GetComponent<MessengerBehaviour> ();
			if (mb.updateAvailable) {
				players = mb.GetPlayers ();
				matchStarted = mb.IsMatchStarted ();
				mb.updateAvailable = false;
			}
		}
		
		currentTime = Time.realtimeSinceStartup;
		ReadyReadyMatch ();
		WaitOnTimer ();
		
		if (updateWaiting) {
			updateWaiting = false;
			UpdateStart ();
		}
		


        
        if (requestRunning)
        {
            if (request.isDone)
            {
                requestRunning = false;

				//Debug.Log(requestFunction);
				//Debug.Log(request.response.status);
				//Debug.Log(request.response.Text);
				
                if (request.response.status != 200)
                {
                    serviceUnavailable = true;
                    return;
                }


                string finishData = request.response.Text;

                switch(requestFunction){
                case "register":
                    FinishRegister(finishData);
                    break;
                case "join":
                    FinishJoin(finishData);
                    break;
                case "update":
                    FinishUpdate(finishData);
                    break;
                case "authorize":
                    FinishAuthorize(finishData);
                    break;
                default:
                    request = null;
                    break;
                }
            }
        }
	}

	/// <summary>
	/// waits until the timer expires and then either searches for a match, or updates the session
	/// depending on whether players have joined you or not.
	/// </summary>
	private void WaitOnTimer ()
	{
		if (timerEnabled) {
			//if the search has been cancelled before the result returns, sets the process back to the beginning.
			if (searchCancelled) {
				
				
				timerEnabled = false;
				searchCancelled = false;
				registrationReady = false;
				registrationStarted = false;
				Disconnect ();
				return;
			}
			
			if (currentTime > timerStart + timeToWaitAsServer) {
				timerEnabled = false;
				if (!matchFound)
					JoinStart ();
				else
					UpdateStart ();
			}
		}
	}



	/// <summary>
	/// Checks if registration is ready. If it is, starts a server. 
	/// </summary>
	private void ReadyReadyMatch ()
	{
		//if registration has not not started and the network is connected
		if (!registrationReady && !networkDisconnected) {
			
			
			
			//if there was an error with network testing the network is set to disconnected.
			if (TestConnection () == ConnectionTesterStatus.Error) {
				networkDisconnected = true;
				registrationWaiting = false;
				return;
			} else {
				if (authorized) {
					myIP = Network.player.ipAddress;
					registrationReady = true;
				}
			}
			
			if (joinWaiting && registrationReady) {
				joinWaiting = false;
				JoinStart ();
				return;
			}
			
			//if the person is waiting to register still and the network test has completed
			if (registrationWaiting && registrationReady) {
				
				registrationWaiting = false;
				RegistrationStart ();
				
			}
		}
	}

	/// <summary>
	/// Starts up a server, returns true if successful, false if not. 
	/// </summary>
	/// <returns>
	/// A <see cref="System.Boolean"/>
	/// </returns>
	private bool StartServer ()
	{
		//builds a password of random numbers and letters.
		StringBuilder sb = new StringBuilder ();
		System.Random r = new System.Random ();
		for (int i = 0; i < 8; i++) {
			int num = (int)(r.Next () % 62);
			sb.Append ((char)(num + 48 + ((num >= 10) ? 7 : 0) + ((num >= 36) ? 8 : 0)));
		}
		
		passcode = sb.ToString ();
		
		
		
		//sets the incoming password on the server.
		try {
			
			Network.incomingPassword = passcode.ToString ();
			
		} catch (Exception e) {
			
		}
		
		
		bool useNat = !Network.HavePublicAddress ();
		
		//Starts a server
		if (InitializeServer (maxPlayers - 1, port, useNat) == NetworkConnectionError.NoError) {
			//sets my ip
			
			myIP = Network.player.ipAddress;
			
			UnityEngine.Object rmm = Resources.Load ("ReadyMatchMessenger");
				
			messenger = (GameObject)Network.Instantiate (rmm, Vector3.zero, Quaternion.identity, 0);
			
			return true;
		} else {
			//failure to start server. sets my ip to empty string. I will be client.
			myIP = "";
			
			Disconnect ();
			return false;
		}
	}


	/// <summary>
	///Start the whole matchmaking process. 
	/// </summary>
	private void Register ()
	{
		
		//prepares to register
		if (!registrationReady) {
			registrationWaiting = true;
			ReadyReadyMatch ();
		} else
			RegistrationStart ();
	}

	/// <summary>
	///Begins the registration http request 
	/// </summary>
	private void RegistrationStart ()
	{
		if (registrationStarted)
			return;
		registrationStarted = true;
		
		
		if (!StartServer ()) {
			
			registrationStarted = false;
			timerEnabled = true;
			timerStart = currentTime;
			return;
		}
		
		
		//json encode the data.
		string requestData = JSON.Encode (BuildSession ());
		
		request = new Request("post",masterServerURL + "/session");
		
        request.SetHeader("Content-Type", "application/json");
        request.SetHeader("Authorization",authToken);
        request.Text = requestData;

        requestFunction = "register";
        requestRunning = true;
		try {

            request.Send();

		} catch (Exception e) {
            requestRunning = false;
            request = null;
			serviceUnavailable = true;
			return;
			
		}
	}

	/// <summary>
	/// handles the result of the request 
	/// </summary>
	/// <param name="res">
	/// A <see cref="System.String"/>
	/// </param>
	private void FinishRegister (string res)
	{
		
		Hashtable match = (Hashtable)JSON.Decode (res);
		//checks whether the match making service has specified this player as the server
		
		//the url to access the session in the mm service
		sessionID = (String)match["id"];
		
		
		
		
		//set the timer till the next join;
		timerEnabled = true;
		timerStart = currentTime;
		
		
	}


	/// <summary>
	/// Called by the player when they are ready for matchmaking to start 
	/// </summary>
	public void Join ()
	{
		if (!registrationReady)
			joinWaiting = true; else if (!registrationStarted)
			JoinStart ();
	}

	/// <summary>
	/// Called when the player is ready to find a server. 
	/// </summary>
	private void JoinStart ()
	{
		if (joinStarted)
			return;
		
		joinStarted = true;
		if (networkDisconnected)
			return;
		
		//json encode the data.
		string requestData = JSON.Encode (BuildSession ());
		
        request = new Request("post", masterServerURL + "/match");

        request.SetHeader("Content-Type", "application/json");
        request.SetHeader("Authorization", authToken);
        request.Text = requestData;

        requestFunction = "join";
        requestRunning = true;
        try
        {

            request.Send();

        }
        catch (Exception e)
        {
            requestRunning = false;
            request = null;
            serviceUnavailable = true;
            return;

        }
		
		
		
	}



	/// <summary>
	/// Called when the asynchronous web request call finishes. Takes
	/// a list of servers and attempts to join them.
	/// </summary>
	/// <param name="res">
	/// A <see cref="String"/>
	/// </param>
	private void FinishJoin (String res)
	{
	
		joinAttempt = 0;
		
		//decodes the string as json.
		joinList = (ArrayList)JSON.Decode (res);
		
		//stops the running server if it is running.
		if (sessionID != null && joinList.Count > 0) {
			
			if (myIP.CompareTo (((Hashtable)joinList[0])["ip"]) != 0) {
				
				
			}
			
			if (joinList.Count != 1 || myIP.CompareTo (((Hashtable)joinList[0])["ip"]) != 0)
				Disconnect ();
		}
		if (joinList.Count == 0)
			Disconnect ();
		
		JoinServer ();
	}

	/// <summary>
	/// Tries to connect to a potential server 
	/// </summary>
	private void JoinServer ()
	{
		//if the search has been cancelled before the result returns, sets the process back to the beginning.
		if (searchCancelled) {
			
			
			joinStarted = false;
			searchCancelled = false;
			registrationReady = false;
			registrationStarted = false;
			Disconnect ();
			return;
		}
		
		if (joinAttempt >= joinList.Count) {
			if (!matchFound && sessionID == null) {
				registrationStarted = false;
				registrationReady = false;
				joinStarted = false;
				registrationWaiting = true;
			} else {
				joinStarted = false;
				timerStart = currentTime;
				timerEnabled = true;
				updateWaiting = true;
			}
			return;
		}
		
		
		//gets the match field of the object
		Hashtable match = (Hashtable)joinList[joinAttempt];
		
		if (myIP.CompareTo (match["ip"]) != 0) {
			//connects to the server the mm service has returned
			NetworkConnectionError n = Connect (match["ip"].ToString (), int.Parse (match["port"].ToString ()), match["passcode"].ToString ());
			
			
		} else {
			joinAttempt++;
			JoinServer ();
		}
	}

	/// <summary>
	/// when the game fails to join a server, tries the next one on the list. 
	/// </summary>
	/// <param name="n">
	/// A <see cref="NetworkConnectionError"/>
	/// </param>
	void OnFailedToConnect (NetworkConnectionError n)
	{
		
		joinAttempt++;
		JoinServer ();
	}

	/// <summary>
	/// when the game successfully connects to a server, sets matchFound to true! 
	/// </summary>
	void OnConnectedToServer ()
	{
		//Time.timeScale = 1.0f;
		matchFound = true;
		
		
	}


	/// <summary>
	/// if the player is a server, when a player joins locks the match 
	/// </summary>
	void OnPlayerConnected ()
	{
		players++;
		//Time.timeScale = 1.0f;
		matchFound = true;
		if (players >= minPlayers) {
			matchStarted = true;
			messenger.networkView.RPC ("OnMatchStarted", RPCMode.Others, players);
		} else
			messenger.networkView.RPC ("OnPlayerCountChange", RPCMode.Others, players);
	}

	/// <summary>
	/// if all players disconnect, unlock match if it hasn't started yet 
	/// </summary>
	void OnPlayerDisconnected ()
	{
		players--;
		messenger.networkView.RPC ("OnPlayerCountChange", RPCMode.Others, players);
		if (players == 1 && !matchStarted) {
			matchFound = false;
		}
	}

	/// <summary>
	/// destroys the messenger if you get disconnected. 
	/// </summary>
	void OnDisconnectedFromServer ()
	{
		Destroy (messenger);
		messenger = null;
		
		searchCancelled = false;
		registrationReady = false;
		registrationStarted = false;
		joinStarted = true;
		matchFound = false;
	}


	/// <summary>
	///function that is called before the player disconnects from a game 
	/// </summary>
	public void DisconnectFromServer ()
	{
		Disconnect ();
		searchCancelled = false;
		registrationReady = false;
		registrationStarted = false;
		joinStarted = true;
		matchFound = false;
	}

	/// <summary>
	/// async call to delete the existing registration from the server. 
	/// </summary>
	void SendUnregisterWebRequest ()
	{
		
		
        deleteRequest = new Request("delete",masterServerURL + "/session/" + sessionID);

        deleteRequest.SetHeader("Content-Type", "application/json");
        deleteRequest.SetHeader("Authorization", authToken);

        try
        {

            deleteRequest.Send();

        }
        catch (Exception e)
        {

        }
		
		
		
	}
		
		
	/// <summary>
	/// updates the session on the match making server 
	/// </summary>
	private void UpdateStart ()
	{
		
        //json encode the data.
        string requestData = JSON.Encode(BuildSession());

        request = new Request("put", masterServerURL + "/session/"+sessionID);
		
		
        request.SetHeader("Content-Type", "application/json");
        request.SetHeader("Authorization", authToken);
        request.Text =requestData;

        requestFunction = "update";
        requestRunning = true;
        try
        {
            
            request.Send();

        }
        catch (Exception e)
        {
            requestRunning = false;
            request = null;
            serviceUnavailable = true;
            return;

        }
		
	}


	/// <summary>
	/// handles the result of the request 
	/// </summary>
	/// <param name="res">
	/// A <see cref="System.String"/>
	/// </param>
	private void FinishUpdate (string res)
	{
		
		
		
		
		//set the timer till the next join;
		timerEnabled = true;
		timerStart = currentTime;
	}


	/// <summary>
	/// Gets the number result from the Nat test. 
	/// </summary>
	/// <returns>
	/// A <see cref="System.Int32"/>
	/// </returns>
	private int GetNat ()
	{
		int tnat;
		
		
		ConnectionTesterStatus conTestRes = TestConnection ();
		
		//defines the nat word based on the nat from the network test
		switch (conTestRes) {
		case ConnectionTesterStatus.LimitedNATPunchthroughPortRestricted:
			tnat = 2;
			//middle
			break;
		case ConnectionTesterStatus.LimitedNATPunchthroughSymmetric:
			tnat = 1;
			//worst
			break;
		case ConnectionTesterStatus.NATpunchthroughAddressRestrictedCone:
			tnat = 3;
			break;
		default:
			tnat = 4;
			//best
			break;
		}
		
		return tnat;
		
	}

	/// <summary>
	/// builds a session object based on the set variables.
	/// </summary>
	/// <returns>
	/// A <see cref="Hashtable"/>
	/// </returns>
	private Hashtable BuildSession ()
	{
		//creates the json object.
		Hashtable session = new Hashtable ();
		
		//Add ip, port and passcode to session
		session.Add ("ip", myIP);
		session.Add ("port", port);
		session.Add ("passcode", passcode);
		
		session.Add ("min_players", minPlayers);
		session.Add ("max_players", maxPlayers);
		session.Add ("current_players", players);
		
		nat = GetNat ();
		
		
		//add nat to session
		session.Add ("nat", nat);
		
		//add player to nat. This will be a json object that contains 
		//a list of parameters specified by the developer.
		session.Add ("game", playerOptions);
		return session;
	}


	/// <summary>
	/// gets the authorization token from the server.
	/// </summary>
	private void AuthorizeStart ()
	{
		
		Hashtable auth = new Hashtable ();
		
		
		auth.Add ("applicationID", applicationID);
		auth.Add ("serviceID", serviceID);
		
		
		//json encode the data.
		string requestData = JSON.Encode (auth);

        request = new Request("post", authServerURL + "/authorize");

        request.SetHeader("Content-Type", "application/json");
        request.Text =requestData;

        requestFunction = "authorize";
        requestRunning = true;
        try
        {
            
            request.Send();

        }
        catch (Exception e)
        {
            requestRunning = false;
            request = null;
            serviceUnavailable = true;
            
            return;

        }
		
		
	}

	/// <summary>
	/// handles the result of the request 
	/// </summary>
	/// <param name="res">
	/// A <see cref="System.String"/>
	/// </param>
	private void FinishAuthorize (string res)
	{
		
		Hashtable auth = (Hashtable)JSON.Decode (res);
		//checks whether the match making service has specified this player as the server
		
		authToken = auth["token"].ToString ();
		
		authorized = true;
		
	}
	
}
