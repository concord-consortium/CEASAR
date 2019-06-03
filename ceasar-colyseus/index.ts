import http from "http";
import express from "express";
import cors from "cors";
import { Server } from "colyseus";
import { monitor } from "@colyseus/monitor";

import { CeasarRoom } from "./CeasarRoom";

const port = Number(process.env.PORT || 2567);
const app = express()
app.use(cors());

const server = http.createServer(app);
const gameServer = new Server({ server });

// register your room handlers
gameServer.register('ceasar', CeasarRoom);

// register colyseus monitor AFTER registering your room handlers
app.use("/colyseus", monitor(gameServer));

gameServer.listen(port);
console.log(`Listening on ws://localhost:${ port }`)
