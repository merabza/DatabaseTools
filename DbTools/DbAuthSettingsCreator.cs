using DbTools.Errors;
using DbTools.Models;
using OneOf;
using System.Collections.Generic;
using SystemToolsShared;
using SystemToolsShared.Errors;

namespace DbTools;

public static class DbAuthSettingsCreator
{
    public static OneOf<DbAuthSettingsBase, IEnumerable<Err>> Create(bool windowsNtIntegratedSecurity,
        string? serverUser, string? serverPass, bool useConsole)
    {
        switch (windowsNtIntegratedSecurity)
        {
            case false when !string.IsNullOrWhiteSpace(serverUser) && !string.IsNullOrWhiteSpace(serverPass):
                return new DbAuthSettings(serverUser, serverPass);
            case true:
                {
                    if (!string.IsNullOrWhiteSpace(serverUser) || !string.IsNullOrWhiteSpace(serverPass))
                        StShared.WriteWarningLine(
                            "windowsNtIntegratedSecurity is on and serverUser is specified or serverPass is specified. both will be ignored.",
                            useConsole);
                    return new DbAuthSettingsBase();
                }
            default:
                StShared.WriteErrorLine(
                    "windowsNtIntegratedSecurity is off and serverUser does not specified or serverPass does not specified",
                    useConsole);
                return new[]
                {
                    DbToolsErrors
                        .WindowsNtIntegratedSecurityIsOffAndServerUserDoesNotSpecifiedOrServerPassDoesNotSpecified
                };
        }
    }
}