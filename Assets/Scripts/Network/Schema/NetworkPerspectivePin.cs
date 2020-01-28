// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 0.5.14
// 

using Colyseus.Schema;

public class NetworkPerspectivePin : Schema {
	[Type(0, "ref", typeof(NetworkTransform))]
	public NetworkTransform location = new NetworkTransform();

	[Type(1, "number")]
	public float latitude = 0;

	[Type(2, "number")]
	public float longitude = 0;

	[Type(3, "number")]
	public float datetime = 0;
}

