namespace AuthSecurityApi.Domain.DTOs
{
    public class ForgotPasswordResponse
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = "Якщо обліковий запис із таким Email існує, інструкції з відновлення буде надіслано.";
        public string? Token { get; set; }
    }
}
