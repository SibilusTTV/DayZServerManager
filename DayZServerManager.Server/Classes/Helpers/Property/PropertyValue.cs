using System.Text.Json;

namespace DayZServerManager.Server.Classes.Helpers.Property
{
    public class PropertyValue
    {
        private int _id;
        private string _propertyName;
        private DataType _dataType;
        private object _value;
        private string _comment;

        public int Id { get { return _id; } }
        public string PropertyName { get { return _propertyName; } }
        public DataType DataType { get { return _dataType; } set { _dataType = value; } }
        public object Value { get { return _value; } set { _value = value; } }
        public string Comment { get { return _comment; } set { _comment = value; } }

        public PropertyValue(int id, string PropertyName, DataType DataType, object Value, string Comment)
        {
            _id = id;
            _propertyName = PropertyName;
            _dataType = DataType;
            if (Value is JsonElement)
            {
                JsonElement jsonValue = (JsonElement)Value;
                switch (DataType)
                {
                    case DataType.Boolean:
                        _value = jsonValue.GetBoolean();
                        break;
                    case DataType.Decimal:
                        _value = jsonValue.GetSingle();
                        break;
                    case DataType.Number:
                        _value = jsonValue.GetInt64();
                        break;
                    case DataType.Text:
                    case DataType.TextLong:
                    case DataType.TextShort:
                        string? stringValue = jsonValue.GetString();
                        if (stringValue != null)
                        {
                            _value = stringValue;
                        }
                        else
                        {
                            _value = jsonValue.GetRawText();
                        }
                        break;
                    case DataType.Array:
                        JsonElement.ArrayEnumerator enumerator = jsonValue.EnumerateArray();
                        List<string> values = new List<string>();
                        foreach (JsonElement element in enumerator)
                        {
                            string? elementStringValue = element.GetString();
                            if (elementStringValue != null)
                            {
                                values.Add(elementStringValue);
                            }
                            else
                            {
                                values.Add(jsonValue.GetRawText());
                            }
                        }
                        _value = values;
                        break;
                    default:
                        _value = jsonValue;
                        break;
                }
            }
            else
            {
                _value = Value;
            }
            _comment = Comment;
        }
    }
}
