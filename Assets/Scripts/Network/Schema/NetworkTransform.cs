// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 0.5.41
// 

using Colyseus.Schema;

public class NetworkTransform : Schema {
	[Type(0, "ref", typeof(NetworkVector3))]
	public NetworkVector3 position = new NetworkVector3();

	[Type(1, "ref", typeof(NetworkVector3))]
	public NetworkVector3 rotation = new NetworkVector3();

	[Type(2, "ref", typeof(NetworkVector3))]
	public NetworkVector3 localScale = new NetworkVector3();

	[Type(3, "string")]
	public string name = "";
}

