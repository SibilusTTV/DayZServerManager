namespace DayZServerManager.Server.Classes.Helpers.Property
{
    public class PropertyValue
    {
        public int id { get; }
        public string PropertyName { get; }
        public DataType DataType { get; }
        public object Value { get; set; }
        public string Comment { get; set; }

        public PropertyValue(int id, string PropertyName, DataType DataType, object Value, string Comment)
        {
            this.id = id;
            this.PropertyName = PropertyName;
            this.DataType = DataType;
            this.Value = Value;
            this.Comment = Comment;
        }
    }
}
