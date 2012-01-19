
public var playerName: String;
public var botKills: int;
public var playerKills: int;
public var deaths: int;

public var damageDealt: float;
public var damageTaken: float;

public var score: int;

function OnBotKill(sc:int){
	Debug.Log("sending updated scoreboard");
	score+=sc;
	botKills++;
	if(Network.peerType!=NetworkPeerType.Disconnected){
		for(var x in GameObject.FindGameObjectsWithTag("Player")){
			
			var view2:NetworkView = x.GetComponent(NetworkView);
			
			var s = x.GetComponent(Stats);
				
			view2.RPC("NewPlayerReply",RPCMode.Others,s.playerName,s.botKills, s.playerKills, s.deaths, s.damageDealt, s.damageTaken, s.score);
			
		}
	}
}

function OnDamageDealt(d:float){
	damageDealt+=d;
}

function OnDamageTaken(d:float){
	damageTaken+=d;
}

function OnDeath(){
	deaths++;
}

function OnPlayerKill(sc:int){
	score+=sc;
	playerKills++;
	if(Network.peerType!=NetworkPeerType.Disconnected){
		for(var x in GameObject.FindGameObjectsWithTag("Player")){
			
			var view2:NetworkView = x.GetComponent(NetworkView);
			
			var s = x.GetComponent(Stats);
				
			view2.RPC("NewPlayerReply",RPCMode.Others,s.playerName,s.botKills, s.playerKills, s.deaths, s.damageDealt, s.damageTaken, s.score);
			
		}
	}
}

	@RPC	
	public function NewPlayerJoined(name: String,  info: NetworkMessageInfo)
	{
		var view:NetworkView = GetComponent("NetworkView") as NetworkView;
		if(view.owner == info.sender){
			playerName = name;
		}
		
		for(var x in GameObject.FindGameObjectsWithTag("Player")){
			
			var view2:NetworkView = x.GetComponent(NetworkView);
			
			var s = x.GetComponent(Stats);
				
			view2.RPC("NewPlayerReply",RPCMode.Others,s.playerName,s.botKills, s.playerKills, s.deaths, s.damageDealt, s.damageTaken, s.score);
			
		}				
	}
	
	@RPC	
	public function NewPlayerReply( name: String, bk:int, pk:int, d:int, dd:float,dt:float,s:int, info: NetworkMessageInfo)
	{
		Debug.Log(name);

		var view:NetworkView = GetComponent("NetworkView") as NetworkView;
				
		
		playerName = name;
		botKills = bk;
		playerKills = pk;
		deaths = d;
		damageDealt = dd;
		damageTaken = dt;
		score = s;
		
				
	}
	
	@RPC
	public function SendBotCount(info:NetworkMessageInfo){
		var view:NetworkView  = GetComponent(NetworkView) as NetworkView ;
		
		var cam = GameObject.FindWithTag("MainCamera");
		var scoreBoard = cam.GetComponent("ScoreBoardScript") as ScoreBoardScript;
		view.RPC("GetBotCount",info.sender,scoreBoard.GetBotCount(),scoreBoard.GetTimeLeft());
	}
	
	
	@RPC
	public function GetBotCount(botCount: int, timeLeft: float){	
		var cam = GameObject.FindWithTag("MainCamera");
		var scoreBoard = cam.GetComponent("ScoreBoardScript") as ScoreBoardScript;
		scoreBoard.SetBotCount(botCount);
		scoreBoard.StartGame(timeLeft);
	}