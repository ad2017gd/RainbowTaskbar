namespace RainbowTaskbar.API.WebSocket.Events; 

public class MouseDownEvent : WebSocketAPIEvent {

    public double X { get; }
    public double Y { get; }

    public MouseStates MouseStates { get; }

    public MouseDownEvent(double x, double y, MouseStates mouseStates) : base("MouseDown") {
        X = x;
        Y = y;

        MouseStates = mouseStates;
    }
}