#pragma strict

public var Player : GameObject;
public var mouseDownSignals : SignalSender;
public var mouseUpSignals : SignalSender;

function Update () {
	if((Network.peerType != NetworkPeerType.Client && Network.peerType != NetworkPeerType.Server) || !Player || !Player.GetComponent("NetworkView") || (Player.GetComponent("NetworkView") as NetworkView).isMine){
	
	if (Input.GetMouseButtonDown(0))
		mouseDownSignals.SendSignals (this);
	
	if (Input.GetMouseButtonUp(0))
		mouseUpSignals.SendSignals (this);
	}
}
