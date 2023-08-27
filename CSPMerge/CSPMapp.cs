using System.Security.Cryptography;
using CSPMerge.Commands;
using McMaster.Extensions.CommandLineUtils;

namespace CSPMerge;

[Command(Name = "CSPMerge", Description = "merge PackageReference references and versions in csproj files")]
[Subcommand(typeof(List), typeof(Compare), typeof(Update))]
public class CSPMApp
{
    public void OnExecute(){}
}
