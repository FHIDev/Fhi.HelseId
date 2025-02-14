namespace Fhi.TestFramework.NHNTTT.Dtos
{
    internal class TokenResponse
    {
        public bool IsError { get; set; }

        public SuccessResponse SuccessResponse { get; set; } = new SuccessResponse(string.Empty, string.Empty);

        public ErrorResponse ErrorResponse { get; set; } = new ErrorResponse();
    }
}
