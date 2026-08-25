
namespace Domain.Mission.TypesChanges;

public class TypesChangesItem
{
    public string name { get; set; }
    public int? nominal { get; set; }
    public long? lifetime { get; set; }
    public int? restock { get; set; }
    public int? min { get; set; }
    public int? quantmin { get; set; }
    public int? quantmax { get; set; }
    public int? cost { get; set; }
    public FlagsChangesItem? flags { get; set; }
    public string? category { get; set; }
    public List<string>? usage { get; set; }
    public List<string>? value { get; set; }
}
