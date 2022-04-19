namespace RainbowTaskbar.API.WebSocket.Events; 

public class MouseUpEvent : WebSocketAPIEvent {

    public double X { get; }
    public double Y { get; }

    public MouseStates MouseStates { get; }

    public MouseUpEvent(double x, double y, MouseStates mouseStates) : base("MouseUp") {
        X = x;
        Y = y;

        MouseStates = mouseStates;
    }
}