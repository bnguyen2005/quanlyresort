using QuanLyResort.Models;

namespace QuanLyResort.Services;

public interface IRoomService
{
    Task<IEnumerable<Room>> GetAllRoomsAsync();
    Task<Room?> GetRoomByIdAsync(int roomId);
    Task<IEnumerable<Room>> GetAvailableRoomsAsync(string? roomType = null);
    Task<bool> UpdateRoomStatusAsync(int roomId, bool isAvailable, string housekeepingStatus);
    Task<Room> CreateRoomAsync(Room room);
    Task<bool> UpdateRoomAsync(Room room);
}

