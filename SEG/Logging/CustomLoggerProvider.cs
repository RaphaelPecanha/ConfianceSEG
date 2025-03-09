using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;

namespace SEG.Logging;

public class CustomLoggerProvider : ILoggerProvider
{
    readonly CustomLoggerProviderConfiguration _configuration;
    readonly ConcurrentDictionary<string, CustomerLogger> loggers = new ConcurrentDictionary<string, CustomerLogger>();
    private readonly IConfiguration _configurationSettings;

    public CustomLoggerProvider(CustomLoggerProviderConfiguration configuration, IConfiguration configurationSettings)
    {
        _configuration = configuration;
        _configurationSettings = configurationSettings;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return loggers.GetOrAdd(categoryName, name => new CustomerLogger(name, _configuration, _configurationSettings));
    }

    public void Dispose()
    {
        loggers.Clear();
    }
}
