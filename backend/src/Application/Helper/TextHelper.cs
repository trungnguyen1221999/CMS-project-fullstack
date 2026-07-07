using System.Text;

namespace Application.Helper
{
    public static class TextHelper
    {
        public static string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            // Convert to lower case
            text = text.ToLowerInvariant();
            // Remove invalid characters
            var sb = new StringBuilder();
            foreach (var c in text)
            {
                if (char.IsLetterOrDigit(c) || c == ' ')
                {
                    sb.Append(c);
                }
            }
            // Replace spaces with hyphens
            var slug = sb.ToString().Replace(' ', '-');
            // Remove consecutive hyphens
            while (slug.Contains("--"))
            {
                slug = slug.Replace("--", "-");
            }
            // Trim hyphens from the start and end
            slug = slug.Trim('-');
            return slug;
        }
    }
}
