using Microsoft.Extensions.Options;
using TuxfamilyScraper.Library.Settings;

namespace TuxfamilyScraper.Library.Data;

public class AdminPassword : IAdminPassword
{
    private readonly string _adminPassword;
    
    public AdminPassword(IOptions<TuxfamilyDatabaseSettings> options)
    {
        _adminPassword = options.Value.AdminPassword;
    }
    
    public string GetPassword()
    {
        return _adminPassword;
    }
}