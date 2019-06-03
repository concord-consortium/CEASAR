import { Room, Client } from "colyseus";
import { Schema, type, MapSchema } from "@colyseus/schema";

export class Player extends Schema {
  @type("string")
  id = "";

  @type("string")
  username = "";

  @type("string")
  currentScene = "stars";

  @type("number")
  x = Math.floor(Math.random() * 10) - 5;

  @type("number")
  y = Math.floor(Math.random() * 10) - 5;
}
export class State extends Schema {
  @type({ map: Player })
  players = new MapSchema<Player>();

  createPlayer (id: string) {
      this.players[id] = new Player();
  }

  removePlayer (id: string) {
      delete this.players[ id ];
  }

  movePlayer(id: string, movement: any) {
    if (movement.posX) {
      this.movePlayerToPosition(id, movement.posX, movement.posY);
    } else {
      if (movement.x) {
        this.players[id].x += movement.x;

      } else if (movement.y) {
        this.players[id].y += movement.y;
      }
    }
  }

  movePlayerToPosition(id: string, posX: number, posY: number) {
    this.players[id].x = posX;
    this.players[id].y = posY;
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
