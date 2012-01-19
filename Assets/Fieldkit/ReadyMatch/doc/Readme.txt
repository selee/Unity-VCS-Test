Introduction to ReadyMatch 
==========================

ReadyMatch is the fastest, easiest way to add multiplayer matchmaking 
to Unity games on – or across – any platform. Join your players 
instantly to open matches, connecting up to N players to a designated 
host. Be up and running in a matter of minutes. No complex UI to design 
– just drop in a “Play Now” button and you’re done! ReadyMatch sits on 
top of the built-in Unity networking components. In one “Play Now” step, 
ReadyMatch detects the network configuration of the client and connects 
to the first available game that matches the player’s network 
configuration. 


To make ReadyMatch integration easy to understand, we have built a 
multi-player version of the free Unity game AngryBots. You can download 
it from the FieldKit portal at: 

http://fieldkit.poweredbygamespy.com 

Package Contents
================

ReadyMatch.cs 			One script to rule them all! 

Components
==========
ReadyMatch is the single component you will need to include somewhere in 
your scene to make matches between game clients. A popular option is to 
add the ReadyMatch component to the Main Camera or to a Network 
Controller prefab that will be present in the scene when matchmaking 
occurs. 

ReadyMatch Parameters 
=====================
Matchmaking exposes the following parameters: 

Application ID
-------------- 

This is the ID your game receives when registered through the portal. 
ReadyMatch receives authorization for matchmaking using this as a key. 


Use GUI 
-------

ReadyMatch contains a basic skinnable UI and button to initiate 
matchmaking. Setting this value to true indicates that you want to use 
this GUI. 


Min Players 
-----------

The minimum number of players required to start the game. As soon as 
this number of players joins, the match starts. 


Max Players
----------- 

The maximum allowable number of players. Players can join the game in 
progress until this number is hit. Time To Wait As Server This is the 
number of seconds between attempts at joining a match. During this time, 
the game will create a server and wait for other players to join. Port 
The port this game uses to connect Master ServerURL The URL of the 
matchmaking service. Defaults to the correct GameSpy server. Player 
Options A Hashtable that contains the custom game parameters for your 
game. Valid Items to place in this Hashtable are: String Float Int 
ArrayList Hashtable bool null For version 1.0, this data is sent and 
received by ReadyMatch but is not used directly in making matches. 


ReadyMatch Functions 
====================

ReadyMatch also exposes the following public functions. 

Join 
----

Called when the player wants to begin matchmaking. When 
using the included menu, the button calls this function. 

GetMatchStatusCode 
------------------

Returns the following codes for the given conditions 
	-2 -- unable to connect to the matchmaking service. 
	-1 -- the computer is not connected to a network. 
	0 -- matchmaking has not been started 
	1 -- the application is not yet authorized with this service. 
	2 -- waiting for the connection to be tested 
	3 -- the service is ready to begin matchmaking 
	4 -- matchmaking has started but no match has yet been made 
	5 -- a match has been found, but the game has not yet begun 
	6 -- matchmaking has successfully found a game and has started the match 

GetMatchStatus
-------------- 

Returns a string that is explains the current status of 
matchmaking. 

GetPlayerCount
-------------- 

Returns the number of players currently in 
the game. 

