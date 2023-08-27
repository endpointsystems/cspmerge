using System.Net;
using System.Xml.Linq;
using McMaster.Extensions.CommandLineUtils;
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

    public Task OnExecute()
    {
        ArgumentNullException.ThrowIfNull(Source);
        ArgumentNullException.ThrowIfNull(Destination);

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
            var s = sp.FirstOrDefault(x => x.Attribute("Include")!.Value == f.Attribute("Include")!.Value);
            if (s == null) continue;
            var lhs = f.Attribute("Version")!.Value;
            var rhs = s.Attribute("Version")!.Value;
            var s1 = SemVersion.FromVersion(new Version(lhs));
            var s2 = SemVersion.FromVersion(new Version(rhs));
            var c = compareVersions(s1, s2);

            table.AddRow(f.Attribute("Include")!.Value,f.Attribute("Version")!.Value, c, s.Attribute("Version")!.Value);
            sp2.RemoveAt(sp2.IndexOf(s));
            fp2.RemoveAt(fp2.IndexOf(f));
        }

        foreach (var f in fp2)
        {
            table.AddRow(f.Attribute("Include")!.Value, f.Attribute("Version")!.Value);
        }

        foreach (var s in sp2)
        {
            table.AddRow(s.Attribute("Include")!.Value, String.Empty, String.Empty, s.Attribute("Version")!.Value);
        }

        AnsiConsole.Write(table);

        return Task.CompletedTask;
    }


}
