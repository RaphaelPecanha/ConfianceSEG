using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace SEG.Logging;

public class CustomerLogger : ILogger
{
    readonly string loggerName;
    readonly CustomLoggerProviderConfiguration loggerConfig;
    private readonly string logDirectory;

    public CustomerLogger(string loggerName, CustomLoggerProviderConfiguration loggerConfig, IConfiguration configuration)
    {
        this.loggerName = loggerName;
        this.loggerConfig = loggerConfig;

        // Obtém o diretório de logs do appsettings.json
        logDirectory = configuration.GetValue<string>("Logging:LogDirectory") ?? "C:\\WebAPI\\Logging";

        // Garante que o diretório exista
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel == loggerConfig.LogLevel;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        string mensagem = $"{logLevel.ToString()}: {eventId} - {formatter(state, exception)}";
        EscreverTextoNoArquivo(mensagem);
    }

    private void EscreverTextoNoArquivo(string mensagem)
    {
        string caminhoArquivoLog = Path.Combine(logDirectory, $"log_{DateTime.Now:yyyy-MM-dd}.txt");

        try
        {
            using StreamWriter streamWrite = new StreamWriter(caminhoArquivoLog, true);
            streamWrite.WriteLine(mensagem);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
