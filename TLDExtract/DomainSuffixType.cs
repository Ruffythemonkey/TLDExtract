namespace TLDExtract;


public class ExtractResult
{
    public string SubDomain { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string Suffix { get; set; } = string.Empty;
    public DomainSuffixType SuffixType { get; set; }
    public string EffectiveDomain
    {
        get => Domain + "." + Suffix;
    }
    public override string ToString()
    {
        return string.Format(
            "ExtractResult(subdomain='{0}', domain='{1}', suffix='{2}', suffix type='{3}')",
            SubDomain, Domain, Suffix, SuffixType);
    }
}

public enum DomainSuffixType
{
    ICANN,
    Private
}
