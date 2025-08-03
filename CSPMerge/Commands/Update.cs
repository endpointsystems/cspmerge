using System.Xml.Linq;
using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace CSPMerge.Commands;

[Command(Name = "update", Description = "update matching package versions to the latest version")]
public class Update: BaseCommand
{
    [Option(ShortName = "s", LongName = "source", Description = "the source csproj")]
    public string Source { get; set; }

    [Option(ShortName = "d", LongName = "destination", Description = "The destination csproj")]
    public string Destination { get; set; }

    [Option(LongName = "sync",ShortName = "c", Description = "if set, syncs versions from source to destination")]
    public bool Sync { get; set; }

    // ReSharper disable once CognitiveComplexity
    public Task OnExecute(IConsole console)
    {
        AnsiConsole.WriteLine($"sync is {Sync}");
        if (string.IsNullOrEmpty(Source) || string.IsNullOrEmpty(Destination))
        {
            console.WriteLine("source or destination parameters missing - make sure to use the -s and -d options");
            return Task.CompletedTask;
        }

        var src = XElement.Load(Source);
        var dest = XElement.Load(Destination);

        var sf = Path.GetFileNameWithoutExtension(Source);
        var df = Path.GetFileNameWithoutExtension(Destination);

        var selem = getPackageRefs(src);
        var delem = getPackageRefs(dest);

        foreach (var s in selem)
        {
            var d = delem.FirstOrDefault(x => x.Attribute("Include")!.Value == s.Attribute("Include")!.Value);
            if (d == null)
            {
                if (!Sync)
                {
                    console.WriteLine(
                        $"{s.Attribute("Include")!.Value} does not exist, sync is disabled, skipping");
                    continue;
                }

                console.WriteLine($"adding missing package {s.Attribute("Include")!.Value} to destination");
                var pref = (from x in dest.Descendants()
                    where x.Name == "PackageReference"
                    select x).First().Parent;

                pref!.Add(s);
                //delem.Add(s);
                continue;
            }

            var c = compareVersions(s, d);
            var inc = s.Attribute("Include")!.Value;
            if (c == Comparison.LessThan && Sync)
            {
                AnsiConsole.WriteLine($"syncing {inc} version in {sf} to {d.Attribute("Version")!.Value}");
                s.Attribute("Version")!.SetValue(d.Attribute("Version")!.Value);
                //s.Attribute("Version")!.Value = d.Attribute("Version")!.Value;
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
        AnsiConsole.WriteLine($"Completed at {DateTime.Now:t}");
        return Task.CompletedTask;

    }
}

public enum Comparison
{
    LessThan,
    GreaterThan,
    Equal,
    NotExist
}
