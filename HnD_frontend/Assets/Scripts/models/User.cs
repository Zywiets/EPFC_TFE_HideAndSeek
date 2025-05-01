using UnityEngine;

[System.Serializable]
public class Movement {
    public float x;
    public float y;

    public Movement(float x, float y) {
        this.x = x;
        this.y = y;
    }
}

[System.Serializable] // Add this attribute
public class User {
    public int id;
    public string username;
    public string password;
    public string email;
    public string score;
    public int scoreTotal;
    public string socketId;
    public string lobbyId;
    public Point point;
    public Movement movement;

    public override string ToString() {
        return id + " " + username + " " + password + " " + email + " " + score + " " + scoreTotal + " " + socketId + " " + lobbyId + " " + point;
    }
}