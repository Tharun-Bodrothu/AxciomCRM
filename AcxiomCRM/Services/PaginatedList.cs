#nullable enable

using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services;

/// <summary>
/// A generic paginated list representing a subset of elements from a larger collection.
/// </summary>
/// <typeparam name="T">The entity or model type.</typeparam>
public class PaginatedList<T> : List<T>
{
    /// <summary>
    /// Current 1-based page index.
    /// </summary>
    public int PageIndex { get; private set; }

    /// <summary>
    /// Total number of pages available.
    /// </summary>
    public int TotalPages { get; private set; }

    /// <summary>
    /// Total count of items across all pages.
    /// </summary>
    public int TotalCount { get; private set; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; private set; }

    /// <summary>
    /// Indicates whether a previous page exists.
    /// </summary>
    public bool HasPreviousPage => PageIndex > 1;

    /// <summary>
    /// Indicates whether a next page exists.
    /// </summary>
    public bool HasNextPage => PageIndex < TotalPages;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaginatedList{T}"/> class.
    /// </summary>
    /// <param name="items">The items for the current page.</param>
    /// <param name="count">Total count of items across all pages.</param>
    /// <param name="pageIndex">Current 1-based page index.</param>
    /// <param name="pageSize">Number of items per page.</param>
    public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
    {
        TotalCount = count;
        PageSize = pageSize > 0 ? pageSize : 10;
        PageIndex = pageIndex > 0 ? pageIndex : 1;
        TotalPages = (int)Math.Ceiling(count / (double)PageSize);

        AddRange(items);
    }

    /// <summary>
    /// Parameterless constructor for serialization scenarios.
    /// </summary>
    [JsonConstructor]
    public PaginatedList()
    {
        PageIndex = 1;
        TotalPages = 0;
        TotalCount = 0;
        PageSize = 10;
    }

    /// <summary>
    /// Creates a new <see cref="PaginatedList{T}"/> asynchronously from an <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <param name="source">The source queryable sequence.</param>
    /// <param name="pageIndex">The requested 1-based page number.</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <returns>A paginated list containing the sliced subset of items.</returns>
    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
    {
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 10;

        var count = await source.CountAsync();
        var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }

    /// <summary>
    /// Creates a new <see cref="PaginatedList{T}"/> synchronously from an in-memory collection.
    /// </summary>
    /// <param name="source">The source collection.</param>
    /// <param name="pageIndex">The requested 1-based page number.</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <returns>A paginated list containing the sliced subset of items.</returns>
    public static PaginatedList<T> Create(IEnumerable<T> source, int pageIndex, int pageSize)
    {
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 10;

        var sourceList = source as IList<T> ?? source.ToList();
        var count = sourceList.Count;
        var items = sourceList.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }
}
