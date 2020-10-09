// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 0.5.41
// 

using Colyseus.Schema;

public class UpdateMessage : Schema {
	[Type(0, "string")]
	public string updateType = "";

	[Type(1, "string")]
	public string playerId = "";

	[Type(2, "string")]
	public string metadata = "";
}

