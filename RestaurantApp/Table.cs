public class Table
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? LockedUser { get; set; }
    public string? InsertDate { get; set; }
    public string? SumTotal{ get; set; }
    public string? UserInsertName { get; set; }
    public int TableNumber { get; set; }
    public int IdDepartment { get; set; }
    public int TableStatusId { get; set; }
    public int IdParent { get; set; }
    public int OrderId { get; set; }
    public int PersonCount { get; set; }
    public int OrderReady { get; set; }
    public bool IsLocked { get; set; }
    public int TotalDecreased { get; set; }
    public int CustomerId { get; set; }
    public int NoteId { get; set; }
    public int UserInsertId { get; set; }
    public int LockedUserId { get; set; }
    public bool IsSelected { get; set; }
    public int PastMins { get; set; }
    public bool IsOrderAlarm { get; set; }
    public bool HasTriggeredDetails { get; set; }
    public bool IsActive { get; set; }
}