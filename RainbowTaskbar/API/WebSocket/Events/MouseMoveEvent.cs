namespace RainbowTaskbar.API.WebSocket.Events; 

public class MouseMoveEvent : WebSocketAPIEvent {
    public double X { get; }
    public double Y { get; }

    public MouseMoveEvent(double x, double y) : base("MouseMove") {
        X = x;
        Y = y;
    }
}