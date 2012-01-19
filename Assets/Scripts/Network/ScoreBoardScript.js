public var botCount = 0;
public var gameTimeLimit: float;


private var gameStarted = false;
private var hidden = true;
private var gameOver = false;
private var gameStartTime: float;



function Update(){
	if(gameOver || !gameStarted)
		return;

	if(Input.GetKeyDown("tab")){
		hidden = false;
	}
	
	if(Input.GetKeyUp("tab")){
		hidden = true;
	}
}


function OnGUI(){
	if(!gameStarted)
		return;
	var botsLeft = botCount;
	
	var players = GameObject.FindGameObjectsWithTag("Player");
	var statlist = new ArrayList(Stats);
	
	var stats : Stats;
	for(var p in players){
	
		stats = p.GetComponent(Stats) as Stats;
	
		if(stats){
			botsLeft-=stats.botKills;
			if(!hidden){
				var inserted= false;
				for(var i = 0; i<statlist.Count; i++){
					var thisStat = statlist[i] as Stats;
					if(stats.score>thisStat.score){
					/*
						statlist.length++;
						for(var j = statlist.length-1; j>i;j--){
							statlist[j]=statlist[j-1];
						}
						statlist[i] = stats;
					*/
						statlist.Insert(i,stats);
						inserted = true;
						break;
					}
				}
				if(!inserted)
					statlist.Add(stats);
			}
		}
	}
	
	if(!hidden){
		
		
	GUI.BeginGroup(Rect(Screen.width/2-200, Screen.height/2-200, 400, 400));	
	
	  //the menu background box
    GUI.Box(Rect(0, 0, 400, 400), "");	
	
	GUI.Label(Rect(10,10,380, 30),"Angry Bots Multiplayer");
				
	GUI.Label(Rect(10,40,76, 30),"Player");
	GUI.Label(Rect(86,40,76, 30),"Bot Kills");
	GUI.Label(Rect(162,40,76, 30),"Player Kills");
	GUI.Label(Rect(238,40,76, 30),"Deaths");
	GUI.Label(Rect(314,40,76, 30),"Score");
							

	var height = 80;
	
	for(stats in statlist){
		var stat = stats as Stats;
		GUI.Label(Rect(10,height,76, 30),stat.playerName+"");
		GUI.Label(Rect(86,height,76, 30),stat.botKills+"");
		GUI.Label(Rect(162,height,76, 30),stat.playerKills+"");
		GUI.Label(Rect(238,height,76, 30),stat.deaths+"");
		GUI.Label(Rect(314,height,76, 30),stat.score+"");			
		height+=30;
	
	}
	if(gameOver){
		height+=30;
		GUI.Label(Rect(10,height,380, 40),(statlist[0] as Stats).playerName+" wins the game!");	
	}
     //layout end
    GUI.EndGroup(); 
	}
	
	GUI.BeginGroup(Rect(Screen.width-110, 30, 80, 80));	
	    
    
  
  //the menu background box
    GUI.Box(Rect(0, 0, 80, 80), "");
   
	GUI.Label(Rect(10,10,60, 30),"Bots Left: ");
	GUI.Label(Rect(10,25,60, 30),botsLeft+"");
	
	GUI.Label(Rect(10,40,60, 30),"Time Left:");
	var t = gameTimeLimit - (Time.time - gameStartTime);
	
	var m: int = t/60;
	var s: int = t%60;
	
	GUI.Label(Rect(10,55,60, 30),m+":"+(s<10?"0":"")+s);
	
	
			
	if(botsLeft==0 || t<=0){
 	
 		gameOver = true;
 		hidden = false;
 		Time.timeScale = 0;
 		 	
 	}

	GUI.EndGroup();

}

function StartGame(tl: float){
	gameStarted = true;
	gameStartTime = tl + Time.time- gameTimeLimit;
}
function StartGame(){
	gameStarted = true;
	gameStartTime = Time.time;
}


function GetTimeLeft(): float{
	return gameTimeLimit - (Time.time - gameStartTime);
}


function IncBotCount(){
	botCount++;
}

function GetBotCount(){
	return botCount;
}

function SetBotCount(b:int){
	botCount = b;
}