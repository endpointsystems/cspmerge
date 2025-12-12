using System.Xml.Linq;
using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;

namespace CSPMerge.Commands;

[Command(Name = "list", Description = "list the PackageRefs for a given csproj")]
public class List: BaseCommand
{
    [Option(ShortName = "s", LongName = "source", Description = "the path and file name of the csproj file")]
    public string Source { get; set; }

    public async Task OnExecute()
    {
        var fs = File.OpenRead(Source);
        var root = await XElement.LoadAsync(fs,LoadOptions.None,new CancellationToken());
        var pgroups = from x in root.Elements()
            where x.Name == "ItemGroup"
            select x;
        var prefs = from x in pgroups.Elements()
            where x.Name == "PackageReference"
            select x;

        var table = new Table();
        table.AddColumn("Name", config =>
        {
            config.Alignment = Justify.Left;
            config.NoWrap = false;
        });
        table.AddColumn("Version", config =>
        {
            config.Alignment = Justify.Left;
            config.NoWrap = false;
        });
        foreach (var element in prefs)
        {
            if (ValidatePackageReference(element, out string error))
            {
                table.AddRow(GetAttributeValue(element, "Include"), GetAttributeValue(element, "Version"));
            }
            else
            {
                AnsiConsole.WriteLine($"Skipping invalid package reference: {error}");
            }
        }

        AnsiConsole.Write(table);
    }
}
