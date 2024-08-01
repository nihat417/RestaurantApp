public class TableGroup
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int TableCount { get; set; }
    public int FreeTableCount { get; set; }
    public bool IsClicked { get; set; }
    public bool IsXTable { get; set; }
    public bool IsActive { get; set; }
    public int AllTableGroups { get; set; }
    public int PrintServerType { get; set; }
    public List<Table> Tables { get; set; }
}
