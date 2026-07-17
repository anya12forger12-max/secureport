using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Manages report generation, storage, search, and organization.
/// </summary>
public interface IReportManager
{
    /// <summary>Generates a report from prepared report data and stores it.</summary>
    Task<ReportMetadata> GenerateAndStoreReportAsync(
        ReportData data,
        Enums.ExportFormat format,
        string? customTitle,
        CancellationToken ct);

    /// <summary>Gets all stored report metadata.</summary>
    Task<IReadOnlyList<ReportMetadata>> GetAllReportsAsync(CancellationToken ct);

    /// <summary>Gets report metadata by ID.</summary>
    Task<ReportMetadata?> GetReportByIdAsync(Guid reportId, CancellationToken ct);

    /// <summary>Deletes a report and its file.</summary>
    Task<bool> DeleteReportAsync(Guid reportId, CancellationToken ct);

    /// <summary>Renames a report's display title.</summary>
    Task<bool> RenameReportAsync(Guid reportId, string newTitle, CancellationToken ct);

    /// <summary>Duplicates a report with a new ID.</summary>
    Task<ReportMetadata?> DuplicateReportAsync(Guid reportId, string? newTitle, CancellationToken ct);

    /// <summary>Archives a report.</summary>
    Task<bool> ArchiveReportAsync(Guid reportId, CancellationToken ct);

    /// <summary>Restores an archived report.</summary>
    Task<bool> RestoreReportAsync(Guid reportId, CancellationToken ct);

    /// <summary>Searches reports by criteria.</summary>
    Task<IReadOnlyList<ReportMetadata>> SearchReportsAsync(ReportSearchCriteria criteria, CancellationToken ct);

    /// <summary>Adds a tag to a report.</summary>
    Task<bool> AddTagToReportAsync(Guid reportId, string tag, CancellationToken ct);

    /// <summary>Removes a tag from a report.</summary>
    Task<bool> RemoveTagFromReportAsync(Guid reportId, string tag, CancellationToken ct);

    /// <summary>Re-exports a previously generated report.</summary>
    Task<string> ReExportReportAsync(Guid reportId, string destinationPath, CancellationToken ct);
}
