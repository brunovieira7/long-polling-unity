using Colyseus;
using Colyseus.Schema;
public static class RoomManager {
    public static Room<State> room { get; set; }

    public static Client client { get; set; }
}