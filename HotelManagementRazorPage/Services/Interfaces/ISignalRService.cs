namespace Services.Interfaces
{
    public interface ISignalRService
    {
        Task SendRoomStatusUpdate(string message);
    }
}
