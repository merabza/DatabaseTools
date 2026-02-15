using DatabaseTools.DbTools.Errors;
using DatabaseTools.DbTools.Models;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace DatabaseTools.DbTools.Tests;

public class DbAuthSettingsCreatorTests
{
    [Fact]
    public void Create_WithWindowsAuthenticationAndNoCredentials_ReturnsDbAuthSettingsBase()
    {
        // Arrange
        bool windowsNtIntegratedSecurity = true;
        string? serverUser = null;
        string? serverPass = null;
        bool useConsole = false;

        // Act
        OneOf<DbAuthSettingsBase, Err[]> result =
            DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT0);
        DbAuthSettingsBase? authSettings = result.AsT0;
        Assert.IsType<DbAuthSettingsBase>(authSettings);
        Assert.IsNotType<DbAuthSettings>(authSettings);
    }

    [Fact]
    public void Create_WithWindowsAuthenticationAndEmptyCredentials_ReturnsDbAuthSettingsBase()
    {
        // Arrange
        bool windowsNtIntegratedSecurity = true;
        string serverUser = string.Empty;
        string serverPass = string.Empty;
        bool useConsole = false;

        // Act
        OneOf<DbAuthSettingsBase, Err[]> result =
            DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT0);
        DbAuthSettingsBase? authSettings = result.AsT0;
        Assert.IsType<DbAuthSettingsBase>(authSettings);
        Assert.IsNotType<DbAuthSettings>(authSettings);
    }

    [Fact]
    public void Create_WithWindowsAuthenticationAndWhitespaceCredentials_ReturnsDbAuthSettingsBase()
    {
        // Arrange
        bool windowsNtIntegratedSecurity = true;
        string serverUser = "   ";
        string serverPass = "   ";
        bool useConsole = false;

        // Act
        OneOf<DbAuthSettingsBase, Err[]> result =
            DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT0);
        DbAuthSettingsBase? authSettings = result.AsT0;
        Assert.IsType<DbAuthSettingsBase>(authSettings);
        Assert.IsNotType<DbAuthSettings>(authSettings);
    }

    [Fact]
    public void Create_WithWindowsAuthenticationAndServerUser_ReturnsDbAuthSettingsBaseAndLogsWarning()
    {
        // Arrange
        bool windowsNtIntegratedSecurity = true;
        string serverUser = "testuser";
        string? serverPass = null;
        bool useConsole = false;

        // Act
        OneOf<DbAuthSettingsBase, Err[]> result =
            DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT0);
        DbAuthSettingsBase? authSettings = result.AsT0;
        Assert.IsType<DbAuthSettingsBase>(authSettings);
        Assert.IsNotType<DbAuthSettings>(authSettings);
    }

    [Fact]
    public void Create_WithWindowsAuthenticationAndServerPass_ReturnsDbAuthSettingsBaseAndLogsWarning()
    {
        // Arrange
        bool windowsNtIntegratedSecurity = true;
        string? serverUser = null;
        string serverPass = "testpass";
        bool useConsole = false;

        // Act
        OneOf<DbAuthSettingsBase, Err[]> result =
            DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT0);
        DbAuthSettingsBase? authSettings = result.AsT0;
        Assert.IsType<DbAuthSettingsBase>(authSettings);
        Assert.IsNotType<DbAuthSettings>(authSettings);
    }

    [Fact]
    public void Create_WithWindowsAuthenticationAndBothCredentials_ReturnsDbAuthSettingsBaseAndLogsWarning()
    {
        // Arrange
        bool windowsNtIntegratedSecurity = true;
        string serverUser = "testuser";
        string serverPass = "testpass";
        bool useConsole = false;

        // Act
        OneOf<DbAuthSettingsBase, Err[]> result =
            DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT0);
        DbAuthSettingsBase? authSettings = result.AsT0;
        Assert.IsType<DbAuthSettingsBase>(authSettings);
        Assert.IsNotType<DbAuthSettings>(authSettings);
    }

    [Fact]
    public void Create_WithSqlAuthenticationAndValidCredentials_ReturnsDbAuthSettings()
    {
        // Arrange
        bool windowsNtIntegratedSecurity = false;
        string serverUser = "testuser";
        string serverPass = "testpass";
        bool useConsole = false;

        // Act
        OneOf<DbAuthSettingsBase, Err[]> result =
            DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT0);
        DbAuthSettingsBase? authSettings = result.AsT0;
        Assert.IsType<DbAuthSettings>(authSettings);
        var dbAuthSettings = (DbAuthSettings)authSettings;
        Assert.Equal("testuser", dbAuthSettings.ServerUser);
        Assert.Equal("testpass", dbAuthSettings.ServerPass);
    }

    [Fact]
    public void Create_WithSqlAuthenticationAndNullServerUser_ReturnsError()
    {
        // Arrange
        bool windowsNtIntegratedSecurity = false;
        string? serverUser = null;
        string serverPass = "testpass";
        bool useConsole = false;

        // Act
        OneOf<DbAuthSettingsBase, Err[]> result =
            DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT1);
        Err[]? errors = result.AsT1;
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
        bool windowsNtIntegratedSecurity = false;
        string serverUser = "testuser";
        string? serverPass = null;
        bool useConsole = false;

        // Act
        OneOf<DbAuthSettingsBase, Err[]> result =
            DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT1);
        Err[]? errors = result.AsT1;
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
        bool windowsNtIntegratedSecurity = false;
        string? serverUser = null;
        string? serverPass = null;
        bool useConsole = false;

        // Act
        OneOf<DbAuthSettingsBase, Err[]> result =
            DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT1);
        Err[]? errors = result.AsT1;
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
        bool windowsNtIntegratedSecurity = false;
        string serverUser = string.Empty;
        string serverPass = "testpass";
        bool useConsole = false;

        // Act
        OneOf<DbAuthSettingsBase, Err[]> result =
            DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT1);
        Err[]? errors = result.AsT1;
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
        bool windowsNtIntegratedSecurity = false;
        string serverUser = "testuser";
        string serverPass = string.Empty;
        bool useConsole = false;

        // Act
        OneOf<DbAuthSettingsBase, Err[]> result =
            DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT1);
        Err[]? errors = result.AsT1;
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
        bool windowsNtIntegratedSecurity = false;
        string serverUser = "   ";
        string serverPass = "testpass";
        bool useConsole = false;

        // Act
        OneOf<DbAuthSettingsBase, Err[]> result =
            DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT1);
        Err[]? errors = result.AsT1;
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
        bool windowsNtIntegratedSecurity = false;
        string serverUser = "testuser";
        string serverPass = "   ";
        bool useConsole = false;

        // Act
        OneOf<DbAuthSettingsBase, Err[]> result =
            DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT1);
        Err[]? errors = result.AsT1;
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
        bool windowsNtIntegratedSecurity = false;
        string serverUser = "   ";
        string serverPass = "   ";
        bool useConsole = false;

        // Act
        OneOf<DbAuthSettingsBase, Err[]> result =
            DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT1);
        Err[]? errors = result.AsT1;
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
        bool windowsNtIntegratedSecurity = true;
        string? serverUser = null;
        string? serverPass = null;

        // Act
        OneOf<DbAuthSettingsBase, Err[]> result =
            DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT0);
        DbAuthSettingsBase? authSettings = result.AsT0;
        Assert.IsType<DbAuthSettingsBase>(authSettings);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Create_WithSqlAuthenticationValidCredentialsAndUseConsoleParameter_ReturnsDbAuthSettings(
        bool useConsole)
    {
        // Arrange
        bool windowsNtIntegratedSecurity = false;
        string serverUser = "testuser";
        string serverPass = "testpass";

        // Act
        OneOf<DbAuthSettingsBase, Err[]> result =
            DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT0);
        DbAuthSettingsBase? authSettings = result.AsT0;
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
        bool windowsNtIntegratedSecurity = false;
        string? serverUser = null;
        string? serverPass = null;

        // Act
        OneOf<DbAuthSettingsBase, Err[]> result =
            DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT1);
        Err[]? errors = result.AsT1;
        Assert.Single(errors);
    }

    [Fact]
    public void Create_ErrorMessageContent_MatchesExpectedMessage()
    {
        // Arrange
        bool windowsNtIntegratedSecurity = false;
        string? serverUser = null;
        string? serverPass = null;
        bool useConsole = false;

        // Act
        OneOf<DbAuthSettingsBase, Err[]> result =
            DbAuthSettingsCreator.Create(windowsNtIntegratedSecurity, serverUser, serverPass, useConsole);

        // Assert
        Assert.True(result.IsT1);
        Err[]? errors = result.AsT1;
        Assert.Single(errors);
        Assert.Equal(
            "WindowsNtIntegratedSecurity is off and serverUser does not specified or serverPass does not specified",
            errors[0].ErrorMessage);
    }
}
