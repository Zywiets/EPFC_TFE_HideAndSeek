using System;
using System.Collections.Generic;

[Serializable]
public class Lobby {
    public string id;
    public string name;
    public List<User> users = new();
    public List<Point> spawnPoints = new();
}

[Serializable]
public class LobbyIsHost {
    public Lobby lobby;
    public bool isLobbyHost;
}