namespace RainbowTaskbar.API.WebSocket.Events; 

public class MouseStates {
    public bool Left { get; }
    public bool Right { get; }
    public bool Middle { get; }

    public MouseStates(bool left, bool right, bool middle) {
        Left = left;
        Right = right;
        Middle = middle;
    }
}