namespace AuthSecurityApi.Security
{
    public static class AgeRestriction
    {
        public static bool IsAtLeastSixteen(DateOnly dateOfBirth, DateOnly? today = null)
        {
            var currentDate = today ?? DateOnly.FromDateTime(DateTime.UtcNow);
            return dateOfBirth <= currentDate.AddYears(-16);
        }
    }
}
