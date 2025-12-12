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
            if (!ValidatePackageReference(s, out string sError))
            {
                console.WriteLine($"Skipping invalid package reference in source: {sError}");
                continue;
            }

            var sInclude = GetAttributeValue(s, "Include");
            var d = delem.FirstOrDefault(x => x.Attribute("Include")?.Value == sInclude);
            if (d == null)
            {
                if (!Sync)
                {
                    console.WriteLine(
                        $"{sInclude} does not exist, sync is disabled, skipping");
                    continue;
                }

                console.WriteLine($"adding missing package {sInclude} to destination");
                var existingPackageRef = (from x in dest.Descendants()
                    where x.Name == "PackageReference"
                    select x).FirstOrDefault();

                if (existingPackageRef?.Parent != null)
                {
                    existingPackageRef.Parent.Add(s);
                }
                else
                {
                    // No existing PackageReference found, create an ItemGroup
                    var itemGroup = new XElement("ItemGroup");
                    itemGroup.Add(s);
                    dest.Add(itemGroup);
                }
                continue;
            }

            if (!ValidatePackageReference(d, out string dError))
            {
                console.WriteLine($"Skipping invalid package reference in destination: {dError}");
                continue;
            }

            var c = compareVersions(s, d);
            var sVersion = GetAttributeValue(s, "Version");
            var dVersion = GetAttributeValue(d, "Version");

            if (c == Comparison.LessThan && Sync)
            {
                AnsiConsole.WriteLine($"syncing {sInclude} version in {sf} to {dVersion}");
                s.Attribute("Version")!.SetValue(dVersion);
                continue;
            }

            if (c == Comparison.LessThan)
            {
                AnsiConsole.WriteLine($"{sInclude} source {sf} {sVersion} is less than destination version {dVersion}, skipping");
                continue;
            }

            if (c == Comparison.GreaterThan)
            {
                AnsiConsole.WriteLine($"updating {sInclude} in {df} from {dVersion} to {sVersion}");
                d.Attribute("Version")!.Value = sVersion;
            }

            if (c == Comparison.Equal)
            {
                AnsiConsole.WriteLine($"{sInclude} have equal versions");
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
