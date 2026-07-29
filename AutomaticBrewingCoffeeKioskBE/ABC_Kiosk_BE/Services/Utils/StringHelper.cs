

namespace Services.Utils
{
    public static class StringHelper
    {
        public static string ToCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input) || char.IsLower(input[0]))
                return input;

            return char.ToLowerInvariant(input[0]) + input.Substring(1);
        }

        public static string GetCouchDbDatabaseNameFormat(this string str)
        {
            return $"{str.ToLower()}s";
        }

        //return http://sa:12345@127.0.0.1:5984/
        public static string BuildCouchDbUrl(string baseUrl, string username, string pwd)
        {
            var uriBuilder = new UriBuilder(baseUrl)
            {
                UserName = username,
                Password = pwd
            };

            return $"{uriBuilder.Uri}";
        }
    } 

}
