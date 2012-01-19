#pragma strict

// Public member data
public var behaviourOnSpotted : MonoBehaviour;
public var soundOnSpotted : AudioClip;
public var behaviourOnLostTrack : MonoBehaviour;

// Private memeber data
private var character : Transform;
private var player : Transform;
private var insideInterestArea : boolean = true;

function Awake () {
	character = transform;
	if(!player){
		var players = GameObject.FindGameObjectsWithTag("Player");
		var min : Vector3 = Vector3.zero;
		for(var p in players){
			if(min==Vector3.zero || (p.transform.position-transform.position).magnitude < min.magnitude){
				min = (p.transform.position - transform.position);
				player = p.transform;
			}
		}	
	}

}

function Update(){
	if(insideInterestArea){
		var players = GameObject.FindGameObjectsWithTag("Player");
		var min : Vector3 = Vector3.zero;
		for(var p in players){
			if(min==Vector3.zero || (p.transform.position-transform.position).magnitude < min.magnitude){
				min = (p.transform.position - transform.position);
				player = p.transform;
			}
		}	
	}

}


function OnEnable () {
	behaviourOnLostTrack.enabled = true;
	behaviourOnSpotted.enabled = false;
}

function OnTriggerEnter (other : Collider) {
	//Debug.Log('AI trigger fired!');
	for(var p:GameObject in GameObject.FindGameObjectsWithTag("Player")){
		player = p.transform;
	if (other.transform == player && CanSeePlayer ()) {
		OnSpotted ();
		//Debug.Log("attacking player "+p.ToString());
		break;
	}
	}
}

function OnEnterInterestArea () {
	insideInterestArea = true;
}

function OnExitInterestArea () {
	insideInterestArea = false;
	OnLostTrack ();
}

function OnSpotted () {
	if (!insideInterestArea)
		return;
	if (!behaviourOnSpotted.enabled) {
		behaviourOnSpotted.enabled = true;
		behaviourOnLostTrack.enabled = false;
		
		if (audio && soundOnSpotted) {
			audio.clip = soundOnSpotted;
			audio.Play ();
		}
	}
}

function OnLostTrack () {
	if (!behaviourOnLostTrack.enabled) {
		behaviourOnLostTrack.enabled = true;
		behaviourOnSpotted.enabled = false;
	}
}

function CanSeePlayer () : boolean {
	var playerDirection : Vector3 = (player.position - character.position);
	var hit : RaycastHit;
	Physics.Raycast (character.position, playerDirection, hit, playerDirection.magnitude);
	if (hit.collider && hit.collider.transform == player) {
		return true;
	}
	return false;
}
