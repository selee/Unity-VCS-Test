using UnityEngine;
using System.Collections;

public class MessengerBehaviour : MonoBehaviour {
	
	private bool matchStarted = false;
	private int players = 1;
	public bool updateAvailable = false;
	
	
	/// <summary>
	/// rpc called when the match starts on the server. 
	/// </summary>
	/// <param name="p">
	/// A <see cref="System.Int32"/>
	/// </param>
	[RPC]
	void OnMatchStarted (int p)
	{
		matchStarted = true;
		players = p;
		updateAvailable = true;
	}

	/// <summary>
	/// rpc called when the player count changes on the server. 
	/// </summary>
	/// <param name="p">
	/// A <see cref="System.Int32"/>
	/// </param>
	[RPC]
	void OnPlayerCountChange (int p)
	{
		players = p;
		updateAvailable = true;
	}
	
	public int GetPlayers(){
		return players;	
	}
	
	public bool IsMatchStarted(){
		return matchStarted;	
	}
}
