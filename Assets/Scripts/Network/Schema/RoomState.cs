// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 0.5.41
// 

using Colyseus.Schema;

public class RoomState : Schema {
	[Type(0, "map", typeof(MapSchema<NetworkPlayer>))]
	public MapSchema<NetworkPlayer> players = new MapSchema<NetworkPlayer>();
}

