using GWInstekPSUManager.Core.Interfaces.ChannelInterfaces;
using Microsoft.Extensions.Logging;

namespace GWInstekPSUManager.Infrastructure.Services.ChannelServices;

public class ChannelFactory : IChannelFactory
{
    private readonly ILogger<ChannelFactory> _logger;

    public ChannelFactory(ILogger<ChannelFactory> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IPowerSupplyChannel CreateDefaultChannel()
    {
        var channel = new PowerSupplyChannel
        {
            VoltageLimit = 30.0,
            CurrentLimit = 5.0,
            Mode = "CC",
            StartTime = DateTime.Now
        };

        _logger.LogInformation($"Created new channel with defaults: Vmax={channel.VoltageLimit}V, Imax={channel.CurrentLimit}A");
        return channel;
    }

    public IPowerSupplyChannel CreateFromFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty", nameof(filePath));

        var channel = new PowerSupplyChannel();
        try
        {
            channel.LoadFromFileAsync(filePath).Wait();
            _logger.LogInformation($"Loaded channel from {filePath}");
            return channel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to load channel from {filePath}");
            throw;
        }
    }

    public async Task<IPowerSupplyChannel> CreateFromFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty", nameof(filePath));

        var channel = new PowerSupplyChannel();
        try
        {
            await channel.LoadFromFileAsync(filePath);
            _logger.LogInformation($"Loaded channel from {filePath}");
            return channel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to load channel from {filePath}");
            throw;
        }
    }

    public IPowerSupplyChannel CreateHighCurrentChannel(double maxCurrent)
    {
        if (maxCurrent <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxCurrent), "Current must be positive");

        var channel = new PowerSupplyChannel
        {
            VoltageLimit = 30.0,
            CurrentLimit = maxCurrent,
            Mode = "CC",
            StartTime = DateTime.Now
        };

        _logger.LogInformation($"Created high-current channel: Imax={maxCurrent}A");
        return channel;
    }

    public IPowerSupplyChannel CreateHighVoltageChannel(double maxVoltage)
    {
        if (maxVoltage <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxVoltage), "Voltage must be positive");

        var channel = new PowerSupplyChannel
        {
            VoltageLimit = maxVoltage,
            CurrentLimit = 5.0,
            Mode = "CV",
            StartTime = DateTime.Now
        };

        _logger.LogInformation($"Created high-voltage channel: Vmax={maxVoltage}V");
        return channel;
    }
}