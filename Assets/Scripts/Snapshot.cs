using System;

public class Snapshot
{
    public DateTime dateTime;
    public string location;
    public Snapshot(DateTime dt, string loc)
    {
        dateTime = dt;
        location = loc;
    }
}
