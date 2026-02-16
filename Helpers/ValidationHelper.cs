using System.ComponentModel.DataAnnotations;

namespace ExuberantPathfinders.Web.Helpers
{
    public static class ValidationHelper
    {
        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidAmount(decimal amount)
        {
            return amount > 0 && amount <= 999999999.99m;
        }

        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Remove common separators
            var cleaned = System.Text.RegularExpressions.Regex.Replace(phoneNumber, @"[\s\-\(\)\.]+", "");
            return cleaned.Length >= 7 && System.Text.RegularExpressions.Regex.IsMatch(cleaned, @"^\d+$");
        }

        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        public static decimal RoundToTwoDecimals(decimal value)
        {
            return decimal.Round(value, 2, MidpointRounding.AwayFromZero);
        }
    }
}
