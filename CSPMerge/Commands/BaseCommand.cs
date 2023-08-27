using System.Runtime.Intrinsics.Arm;
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

    protected Comparison compareVersions(XElement e1, XElement e2)
    {
        var s1 = SemVersion.FromVersion(new Version(e1.Attribute("Version")!.Value));
        var s2 = SemVersion.FromVersion(new Version(e2.Attribute("Version")!.Value));
        
        if (s1.Major < s2.Major) return Comparison.LessThan;
        if (s1.Major > s2.Major) return Comparison.GreaterThan;
        if (s1.Major == s2.Major && s1.Minor < s2.Minor) return Comparison.LessThan;
        if (s1.Major == s2.Major && s1.Minor > s2.Minor) return Comparison.GreaterThan;
        if (s1.Major == s2.Major && s1.Minor == s2.Minor && s1.Patch < s2.Patch) return Comparison.LessThan;
        if (s1.Major == s2.Major && s1.Minor == s2.Minor && s1.Patch > s2.Patch) return Comparison.GreaterThan;
        return Comparison.Equal;
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


}