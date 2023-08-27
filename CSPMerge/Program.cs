// See https://aka.ms/new-console-template for more information

using CSPMerge;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

var services = new ServiceCollection()
    .AddLogging(loggingBuilder => loggingBuilder.AddSimpleConsole(cfg =>
    {
        cfg.SingleLine = true;
        cfg.ColorBehavior = LoggerColorBehavior.Enabled;
    }));

var p = services.BuildServiceProvider();
var app = new CommandLineApplication<CSPMApp>();
app.Conventions
    .UseDefaultConventions()
    .UseConstructorInjection(p);
app.Execute(args);
