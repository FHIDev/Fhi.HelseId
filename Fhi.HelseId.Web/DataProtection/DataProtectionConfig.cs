namespace Fhi.HelseId.Web.DataProtection
{
    public class DataProtectionConfig
    {
        public bool Enabled { get; set; }
        public string ConnectionString { get; set; } = "";
        public string Schema { get; set; } = "DataProtection";
        public string TableName { get; set; } = "Keys";
    }
}
