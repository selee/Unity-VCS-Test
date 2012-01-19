#pragma strict

public var mouseDownSignals : SignalSender;
public var mouseUpSignals : SignalSender;

private var state : boolean = false;

#if UNITY_IPHONE || UNITY_ANDROID
private var joysticks : Joystick[];

function Start () {
	joysticks = FindObjectsOfType (Joystick) as Joystick[];	
}
#endif

function Update () {
 if (Network.peerType == NetworkPeerType.Client || Network.peerType == NetworkPeerType.Server){
	var view:NetworkView;
		
#if UNITY_IPHONE || UNITY_ANDROID
	if (state == false && joysticks[0].tapCount > 0) {
		view  = GetComponent("NetworkView");
	
		view.RPC("NetDown",RPCMode.Others);
		state = true;
	}
	else if (joysticks[0].tapCount <= 0) {
		view  = GetComponent("NetworkView");
	
		view.RPC("NetUp",RPCMode.Others);
		state = false;
	}	
#else	
	#if !UNITY_EDITOR && (UNITY_XBOX360 || UNITY_PS3)
		// On consoles use the right trigger to fire
		var fireAxis : float = Input.GetAxis("TriggerFire");
		if (state == false && fireAxis >= 0.2) {
			view  = GetComponent("NetworkView");
	
		view.RPC("NetDown",RPCMode.Others);
			state = true;
		}
		else if (state == true && fireAxis < 0.2) {
			view  = GetComponent("NetworkView");
	
		view.RPC("NetUp",RPCMode.Others);
			state = false;
		}
	#else
		if (state == false && Input.GetMouseButtonDown (0)) {
			view  = GetComponent("NetworkView");
	
		view.RPC("NetDown",RPCMode.Others);
			state = true;
		}
		
		else if (state == true && Input.GetMouseButtonUp (0)) {
			view  = GetComponent("NetworkView");
	
		view.RPC("NetUp",RPCMode.Others);
			state = false;
		}
	#endif
#endif
}
	
}

@RPC
function NetUp(info:NetworkMessageInfo){
	var view:NetworkView  = GetComponent("NetworkView");

	if(info.sender == view.owner){

		mouseUpSignals.SendSignals (this);	
	}
}


@RPC
function NetDown(info:NetworkMessageInfo){
var view:NetworkView  = GetComponent("NetworkView");
	
	if(info.sender == view.owner){
		mouseDownSignals.SendSignals (this);
	}
}