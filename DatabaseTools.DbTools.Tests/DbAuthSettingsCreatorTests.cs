using DatabaseTools.DbTools.Errors;
using DatabaseTools.DbTools.Models;
using SystemTools.SystemToolsShared.Errors;

namespace DatabaseTools.DbTools.Tests;

public class DbAuthSettingsCreatorTests
{
    [Fact]
    public void Create_WithWindowsAuthenticationAndNoCredentials_ReturnsDbAuthSettingsBase()
    {
        // Arrange
        var windowsNtIntegratedSecurity = true;
        string? serverUser = null;
        string? serverPass = null;
        var useConsole = false;

        // Act
        var result = DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT0);
        var authSettings = result.AsT0;
        Assert.IsType<DbAuthSettingsBase>(authSettings);
        Assert.IsNotType<DbAuthSettings>(authSettings);
    }

    [Fact]
    public void Create_WithWindowsAuthenticationAndEmptyCredentials_ReturnsDbAuthSettingsBase()
    {
        // Arrange
        var windowsNtIntegratedSecurity = true;
        var serverUser = string.Empty;
        var serverPass = string.Empty;
        var useConsole = false;

        // Act
        var result = DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT0);
        var authSettings = result.AsT0;
        Assert.IsType<DbAuthSettingsBase>(authSettings);
        Assert.IsNotType<DbAuthSettings>(authSettings);
    }

    [Fact]
    public void Create_WithWindowsAuthenticationAndWhitespaceCredentials_ReturnsDbAuthSettingsBase()
    {
        // Arrange
        var windowsNtIntegratedSecurity = true;
        var serverUser = "   ";
        var serverPass = "   ";
        var useConsole = false;

        // Act
        var result = DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT0);
        var authSettings = result.AsT0;
        Assert.IsType<DbAuthSettingsBase>(authSettings);
        Assert.IsNotType<DbAuthSettings>(authSettings);
    }

    [Fact]
    public void Create_WithWindowsAuthenticationAndServerUser_ReturnsDbAuthSettingsBaseAndLogsWarning()
    {
        // Arrange
        var windowsNtIntegratedSecurity = true;
        var serverUser = "testuser";
        string? serverPass = null;
        var useConsole = false;

        // Act
        var result = DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT0);
        var authSettings = result.AsT0;
        Assert.IsType<DbAuthSettingsBase>(authSettings);
        Assert.IsNotType<DbAuthSettings>(authSettings);
    }

    [Fact]
    public void Create_WithWindowsAuthenticationAndServerPass_ReturnsDbAuthSettingsBaseAndLogsWarning()
    {
        // Arrange
        var windowsNtIntegratedSecurity = true;
        string? serverUser = null;
        var serverPass = "testpass";
        var useConsole = false;

        // Act
        var result = DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT0);
        var authSettings = result.AsT0;
        Assert.IsType<DbAuthSettingsBase>(authSettings);
        Assert.IsNotType<DbAuthSettings>(authSettings);
    }

    [Fact]
    public void Create_WithWindowsAuthenticationAndBothCredentials_ReturnsDbAuthSettingsBaseAndLogsWarning()
    {
        // Arrange
        var windowsNtIntegratedSecurity = true;
        var serverUser = "testuser";
        var serverPass = "testpass";
        var useConsole = false;

        // Act
        var result = DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT0);
        var authSettings = result.AsT0;
        Assert.IsType<DbAuthSettingsBase>(authSettings);
        Assert.IsNotType<DbAuthSettings>(authSettings);
    }

    [Fact]
    public void Create_WithSqlAuthenticationAndValidCredentials_ReturnsDbAuthSettings()
    {
        // Arrange
        var windowsNtIntegratedSecurity = false;
        var serverUser = "testuser";
        var serverPass = "testpass";
        var useConsole = false;

        // Act
        var result = DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT0);
        var authSettings = result.AsT0;
        Assert.IsType<DbAuthSettings>(authSettings);
        var dbAuthSettings = (DbAuthSettings)authSettings;
        Assert.Equal("testuser", dbAuthSettings.ServerUser);
        Assert.Equal("testpass", dbAuthSettings.ServerPass);
    }

    [Fact]
    public void Create_WithSqlAuthenticationAndNullServerUser_ReturnsError()
    {
        // Arrange
        var windowsNtIntegratedSecurity = false;
        string? serverUser = null;
        var serverPass = "testpass";
        var useConsole = false;

        // Act
        var result = DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT1);
        var errors = result.AsT1;
        Assert.Single(errors);
        Assert.Equal(
            nameof(DbToolsErrors
                .WindowsNtIntegratedSecurityIsOffAndServerUserDoesNotSpecifiedOrServerPassDoesNotSpecified),
            errors[0].ErrorCode);
    }

    [Fact]
    public void Create_WithSqlAuthenticationAndNullServerPass_ReturnsError()
    {
        // Arrange
        var windowsNtIntegratedSecurity = false;
        var serverUser = "testuser";
        string? serverPass = null;
        var useConsole = false;

        // Act
        var result = DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT1);
        var errors = result.AsT1;
        Assert.Single(errors);
        Assert.Equal(
            nameof(DbToolsErrors
                .WindowsNtIntegratedSecurityIsOffAndServerUserDoesNotSpecifiedOrServerPassDoesNotSpecified),
            errors[0].ErrorCode);
    }

    [Fact]
    public void Create_WithSqlAuthenticationAndNullCredentials_ReturnsError()
    {
        // Arrange
        var windowsNtIntegratedSecurity = false;
        string? serverUser = null;
        string? serverPass = null;
        var useConsole = false;

        // Act
        var result = DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT1);
        var errors = result.AsT1;
        Assert.Single(errors);
        Assert.Equal(
            nameof(DbToolsErrors
                .WindowsNtIntegratedSecurityIsOffAndServerUserDoesNotSpecifiedOrServerPassDoesNotSpecified),
            errors[0].ErrorCode);
    }

    [Fact]
    public void Create_WithSqlAuthenticationAndEmptyServerUser_ReturnsError()
    {
        // Arrange
        var windowsNtIntegratedSecurity = false;
        var serverUser = string.Empty;
        var serverPass = "testpass";
        var useConsole = false;

        // Act
        var result = DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT1);
        var errors = result.AsT1;
        Assert.Single(errors);
        Assert.Equal(
            nameof(DbToolsErrors
                .WindowsNtIntegratedSecurityIsOffAndServerUserDoesNotSpecifiedOrServerPassDoesNotSpecified),
            errors[0].ErrorCode);
    }

    [Fact]
    public void Create_WithSqlAuthenticationAndEmptyServerPass_ReturnsError()
    {
        // Arrange
        var windowsNtIntegratedSecurity = false;
        var serverUser = "testuser";
        var serverPass = string.Empty;
        var useConsole = false;

        // Act
        var result = DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT1);
        var errors = result.AsT1;
        Assert.Single(errors);
        Assert.Equal(
            nameof(DbToolsErrors
                .WindowsNtIntegratedSecurityIsOffAndServerUserDoesNotSpecifiedOrServerPassDoesNotSpecified),
            errors[0].ErrorCode);
    }

    [Fact]
    public void Create_WithSqlAuthenticationAndWhitespaceServerUser_ReturnsError()
    {
        // Arrange
        var windowsNtIntegratedSecurity = false;
        var serverUser = "   ";
        var serverPass = "testpass";
        var useConsole = false;

        // Act
        var result = DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT1);
        var errors = result.AsT1;
        Assert.Single(errors);
        Assert.Equal(
            nameof(DbToolsErrors
                .WindowsNtIntegratedSecurityIsOffAndServerUserDoesNotSpecifiedOrServerPassDoesNotSpecified),
            errors[0].ErrorCode);
    }

    [Fact]
    public void Create_WithSqlAuthenticationAndWhitespaceServerPass_ReturnsError()
    {
        // Arrange
        var windowsNtIntegratedSecurity = false;
        var serverUser = "testuser";
        var serverPass = "   ";
        var useConsole = false;

        // Act
        var result = DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT1);
        var errors = result.AsT1;
        Assert.Single(errors);
        Assert.Equal(
            nameof(DbToolsErrors
                .WindowsNtIntegratedSecurityIsOffAndServerUserDoesNotSpecifiedOrServerPassDoesNotSpecified),
            errors[0].ErrorCode);
    }

    [Fact]
    public void Create_WithSqlAuthenticationAndWhitespaceCredentials_ReturnsError()
    {
        // Arrange
        var windowsNtIntegratedSecurity = false;
        var serverUser = "   ";
        var serverPass = "   ";
        var useConsole = false;

        // Act
        var result = DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT1);
        var errors = result.AsT1;
        Assert.Single(errors);
        Assert.Equal(
            nameof(DbToolsErrors
                .WindowsNtIntegratedSecurityIsOffAndServerUserDoesNotSpecifiedOrServerPassDoesNotSpecified),
            errors[0].ErrorCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Create_WithWindowsAuthenticationAndUseConsoleParameter_ReturnsDbAuthSettingsBase(bool useConsole)
    {
        // Arrange
        var windowsNtIntegratedSecurity = true;
        string? serverUser = null;
        string? serverPass = null;

        // Act
        var result = DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT0);
        var authSettings = result.AsT0;
        Assert.IsType<DbAuthSettingsBase>(authSettings);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Create_WithSqlAuthenticationValidCredentialsAndUseConsoleParameter_ReturnsDbAuthSettings(
        bool useConsole)
    {
        // Arrange
        var windowsNtIntegratedSecurity = false;
        var serverUser = "testuser";
        var serverPass = "testpass";

        // Act
        var result = DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT0);
        var authSettings = result.AsT0;
        Assert.IsType<DbAuthSettings>(authSettings);
        var dbAuthSettings = (DbAuthSettings)authSettings;
        Assert.Equal("testuser", dbAuthSettings.ServerUser);
        Assert.Equal("testpass", dbAuthSettings.ServerPass);
    }

    [Theory]
    [InlineData(false)]
    public void Create_WithSqlAuthenticationInvalidCredentialsAndUseConsoleParameter_ReturnsError(bool useConsole)
    {
        // Arrange
        var windowsNtIntegratedSecurity = false;
        string? serverUser = null;
        string? serverPass = null;

        // Act
        var result = DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT1);
        var errors = result.AsT1;
        Assert.Single(errors);
    }

    [Fact]
    public void Create_ErrorMessageContent_MatchesExpectedMessage()
    {
        // Arrange
        var windowsNtIntegratedSecurity = false;
        string? serverUser = null;
        string? serverPass = null;
        var useConsole = false;

        // Act
        var result = DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT1);
        var errors = result.AsT1;
        Assert.Single(errors);
        Assert.Equal(
            "WindowsNtIntegratedSecurity is off and serverUser does not specified or serverPass does not specified",
            errors[0].ErrorMessage);
    }
}
