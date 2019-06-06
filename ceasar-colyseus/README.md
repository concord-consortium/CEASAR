# `ceasar-colyseus`
Based on `create-colyseus-app`

## :crossed_swords: Usage
If this is the first time running the app, run `npm install`

Start MongoDB:

`mongod --dbpath=data`

Start server:

`npm start`

To regenerate the schema for the room:

`npx schema-codegen CeasarRoom.ts --csharp --output ../Assets/Scripts/Network/`

## Structure

- `index.ts`: main entry point, register an empty room handler and attach [`@colyseus/monitor`](https://github.com/colyseus/colyseus-monitor)
- `MyRoom.ts`: an empty room handler for you to implement your logic
- `loadtest/example.ts`: scriptable client for the loadtest tool (see `npm run loadtest`)
- `package.json`:
    - `scripts`:
        - `npm start`: runs `ts-node index.ts`
        - `npm run loadtest`: runs the [`@colyseus/loadtest`](https://github.com/colyseus/colyseus-loadtest/) tool for testing the connection, using the `loadtest/example.ts` script.
    - `dependencies`:
        - `colyseus`
        - `@colyseus/monitor`
        - `express`
    - `devDependencies`
        - `ts-node`
        - `typescript`
        - `@colyseus/loadtest`
- `tsconfig.json`: TypeScript configuration file


## License

MIT
