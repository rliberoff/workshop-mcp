namespace McpWorkshop.Shared.Configuration;

/// <summary>
/// Data source connection settings.
/// </summary>
public class DataSourceSettings
{
    /// <summary>
    /// Gets or sets the SQL database connection string.
    /// </summary>
    public string SqlConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Cosmos DB connection string.
    /// </summary>
    public string CosmosConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Cosmos DB database name.
    /// </summary>
    public string CosmosDatabase { get; set; } = "workshop";

    /// <summary>
    /// Gets or sets the Azure Blob Storage connection string.
    /// </summary>
    public string BlobStorageConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the local file system path for data files.
    /// </summary>
    public string LocalDataPath { get; set; } = "../../../data";
}
