using System.Collections.Generic;


// Record class representing Solyseus server endpoint
public class ServerRecord {
    public string name;
    public string address;

    public ServerRecord(string _name, string _address)
    {
        name = _name;
        address = _address;
    }
}

// Utility class containing our server definitions
public class ServerList
{
    List<ServerRecord> Servers;

    public static ServerRecord Local = new ServerRecord("Local", "ws://localhost:2567");
    public static ServerRecord Dev = new ServerRecord("Dev", "ws://localhost:2567");
    public static ServerRecord Web = new ServerRecord("Web", "ws://localhost:2567");

    // track our single instance:
    private static ServerList instance;

    // can't use constructor, guaranteed singleton
    protected ServerList() {
        Servers = new List<ServerRecord>
        {
            Local,
            Dev,
            Web
        };
    }

    // Currently not used
    public void AddServer(string name, string address)
    {
        ServerRecord server = new ServerRecord(name, address);
        Servers.Add(server);
    }
    
    // The only way to access this class
    public static ServerList GetInstance()
    {
        return instance ?? (instance = new ServerList());
    }

    public static List<ServerRecord> List()
    {
        return GetInstance().Servers;
    }
}