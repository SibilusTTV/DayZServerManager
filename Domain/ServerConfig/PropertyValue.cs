using System.Text.Json;

namespace Domain.ServerConfig;

public class PropertyValue
{
    public string PropertyName { get; set; }
    public string Value { get; set; }
    public string Comment { get; set; }

    public PropertyValue()
    {
        
    }

    public PropertyValue(string PropertyName, string Value, string Comment)
    {
        this.PropertyName = PropertyName;
        this.Value = Value;
        this.Comment = Comment;
    }
}