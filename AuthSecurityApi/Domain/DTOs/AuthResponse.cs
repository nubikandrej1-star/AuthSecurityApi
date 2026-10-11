namespace AuthSecurityApi.Domain.DTOs
{
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public IEnumerable<string>? Errors { get; set; }
        public string? ErrorCode { get; set; }

        public static AuthResponse Ok(int userId, string? email, string fullName, IList<string> roles, string message = "Успішно") =>
            new() { Success = true, Message = message, UserId = userId, Email = email, FullName = fullName, Roles = roles };

        public static AuthResponse Fail(string message, IEnumerable<string>? errors = null, string? errorCode = null) =>
            new() { Success = false, Message = message, Errors = errors, ErrorCode = errorCode };
    }
}
