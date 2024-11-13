using System.Collections.Generic;
using System.Text.Json.Serialization;

public class ApprovalResponse
{
    [JsonPropertyName("approvals")]
    public List<Approval> Approvals { get; set; } = new List<Approval>();
    [JsonPropertyName("hpr_number")]
    public long HprNumber { get; set; }

    public class Approval
    {
        [JsonPropertyName("profession")]
        public string Profession { get; set; } = string.Empty;
        [JsonPropertyName("authorization")]
        public Authorization Authorization { get; set; } = new Authorization();
        [JsonPropertyName("requisition_rights")]
        public List<RequisitionRight> RequisitionRights { get; set; } = new List<RequisitionRight>();
        [JsonPropertyName("specialities")]
        public List<string> Specialities { get; set; } = new List<string>();
    }

    public class Authorization
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }

    public class RequisitionRight
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}