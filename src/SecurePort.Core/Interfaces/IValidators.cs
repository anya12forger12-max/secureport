using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Generic validation interface for any type T.
/// </summary>
public interface IValidator<in T>
{
    ValidationResult Validate(T value);
}

/// <summary>
/// Validates scan target input.
/// </summary>
public interface ITargetInputValidator : IValidator<string>
{
}

/// <summary>
/// Validates port numbers and ranges.
/// </summary>
public interface IPortValidator
{
    ValidationResult ValidatePort(int port);
    ValidationResult ValidatePortRange(int start, int end);
}

/// <summary>
/// Validates scan profile configurations.
/// </summary>
public interface IProfileValidator : IValidator<string>
{
}

/// <summary>
/// Validates report metadata and content.
/// </summary>
public interface IReportValidator : IValidator<ReportMetadata>
{
}

/// <summary>
/// Validates backup metadata.
/// </summary>
public interface IBackupValidator : IValidator<BackupMetadata>
{
}

/// <summary>
/// Validates import files.
/// </summary>
public interface IImportValidator : IValidator<string>
{
    /// <summary>Validates that the file at the given path is a valid import source.</summary>
    ValidationResult ValidateImportFile(string filePath, string expectedFormat);
}

/// <summary>
/// Validates export destinations.
/// </summary>
public interface IExportValidator : IValidator<string>
{
}

/// <summary>
/// Validates application configuration.
/// </summary>
public interface IConfigurationValidator
{
    ValidationResult ValidateConfiguration(AppConfiguration config);
    ValidationResult ValidateConfigurationFile(string filePath);
}
