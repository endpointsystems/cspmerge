using System.Xml.Linq;
using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;

namespace CSPMerge.Commands;

[Command(Name = "compare", Description = "compare two csproj files")]
public class Compare
{
    [Option(ShortName = "f", LongName = "first", Description = "the first csproj file")]
    public string First { get; set; }

    [Option(ShortName = "s", LongName = "second", Description = "the second csproj file")]
    public string Second { get; set; }

    public async Task OnExecute()
    {
        var fp = getPackageRefs(First);
        var sp = getPackageRefs(Second);

        var table = new Table();
        table.AddColumn("Name", config =>
        {
            config.Alignment = Justify.Left;
            config.NoWrap = false;
        });
        table.AddColumn("First", config =>
        {
            config.Alignment = Justify.Left;
            config.NoWrap = true;
        });
        table.AddColumn("Comparison");
        table.AddColumn("First", config =>
        {
            config.Alignment = Justify.Left;
            config.NoWrap = true;
        });

        foreach (var f in fp)
        {
            var s = sp.FirstOrDefault(x => x.Attribute("Include")!.Value == f.Attribute("Include")!.Value);
            if (s == null) continue;
        }
    }

    private List<XElement> getPackageRefs(string path)
    {
        var root = XElement.Load(path);
        var pgroups = from x in root.Elements()
            where x.Name == "ItemGroup"
            select x;
        var prefs = from x in pgroups.Elements()
            where x.Name == "PackageReference"
            select x;

        return prefs.ToList();
    }
}
