using System.Text.Json.Serialization;
using System.Xml.Linq;
using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;

namespace CSPMerge.Commands;

[Command(Name = "update", Description = "update matching package versions to the latest version")]
public class Update: BaseCommand
{
    [Option(ShortName = "s", LongName = "source", Description = "the source csproj")]
    public string Source { get; set; }

    [Option(ShortName = "d", LongName = "destination", Description = "The destination csproj")]
    public string Destination { get; set; }

    [Option(LongName = "sync",ShortName = "c", Description = "if set, syncs versions in both files")]
    public bool Sync { get; set; }

    public Task OnExecute()
    {
        AnsiConsole.WriteLine($"sync is {Sync}");
        ArgumentNullException.ThrowIfNull(Source);
        ArgumentNullException.ThrowIfNull(Destination);

        var src = XElement.Load(Source);
        var dest = XElement.Load(Destination);

        var sf = Path.GetFileNameWithoutExtension(Source);
        var df = Path.GetFileNameWithoutExtension(Destination);
        
        var selem = getPackageRefs(src);
        var delem = getPackageRefs(dest);

        foreach (var s in selem)
        {
            var d = delem.FirstOrDefault(x => x.Attribute("Include")!.Value == s.Attribute("Include")!.Value);
            if (d == null) continue;

            var c = compareVersions(s, d);
            var inc = s.Attribute("Include")!.Value;
            if (c == Comparison.LessThan && Sync)
            {
                AnsiConsole.WriteLine($"syncing {inc} version in {sf} to {d.Attribute("Version")!.Value}");
                s.Attribute("Version")!.Value = d.Attribute("Version")!.Value;
                continue;
            }

            if (c == Comparison.LessThan)
            {
                AnsiConsole.WriteLine($"{inc} source {sf} {s.Attribute("Version")!.Value} is less than destination version {d.Attribute("Version")!.Value}, skipping");
                continue;
            }

            if (c == Comparison.GreaterThan)
            {
                AnsiConsole.WriteLine($"updating {inc} in {df} from {d.Attribute("Version")!.Value} to {s.Attribute("Version")!.Value}");
                d.Attribute("Version")!.Value = s.Attribute("Version")!.Value;
            }

            if (c == Comparison.Equal)
            {
                AnsiConsole.WriteLine($"{inc} have equal versions");
            }
            
        }
        
        AnsiConsole.WriteLine("saving source..");
        src.Save(Source,SaveOptions.None);
        AnsiConsole.WriteLine("saving destination..");
        dest.Save(Destination, SaveOptions.None);
        return Task.CompletedTask;
        
    }
}

public enum Comparison
{
    LessThan,
    GreaterThan,
    Equal
}