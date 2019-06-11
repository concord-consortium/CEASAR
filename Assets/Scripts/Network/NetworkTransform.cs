// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 0.4.34
// 

using Colyseus.Schema;

public class NetworkTransform : Schema {
	[Type(0, "ref", typeof(NetworkPosition))]
	public NetworkPosition position = new NetworkPosition();

	[Type(1, "ref", typeof(NetworkRotation))]
	public NetworkRotation rotation = new NetworkRotation();
}

