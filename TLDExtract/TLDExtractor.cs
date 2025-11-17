using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace TLDExtract
{
    public static class TLDExtractor
    {
        /// <summary>
        /// Gets or sets the number of days before a suffix file is renewed.
        /// is less then 0 then renew ever. 
        /// </summary>
        public static int RenewSuffixFileDays { get; set; } = 30;

        /// <summary>
        /// Gets or sets the file path used to store suffix data in a temporary location.
        /// </summary>
        /// <remarks>The default value is a path to a file named "suffix_file.json" in the system's
        /// temporary directory. The property can be set to a different path if a custom location is required.</remarks>
        public static string SuffixFilePath { get; set; }
            = Path.Combine(Path.GetTempPath(), "suffix_file.json");

        /// <summary>
        /// Gets the collection of domain suffixes and their associated types.
        /// </summary>
        /// <remarks>The dictionary maps domain suffix strings to their corresponding <see
        /// cref="DomainSuffixType"/> values. This property provides access to all recognized domain suffixes used for
        /// classification or validation purposes. The returned dictionary is read-only and should not be modified by
        /// callers.</remarks>
        public static Dictionary<string, DomainSuffixType> Suffixes
        { get => SetSuffixesProperty(ref field); } = null!;

        /// <summary>
        /// Parses the specified URL or domain name and extracts its subdomain, domain, and suffix components.
        /// </summary>
        /// <remarks>If a full URL is provided, only the host portion is used for extraction. The method
        /// enforces domain and label length constraints as specified by RFC 1035.</remarks>
        /// <param name="url">The URL or domain name to extract components from. Must be a valid absolute URL or a domain name. Cannot be
        /// null or empty.</param>
        /// <returns>An ExtractResult containing the subdomain, domain, and suffix parts of the input. The properties will be
        /// empty if the corresponding component is not present.</returns>
        /// <exception cref="TLDExtractException">Thrown when the input domain name or any of its labels is invalid, such as exceeding length limits or being
        /// empty.</exception>
        /// <exception cref="NotImplementedException">Thrown if an unsupported state is encountered during extraction.</exception>
        public static ExtractResult Extract(string url)
            => ExtractResultA(url).res;


        /// <summary>
        /// Extracts structured information from the specified URL.
        /// </summary>
        /// <param name="url">The URL to analyze and extract information from. Cannot be null.</param>
        /// <returns>An <see cref="ExtractResult"/> containing the extracted data from the provided URL.</returns>
        public static ExtractResult Extract(Uri url) => Extract(url.ToString());

        public static bool TryExtract(string url, out ExtractResult? result)
        {
            var x = ExtractResultA(url, false);
            result = x.isUrl ? x.res : null;
            return x.isUrl;
        }

        public static bool TryExtract(Uri url,out ExtractResult? result)
            => TryExtract(url.ToString(), out result);


        private enum State
        {
            suffix,
            subdomain
        }

        private static (ExtractResult res, bool isUrl) ExtractResultA(string url, bool throwing = true)
        {
            var result = new ExtractResult();

            // Normalize URL → extract host if possible
            if (Uri.TryCreate(url, UriKind.Absolute, out var clean))
                url = clean.Host;

            var hostName = url.ToLowerInvariant().Trim();
            var sections = hostName.Split('.').Reverse();

            // Short Methode
            (ExtractResult, bool) Fail(string msg)
                => throwing ? throw new TLDExtractException(msg) : (result, false);

            if (hostName.Length > 255)
                return Fail("Domain name length cannot be longer than 255 characters");

            string newSuffix = "";
            State state = State.suffix;

            foreach (var part in sections)
            {
                if (string.IsNullOrEmpty(part))
                    return Fail("Domain label cannot be null or empty");

                if (part.Length > 63)
                    return Fail("Domain label length cannot exceed 63 characters (RFC1035)");

                switch (state)
                {
                    case State.suffix:
                        newSuffix = (part + "." + result.Suffix).Trim('.');
                        if (Suffixes.TryGetValue(newSuffix, out var type))
                        {
                            result.Suffix = newSuffix;
                            result.SuffixType = type;
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(result.Suffix))
                                return Fail("Domain suffix cannot be empty");

                            result.Domain = part;
                            state = State.subdomain;
                        }
                        break;

                    case State.subdomain:
                        result.SubDomain = part + "." + result.SubDomain;
                        break;

                    default:
                        return Fail("Unhandled parsing state");
                }
            }

            result.SubDomain = result.SubDomain.TrimEnd('.');
            return (result, true);
        }


        private static Dictionary<string, DomainSuffixType> SetSuffixesProperty(ref Dictionary<string, DomainSuffixType> field)
        {
            if (field is null)
            {

                if (!File.Exists(SuffixFilePath)
                    || new FileInfo(SuffixFilePath).CreationTimeUtc.AddDays(RenewSuffixFileDays) <= DateTime.UtcNow
                    || RenewSuffixFileDays <= 0)
                {
                    field = Task.Run(() => LoadSuffixDatFromWebAsync())
                          .GetAwaiter()
                          .GetResult()
                          .CreateSuffixJsonFile();
                    return field;
                }

                field = File.ReadAllText(SuffixFilePath).LoadFromSuffixJsonFile();
            }

            return field;
        }

        private static Dictionary<string, DomainSuffixType> LoadFromSuffixJsonFile(this string s)
            => JsonSerializer.Deserialize<Dictionary<string, DomainSuffixType>>(s)
            ?? throw new TLDExtractException("load suffix file faild");

        private static Dictionary<string, DomainSuffixType> CreateSuffixJsonFile(this string s)
        {
            var jobj = s.ClrPattern();
            var jstring = JsonSerializer.Serialize(jobj);
            File.WriteAllText(SuffixFilePath, jstring);
            return jobj;
        }

        private static Dictionary<string, DomainSuffixType> ClrPattern(this string str)
        {

            var dict = new Dictionary<string, DomainSuffixType>();
            DomainSuffixType currentType = DomainSuffixType.ICANN; // Start im ICANN-Abschnitt

            foreach (var line in str.ReplaceLineEndings().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmedLine = line.Trim();

                if (string.IsNullOrWhiteSpace(trimmedLine)) continue; // Leere Zeilen überspringen
                if (trimmedLine.StartsWith("//")) // Kommentare
                {
                    if (trimmedLine.Contains("===BEGIN PRIVATE DOMAINS==="))
                        currentType = DomainSuffixType.Private;
                    else if (trimmedLine.Contains("===END PRIVATE DOMAINS==="))
                        currentType = DomainSuffixType.ICANN;
                    continue;
                }

                // Eintrag hinzufügen
                dict[trimmedLine] = currentType;
            }
            return dict;
        }

        private static async Task<string> LoadSuffixDatFromWebAsync()
        {
            try
            {
                using HttpClient client = new HttpClient();
                var result = await client.GetAsync("https://www.publicsuffix.org/list/public_suffix_list.dat");
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new TLDExtractException(ex.Message);
            }
        }

    }
}
