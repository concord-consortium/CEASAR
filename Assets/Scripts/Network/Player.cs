// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 0.4.34
// 

using Colyseus.Schema;

public class Player : Schema {
	[Type(0, "string")]
	public string id = "";

	[Type(1, "string")]
	public string username = "";

	[Type(2, "string")]
	public string currentScene = "";

	[Type(3, "number")]
	public float x = 0;

	[Type(4, "number")]
	public float y = 0;

	[Type(5, "ref", typeof(NetworkTransform))]
	public NetworkTransform playerPosition = new NetworkTransform();

	[Type(6, "ref", typeof(NetworkTransform))]
	public NetworkTransform interactionTarget = new NetworkTransform();
}

