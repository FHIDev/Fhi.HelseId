namespace Fhi.TestFramework.NHNTTT.Dtos
{
    public record TillitsrammeverkClaimsParameters
    {
        public string? PractitionerAuthorizationCode { get; set; }

        public string? PractitionerAuthorizationText { get; set; }

        public string? PractitionerLegalEntityId { get; set; }

        public string? PractitionerLegalEntityName { get; set; }

        public string? PractitionerPointOfCareId { get; set; }

        public string? PractitionerPointOfCareName { get; set; }

        public string? PractitionerDepartmentId { get; set; }

        public string? PractitionerDepartmentName { get; set; }

        public string? CareRelationshipHealthcareServiceCode { get; set; }

        public string? CareRelationshipHealthcareServiceText { get; set; }

        public string? CareRelationshipPurposeOfUseCode { get; set; }

        public string? CareRelationshipPurposeOfUseText { get; set; }

        public string? CareRelationshipPurposeOfUseDetailsCode { get; set; }

        public string? CareRelationshipPurposeOfUseDetailsText { get; set; }

        public string? CareRelationshipTracingRefId { get; set; }

        public string? PatientsPointOfCareId { get; set; }

        public string? PatientsPointOfCareName { get; set; }

        public string? PatientsDepartmentId { get; set; }

        public string? PatientsDepartmentName { get; set; }
    }
}
