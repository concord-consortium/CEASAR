// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 0.5.14
// 

using Colyseus.Schema;

public class Player : Schema {
	[Type(0, "string")]
	public string id = "";

	[Type(1, "string")]
	public string username = "";

	[Type(2, "string")]
	public string currentScene = "";

	[Type(3, "ref", typeof(NetworkTransform))]
	public NetworkTransform playerPosition = new NetworkTransform();

	[Type(4, "ref", typeof(NetworkTransform))]
	public NetworkTransform interactionTarget = new NetworkTransform();

	[Type(5, "ref", typeof(NetworkTransform))]
	public NetworkTransform locationPin = new NetworkTransform();

	[Type(6, "ref", typeof(NetworkCelestialObject))]
	public NetworkCelestialObject celestialObjectTarget = new NetworkCelestialObject();

	[Type(7, "array", typeof(ArraySchema<NetworkTransform>))]
	public ArraySchema<NetworkTransform> annotations = new ArraySchema<NetworkTransform>();

	[Type(8, "boolean")]
	public bool connected = false;
}

