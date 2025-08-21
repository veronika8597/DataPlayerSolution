using Core.Interfaces;
using Microsoft.Extensions.Configuration;


namespace Infrastructure.Services;

public class XorConfigurationService : IXorConfigurationService
{
    private readonly byte _xorKey;

    public XorConfigurationService(IConfiguration configuration)
    {
       _xorKey = configuration.GetValue<byte>("XorKey");
    }
    public byte GetXorKey() => _xorKey;
}