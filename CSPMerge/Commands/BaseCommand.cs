using System.Xml.Linq;
using Semver;

namespace CSPMerge.Commands;

public abstract class BaseCommand
{
    protected string compareVersions(SemVersion s1, SemVersion s2)
    {
        if (s1.Major < s2.Major) return "<";
        if (s1.Major > s2.Major) return ">";
        if (s1.Major == s2.Major && s1.Minor < s2.Minor) return "<";
        if (s1.Major == s2.Major && s1.Minor > s2.Minor) return ">";
        if (s1.Major == s2.Major && s1.Minor == s2.Minor && s1.Patch < s2.Patch) return "<";
        if (s1.Major == s2.Major && s1.Minor == s2.Minor && s1.Patch > s2.Patch) return ">";
        return "==";
    }

    protected string compareVersions(Version v1, Version v2)
    {
        if (v1 < v2) return "<";
        if (v1 > v2) return ">";
        return "==";
    }

    protected Comparison compare(SemVersion s1, SemVersion s2)
    {
        if (s1.Major < s2.Major) return Comparison.LessThan;
        if (s1.Major > s2.Major) return Comparison.GreaterThan;
        if (s1.Major == s2.Major && s1.Minor < s2.Minor) return Comparison.LessThan;
        if (s1.Major == s2.Major && s1.Minor > s2.Minor) return Comparison.GreaterThan;
        if (s1.Major == s2.Major && s1.Minor == s2.Minor && s1.Patch < s2.Patch) return Comparison.LessThan;
        if (s1.Major == s2.Major && s1.Minor == s2.Minor && s1.Patch > s2.Patch) return Comparison.GreaterThan;
        return Comparison.Equal;
    }
    protected Comparison compare(SemVersionRange sm1, SemVersionRange sm2)
    {
        var s1 = sm1[0].Start;
        var s2 = sm2[0].Start;
        if (s1.Major < s2.Major) return Comparison.LessThan;
        if (s1.Major > s2.Major) return Comparison.GreaterThan;
        if (s1.Major == s2.Major && s1.Minor < s2.Minor) return Comparison.LessThan;
        if (s1.Major == s2.Major && s1.Minor > s2.Minor) return Comparison.GreaterThan;
        if (s1.Major == s2.Major && s1.Minor == s2.Minor && s1.Patch < s2.Patch) return Comparison.LessThan;
        if (s1.Major == s2.Major && s1.Minor == s2.Minor && s1.Patch > s2.Patch) return Comparison.GreaterThan;
        return Comparison.Equal;
    }

    protected Comparison compare(Version v1, Version v2)
    {
        if (v1 < v2) return Comparison.LessThan;
        if (v1 > v2) return Comparison.GreaterThan;
        return Comparison.Equal;
    }

    protected Comparison compareVersions(XElement e1, XElement e2)
    {
        var x1 = GetAttributeValue(e1, "Version");
        var x2 = GetAttributeValue(e2, "Version");

        // Boxed versions (e.g., [14.0.0]) should always be treated as "less than"
        // to ensure they get copied/overwritten during merge
        if (x1.Contains("["))
            return Comparison.LessThan;

        if (x2.Contains("["))
            return Comparison.GreaterThan;

        if (x1.Contains(".*") || x2.Contains(".*"))
        {
            var sm1 = SemVersionRange.Parse(x1,SemVersionRangeOptions.Loose);
            var sm2 = SemVersionRange.Parse(x2, SemVersionRangeOptions.Loose);
            return compare(sm1, sm2);
        }

        if (SemVersion.TryParse(x1, SemVersionStyles.Any, out SemVersion s1) &&
            SemVersion.TryParse(x2, SemVersionStyles.Any, out SemVersion s2))
            return compare(s1, s2);

        var v1 = Version.Parse(x1);
        var v2 = Version.Parse(x2);
        return compare(v1, v2);
    }

    protected List<XElement> getPackageRefs(XElement root)
    {
        var pgroups = from x in root.Elements()
            where x.Name == "ItemGroup"
            select x;
        var prefs = from x in pgroups.Elements()
            where x.Name == "PackageReference"
            select x;

        return prefs.ToList();
    }

    protected bool ValidatePackageReference(XElement element, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (element.Attribute("Include") == null)
        {
            errorMessage = "PackageReference missing 'Include' attribute";
            return false;
        }

        if (element.Attribute("Version") == null)
        {
            errorMessage = $"PackageReference '{element.Attribute("Include")?.Value}' missing 'Version' attribute";
            return false;
        }

        return true;
    }

    protected string GetAttributeValue(XElement element, string attributeName)
    {
        var attr = element.Attribute(attributeName);
        if (attr == null)
        {
            throw new InvalidOperationException($"Element is missing required attribute '{attributeName}'");
        }
        return attr.Value;
    }


}
