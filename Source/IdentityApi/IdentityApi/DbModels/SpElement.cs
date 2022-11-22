using System.Data;

namespace IdentityApi.DbModels
{
    public class SpElement
    {
        public SpElement(string key, object value, SqlDbType valueType)
        {
            Key = key;
            Value = value;
            ValueType = valueType;
        }

        public string Key { get; set; }
        public object Value { get; set; }
        public SqlDbType ValueType { get; set; }
    }
}
