using System.Text.RegularExpressions;
using System.Text;

namespace SoleKingECommerce.Helpers
{
    public static class SlugHelper
    {
        private static readonly Dictionary<char, char> VietnameseMap = new Dictionary<char, char>
        {
            ['á'] = 'a',
            ['à'] = 'a',
            ['ả'] = 'a',
            ['ã'] = 'a',
            ['ạ'] = 'a',
            ['â'] = 'a',
            ['ấ'] = 'a',
            ['ầ'] = 'a',
            ['ẫ'] = 'a',
            ['ậ'] = 'a',
            ['ă'] = 'a',
            ['ắ'] = 'a',
            ['ằ'] = 'a',
            ['ẳ'] = 'a',
            ['ẵ'] = 'a',
            ['ặ'] = 'a',
            ['é'] = 'e',
            ['è'] = 'e',
            ['ẻ'] = 'e',
            ['ẽ'] = 'e',
            ['ẹ'] = 'e',
            ['ê'] = 'e',
            ['ế'] = 'e',
            ['ề'] = 'e',
            ['ể'] = 'e',
            ['ễ'] = 'e',
            ['ệ'] = 'e',
            ['í'] = 'i',
            ['ì'] = 'i',
            ['ỉ'] = 'i',
            ['ĩ'] = 'i',
            ['ị'] = 'i',
            ['ó'] = 'o',
            ['ò'] = 'o',
            ['ỏ'] = 'o',
            ['õ'] = 'o',
            ['ọ'] = 'o',
            ['ô'] = 'o',
            ['ố'] = 'o',
            ['ồ'] = 'o',
            ['ổ'] = 'o',
            ['ỗ'] = 'o',
            ['ộ'] = 'o',
            ['ơ'] = 'o',
            ['ớ'] = 'o',
            ['ờ'] = 'o',
            ['ở'] = 'o',
            ['ỡ'] = 'o',
            ['ợ'] = 'o',
            ['ú'] = 'u',
            ['ù'] = 'u',
            ['ủ'] = 'u',
            ['ũ'] = 'u',
            ['ụ'] = 'u',
            ['ư'] = 'u',
            ['ứ'] = 'u',
            ['ừ'] = 'u',
            ['ử'] = 'u',
            ['ữ'] = 'u',
            ['ự'] = 'u',
            ['ý'] = 'y',
            ['ỳ'] = 'y',
            ['ỷ'] = 'y',
            ['ỹ'] = 'y',
            ['ỵ'] = 'y',
            ['đ'] = 'd'
        };

        public static string GenerateSlug(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var ch in input.ToLower())
            {
                if (VietnameseMap.TryGetValue(ch, out var replacement))
                {
                    sb.Append(replacement);
                }
                else if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(ch);
                }
                else if (char.IsWhiteSpace(ch))
                {
                    sb.Append('-');
                }
            }

            var result = sb.ToString();

            result = Regex.Replace(result, "-{2,}", "-").Trim('-');

            return result;
        }
    }
}
