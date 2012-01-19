using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class NetworkController : MonoBehaviour {
	
	// If true, this instance of Unity acts as a server
	public bool isServer = true;
	
	// What network address should we use to connect to the server
	public string serverHost = "localhost";
	
	// What port is the server listening on
	public int serverPort = 8000 ;
	
	// Is the networking component connected to a server?
	public bool connected = false;
	
	public int maxConnections = 32;
	
	public List<GameObject> enemyList =new List<GameObject>(); 
	
	public Transform playerStartLocation ;
	
	public static int NETWORK_GROUP = 1;
	
	public GameObject Player;
	
	// Keep a list of players and some information about them
	public List<NPlayer> players = new List<NPlayer>();
	
	// Keep an ID that the server and client agree on for a client
	public static int clientID = -1;
	
	public bool mmstarted = false;
	
	void Start(){
		Time.timeScale = 0;
	}	
	
	void OnGUI(){
		if(!mmstarted){
		  //layout start
    GUI.BeginGroup(new Rect(50, 50, 300, 300));
    
    //the menu background box
    GUI.Box(new Rect(0, 0, 300, 300), "");

   
    isServer = GUI.Toggle(new Rect(20,20,100, 40),isServer, "IS SERVER");
		
	GUI.Label(new Rect(20,80,100, 40),"Server IP:");
			
	serverHost = GUI.TextField(new Rect(140,80,140, 40),serverHost);
			
	GUI.Label(new Rect(20,160,100, 40),"Server Port:");
			
	serverPort =  Convert.ToInt32(GUI.TextField(new Rect(140,160,140, 40),serverPort.ToString()));
			
			
    if(GUI.Button(new Rect(20, 240, 260, 40), "Start")) {
		Time.timeScale = 1.0f;		
		StartMatchmaking();
	}
		
		
    //layout end
    GUI.EndGroup(); 
		}
	}
	
	// Use this for initialization
	void StartMatchmaking() {
		mmstarted = true;
		//Initialize the server immediately if this instance is going to be
		//a server
		if(isServer)
		{
			Debug.Log(String.Format("Server is listening at {0}:{1}.",serverHost,serverPort));
			Network.InitializeServer(maxConnections,serverPort,false);
			
		}
		else
		{
			Connect();
		}
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	// Connect the client to the server
	public void Connect()
	{
		Debug.Log(String.Format("Connecting client to server at {0}:{1}",serverHost,serverPort));
		Network.Connect(serverHost, serverPort);
	}
	
	// Instantiate a player at the start location
	public GameObject InstantiatePlayer(GameObject Player)
	{
		Vector3 pos = Player.transform.position;
		Quaternion rot = Player.transform.rotation;
		
		var resource = Resources.Load("PlayerPrefab");
		
		if(!resource)
			Debug.Log("Player not found");
		var player = (GameObject)Network.Instantiate(resource,pos,rot,NETWORK_GROUP);
		
/*		SpawnAtCheckpoint sac1 = (SpawnAtCheckpoint)Player.GetComponent("SpawnAtCheckpoint");
		SpawnAtCheckpoint sac2 = (SpawnAtCheckpoint)player.GetComponent("SpawnAtCheckpoint");
		
		sac2.checkpoint = sac1.checkpoint;*/
		
		Destroy(Player);
		
		return player;
	}
	
	//Instantiate any game Object
	public void InstantiateGO(GameObject go){
		if(go){
		var resource = Resources.Load(go.name+"Prefab");
			if(resource){
				Network.Instantiate(resource,go.transform.position,go.transform.rotation,NETWORK_GROUP);
				/*
				GameObject aip = ((GameObject)(go.GetComponent("AIPatrol")));
				if(aip){
					PatrolMoveController pmc = (PatrolMoveController)aip.GetComponent("PatrolMoveController");
					if(pmc){
						GameObject aip2 = ((GameObject)(go.GetComponent("AIPatrol")));
						if(aip2){
							PatrolMoveController pmc2 = (PatrolMoveController)aip.GetComponent("PatrolMoveController");
							if(pmc2){
								pmc2.patrolRoute = pmc.patrolRoute;
							}
						}
					}
				}*/
			}
		}
	}
	
	// ############################################################
	// Register handlers for RakNet networking events
	// ############################################################
	public void OnConnectedToServer()
	{
		// Client has connected to the server. 
		Debug.Log(String.Format("Client has connected to server at {0}:{1}",serverHost,serverPort));
		connected = true;
		
		// Join the client to the server
		// TODO
		
		
		GameObject player = InstantiatePlayer(Player);
		
		foreach(var go in enemyList){
			if(go){
				Destroy(go);
			}
		}
		/*
		foreach(GameObject go in GameObject.FindGameObjectsWithTag("Player")){
			if(go){
				Debug.Log("Checking Players...");
				if( !((NetworkView)go.GetComponent("NetworkView")).isMine){
					Debug.Log("not mine!");
					MonoBehaviour s = (MonoBehaviour)go.GetComponent("PlayerMoveController");
					s.enabled = false;
				}	
				else{
					Debug.Log("mine!");	
				}
			}
		}*/
		NetworkView view = gameObject.GetComponent<NetworkView>();
		
		view.RPC("Join",RPCMode.Server);
		view.RPC("NewPlayerJoined",RPCMode.All,"Jeff");
	}
	
	public void OnFailedToConnect(NetworkConnectionError error)
	{
		Debug.Log(String.Format("Failed to connect to server at {0}:{1} because: {2}",serverHost,serverPort,error));
	}
	
	// ############################################################
	// RPC Methods
	// ############################################################
	
	/// <summary>
	/// Join the server with a particular player name. 
	/// </summary>
	/// <returns>
	/// A network view ID<see cref="System.Int32"/>
	/// </returns>
	[RPC]
	public void Join(NetworkMessageInfo info)
	{
		NetworkView view = gameObject.GetComponent<NetworkView>();
			
		if(Player){
			InstantiatePlayer(Player);
			
			foreach(var go in enemyList){
			if(go){
				InstantiateGO(go);
				Destroy(go);
			}
		}	
		}
		
	}
	
	[RPC]
	public void NewPlayerJoined(String name, NetworkMessageInfo info)
	{
/*		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		foreach(GameObject go in players){
			if(go){
				NetworkView nv = (NetworkView)go.GetComponent("NetworkView");
				Stats s = (Stats)go.GetComponent("Stats");
				if(nv && s && nv.owner == info.sender){
					s.name = name;
					break;
				}
			}
		}*/
		
	}
	
	// ############################################################
	// Static methods
	// ############################################################

	// Static method to instantiate a server making it possible to execute from a command line
	public static void StartServer()
	{
		Application.LoadLevel(0);
	}
	
	
	
}
