import { Room, Client } from "colyseus";
import { Schema, type, MapSchema } from "@colyseus/schema";

export class Player extends Schema {
  @type("number")
  x = Math.floor(Math.random() * 10);

  @type("number")
  y = Math.floor(Math.random() * 10);
}
export class State extends Schema {
  @type({ map: Player })
  players = new MapSchema<Player>();

  something = "This attribute won't be sent to the client-side";

  createPlayer (id: string) {
      this.players[id] = new Player();
  }

  removePlayer (id: string) {
      delete this.players[ id ];
  }

  movePlayer (id: string, movement: any) {
      if (movement.x) {
          this.players[id].x += movement.x * 10;

      } else if (movement.y) {
          this.players[id].y += movement.y * 10;
      }
  }
}
export class CeasarRoom extends Room<State> {
  onInit(options: any) {
    console.log("CeasarRoom created!", options);
    this.setState(new State());
  }
  onJoin(client: Client, options: any) {
    this.state.createPlayer(client.sessionId);
    this.broadcast(`${client.sessionId} joined.`);
  }
  onMessage(client: Client, data: any) {
    console.log("CeasarRoom received message from", client.sessionId, ":", data);
    this.state.movePlayer(client.sessionId, data);
    this.broadcast(`(${client.sessionId}) ${data.message}`);
  }
  onLeave(client: Client, consented: boolean) {
    this.state.removePlayer(client.sessionId);
    this.broadcast(`${client.sessionId} left.`);
  }
  onDispose() {
    console.log("Dispose CeasarRoom");
  }
}
/*

export class CeasarRoom extends Room {
  onInit(options: any) {
    console.log("BasicRoom created!", options);
  }
  onJoin(client: Client, options: any) {
    this.broadcast(`${client.sessionId} joined.`);
  }
  onMessage(client: Client, message: any) {
    console.log("BasicRoom received message from", client.sessionId, ":", message);
    this.broadcast(`(${client.sessionId}) ${message.message}`);
  }
  onLeave(client: Client, consented: boolean) {
    this.broadcast(`${client.sessionId} left.`);
  }
  onDispose() {console.log("Dispose BasicRoom");}
}

*/