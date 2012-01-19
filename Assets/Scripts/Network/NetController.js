
	public var controlNetwork:boolean = true;
	
	// If true, this instance of Unity acts as a server
	public var isServer:boolean = true;
	
	// What network address should we use to connect to the server
	public var serverHost :String= "localhost";
	
	// What port is the server listening on
	public var serverPort:int = 8000 ;
	
	// Is the networking component connected to a server?
	public var connected :boolean= false;
	
	public var maxConnections:int = 32;
	
	public var enemyList :GameObject[]; 
	
	public var cam : GameObject;
	
	public var playerStartLocation :Transform;
	
	public static var NETWORK_GROUP :int = 1;
	
	public var Player:GameObject;
			
	public var playerName = "Player";
	
	private var mmstarted :boolean= false;
	

	function Awake(){
		if(controlNetwork)
			Time.timeScale = 0;
	}	
	
	
	function OnGUI(){
		if(!mmstarted){
		  //layout start
		  	
		    GUI.BeginGroup(Rect(Screen.width/2-150, Screen.height/2-150, 300, 300));
		    
		    if(controlNetwork){
			    //the menu background box
			    GUI.Box(Rect(0, 0, 300, 300), "");
			
			   
			    isServer = GUI.Toggle(Rect(20,50,100, 30),isServer, "IS SERVER");
					
				GUI.Label(Rect(20,100,100, 30),"Server IP:");
						
				serverHost = GUI.TextField(Rect(140,100,140, 30),serverHost);
						
				GUI.Label(Rect(20,150,100, 30),"Server Port:");
						
				serverPort =  parseInt(GUI.TextField(Rect(140,150,140, 30),serverPort.ToString()));
						
				GUI.Label(Rect(20,200,100, 30),"Player Name:");
						
				playerName = GUI.TextField(Rect(140,200,140, 30),playerName);		
						
						
			    if(GUI.Button(Rect(20, 250, 260, 30), "Start")) {
					Time.timeScale = 1.0f;		
					StartMatchmaking();
				}
			}
			else{
				//the menu background box
		 		GUI.Box(Rect(0, 0, 300, 80), "");
		 		
				GUI.Label(Rect(20,10,100, 30),"Player Name:"); 
				playerName = GUI.TextField(Rect(140,10,140, 25),playerName);		 	
				if(GUI.Button(Rect(200, 45, 80, 25), "Submit")) {
					mmstarted = true;
				} 	
			
			}
				
		    //layout end
		    GUI.EndGroup(); 
		}
	}
	
	// Use this for initialization
	function StartMatchmaking() {
		//Initialize the server immediately if this instance is going to be
		//a server
		if(isServer)
		{
			var useNat = !Network.HavePublicAddress();
			Network.InitializeServer(maxConnections,serverPort,useNat);
			mmstarted = true;
		}
		else
		{
			Connect();
		}
	
	}
	
	var firstPlayer = true;
	
	// Use this for initialization
	function OnPlayerConnected() {
		if(firstPlayer)
		{
			firstPlayer = false;
				
			Player = InstantiatePlayer(Player);
			
			for(var go in enemyList){
				if(go){
					InstantiateGO(go);
				}
			}
			var scoreBoard = cam.GetComponent("ScoreBoardScript") as ScoreBoardScript;
			scoreBoard.StartGame();
		}
	
	}

	
	// Connect the client to the server
	public function Connect()
	{
		Network.Connect(serverHost, serverPort);
	}
	
	// Instantiate a player at the start location
	public function InstantiatePlayer( Player: GameObject) : GameObject
	{
		var pos :Vector3 = Player.transform.position;
		var rot:Quaternion = Player.transform.rotation;
		
		var resource = Resources.Load("PlayerPrefab");
		
		if(!resource)
			Debug.Log("Player not found");
		var player = Network.Instantiate(resource,pos,rot,NETWORK_GROUP) as GameObject;
		
		var sac1:SpawnAtCheckpoint = Player.GetComponent("SpawnAtCheckpoint") as SpawnAtCheckpoint;
		
		var sac2:SpawnAtCheckpoint = player.GetComponent("SpawnAtCheckpoint") as SpawnAtCheckpoint;
	
		
		if(sac1 && sac2)
			sac2.checkpoint = sac1.checkpoint;
		
		var stats:Stats = player.GetComponent("Stats")as Stats;
		stats.playerName = playerName;

		Destroy(Player);		
		return player;
	}
	
	//Instantiate any game Object
	public function InstantiateGO(go:GameObject){
		if(go){
		var resource = Resources.Load(go.name+"Prefab");
			if(resource){
				
				var scoreBoard = cam.GetComponent("ScoreBoardScript") as ScoreBoardScript;
			
				
				scoreBoard.IncBotCount();	
				
				var newobj:GameObject = Network.Instantiate(resource,go.transform.position,go.transform.rotation,NETWORK_GROUP);
								
				var complist = go.GetComponentsInChildren(PatrolMoveController,true);
				if(complist.length>0){
					var pmc :PatrolMoveController= (complist[0]) as PatrolMoveController;
		
					if(pmc){
						var complist2 = newobj.GetComponentsInChildren(PatrolMoveController,true);
						if(complist2.length>0){
							var pmc2:PatrolMoveController = (complist2[0]) as PatrolMoveController;
							if(pmc2){
								
								pmc2.SetPatrolRoute(pmc.patrolRoute);
							}
						}
						
					}
					
				}
				var newPar:Transform;
			
				
	
				if(go.transform.parent.gameObject.name=="ActiveRadius"){
				
					//newPar = go.transform.parent.parent;
					var par = go.transform.parent;
					Destroy(go);
					Destroy(par.gameObject);
					//newobj.transform.parent.parent = newPar;
				}else{
					//newPar = go.transform.parent;
					//newobj.active = go.active;
					//newobj.transform.parent = newPar;
					Destroy(go);
				}
				
				
				
			
			}
		}
		
	}
	
	// ############################################################
	// Register handlers for RakNet networking events
	// ############################################################
	public function OnConnectedToServer()
	{
		connected = true;
		mmstarted = true;
		// Join the client to the server
		// TODO
		
		
		Player  = InstantiatePlayer(Player);
		
		for(var go in enemyList){
			if(go){
				if(go.transform.parent.gameObject.name=="ActiveRadius"){
					Destroy(go.transform.parent.gameObject);
				}
				else
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
		var view:NetworkView  = Player.GetComponent(NetworkView) as NetworkView ;
		
		view.RPC("NewPlayerJoined",RPCMode.Server,playerName);
		view.RPC("SendBotCount",RPCMode.Server);
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
	
	function OnPlayerDisconnected(player: NetworkPlayer) {
    	Debug.Log("Clean up after player " +  player);
    	var view:NetworkView;
    	var stats : Stats;
    	var players = GameObject.FindGameObjectsWithTag("Player");
    	for(var p in players){
    		
    		view = p.networkView;
    		if(view.owner==player){
    			stats = p.GetComponent(Stats);
    			break;
    		}
    	}
    	if(stats){
    		var scoreBoard = cam.GetComponent("ScoreBoardScript") as ScoreBoardScript;
			scoreBoard.SetBotCount(scoreBoard.GetBotCount()-stats.botKills);
		
			view.RPC("GetBotCount",RPCMode.Others,scoreBoard.GetBotCount(),scoreBoard.GetTimeLeft());
    	}
    	
    	Network.DestroyPlayerObjects(player);
   	 	Network.RemoveRPCs(player);
	}
