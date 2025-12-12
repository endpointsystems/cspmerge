using System.Xml.Linq;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Semver;
using Spectre.Console;

namespace CSPMerge.Commands;

[Command(Name = "compare", Description = "compare two csproj files")]
public class Compare: BaseCommand
{
    [Option(ShortName = "s", LongName = "source", Description = "the first csproj file")]
    public string Source { get; set; }

    [Option(ShortName = "d", LongName = "destination", Description = "the second csproj file")]
    public string Destination { get; set; }

    public Task OnExecute(IConsole console)
    {
        if (string.IsNullOrEmpty(Source) || string.IsNullOrEmpty(Destination))
        {
            console.WriteLine("source or destination missing - make sure to use -s and -d parameters");

            return Task.CompletedTask;

        }

        var root1 = XElement.Load(Source);
        var root2 = XElement.Load(Destination);

        var fp = getPackageRefs(root1);
        var sp = getPackageRefs(root2);

        var fp2 = new List<XElement>(fp);
        var sp2 = new List<XElement>(sp);


        var table = new Table();
        table.AddColumn("Name", config =>
        {
            config.Alignment = Justify.Left;
            config.NoWrap = false;
        });
        table.AddColumn(Path.GetFileNameWithoutExtension(Source)!, config =>
        {
            config.Alignment = Justify.Left;
            config.NoWrap = true;
        });
        table.AddColumn("Comparison");
        table.AddColumn(Path.GetFileNameWithoutExtension(Destination)!, config =>
        {
            config.Alignment = Justify.Left;
            config.NoWrap = true;
        });

        foreach (var f in fp)
        {
            if (!ValidatePackageReference(f, out string fError))
            {
                AnsiConsole.WriteLine($"Skipping invalid package reference: {fError}");
                continue;
            }

            var fInclude = GetAttributeValue(f, "Include");
            var s = sp.FirstOrDefault(x => x.Attribute("Include")?.Value == fInclude);
            if (s == null) continue;

            if (!ValidatePackageReference(s, out string sError))
            {
                AnsiConsole.WriteLine($"Skipping invalid package reference: {sError}");
                continue;
            }

            var lhs = GetAttributeValue(f, "Version");
            var rhs = GetAttributeValue(s, "Version");

            if (lhs.Contains(".*"))
            {
                AnsiConsole.WriteLine($"skipping {fInclude} as it contains a wildcard");
                continue;
            }

            if (rhs.Contains(".*"))
            {
                AnsiConsole.WriteLine($"skipping {fInclude} as it contains a wildcard");
                continue;
            }

            string c = string.Empty;
            var parsed = SemVersion.TryParse(lhs, SemVersionStyles.Any, out SemVersion parsedLhs);
            var parsed2 = SemVersion.TryParse(rhs, SemVersionStyles.Any, out SemVersion parsedRhs);

            if (SemVersion.TryParse(lhs, SemVersionStyles.Any, out SemVersion s1) &&
                SemVersion.TryParse(rhs, SemVersionStyles.Any, out SemVersion s2))
            {
                c = compareVersions(s1, s2);
            }
            else
            {
                var v1 = Version.Parse(lhs);
                var v2 = Version.Parse(rhs);
                c = compareVersions(v1, v2);
            }

            // var s1 = SemVersion.FromVersion(new Version(lhs));
            // var s2 = SemVersion.FromVersion(new Version(rhs));
            // var c = compareVersions(s1, s2);

            table.AddRow(fInclude, lhs, c, rhs);
            sp2.RemoveAt(sp2.IndexOf(s));
            fp2.RemoveAt(fp2.IndexOf(f));
        }

        foreach (var f in fp2)
        {
            if (ValidatePackageReference(f, out _))
            {
                table.AddRow(GetAttributeValue(f, "Include"), GetAttributeValue(f, "Version"));
            }
        }

        foreach (var s in sp2)
        {
            if (ValidatePackageReference(s, out _))
            {
                table.AddRow(GetAttributeValue(s, "Include"), String.Empty, String.Empty, GetAttributeValue(s, "Version"));
            }
        }

        AnsiConsole.Write(table);

        return Task.CompletedTask;
    }


}
