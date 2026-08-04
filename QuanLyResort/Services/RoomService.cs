using QuanLyResort.Models;
using QuanLyResort.Repositories;

namespace QuanLyResort.Services;

public class RoomService : IRoomService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public RoomService(IUnitOfWork unitOfWork, IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<IEnumerable<Room>> GetAllRoomsAsync()
    {
        return await _unitOfWork.Rooms.GetAllAsync();
    }

    public async Task<Room?> GetRoomByIdAsync(int roomId)
    {
        return await _unitOfWork.Rooms.GetByIdAsync(roomId);
    }

    public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(string? roomType = null)
    {
        var rooms = await _unitOfWork.Rooms.FindAsync(r => r.IsAvailable && r.HousekeepingStatus == "Ready");

        if (!string.IsNullOrEmpty(roomType))
        {
            rooms = rooms.Where(r => r.RoomType.Equals(roomType, StringComparison.OrdinalIgnoreCase));
        }

        return rooms;
    }

    public async Task<bool> UpdateRoomStatusAsync(int roomId, bool isAvailable, string housekeepingStatus)
    {
        var room = await _unitOfWork.Rooms.GetByIdAsync(roomId);
        if (room == null)
            return false;

        var oldStatus = $"Available: {room.IsAvailable}, Housekeeping: {room.HousekeepingStatus}";
        
        room.IsAvailable = isAvailable;
        room.HousekeepingStatus = housekeepingStatus;
        room.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Rooms.Update(room);
        await _unitOfWork.SaveChangesAsync();

        var newStatus = $"Available: {isAvailable}, Housekeeping: {housekeepingStatus}";
        await _auditService.LogAsync("Room", roomId, "UpdateStatus", null, oldStatus, newStatus);

        return true;
    }

    public async Task<Room> CreateRoomAsync(Room room)
    {
        room.CreatedAt = DateTime.UtcNow;
        await _unitOfWork.Rooms.AddAsync(room);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync("Room", room.RoomId, "Create", null, null, 
            $"Room {room.RoomNumber} created");

        return room;
    }

    public async Task<bool> UpdateRoomAsync(Room room)
    {
        var existingRoom = await _unitOfWork.Rooms.GetByIdAsync(room.RoomId);
        if (existingRoom == null)
            return false;

        room.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Rooms.Update(room);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync("Room", room.RoomId, "Update", null, null, 
            $"Room {room.RoomNumber} updated");

        return true;
    }
}

