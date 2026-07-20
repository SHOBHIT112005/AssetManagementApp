using AssetManagement.Application.DTOs;
using AssetManagement.Application.DTOs.AssignmentDTOs;
using AssetManagement.Application.Interfaces.Assets;
using AssetManagement.Application.Interfaces.Assignments;
using AssetManagement.Application.Interfaces.Employees;
using AssetManagement.Application.Interfaces.Data;
using AssetManagement.Application.Interfaces.Agent;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums.Assets;
using AssetManagement.Domain.Enums.Assignments;
using AssetManagement.Domain.Enums.Employees;
using AssetManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Dapper;

namespace AssetManagement.Infrastructure.Repositories;

public class AssetAssignmentRepository : IAssetAssignmentRepository
{
    private readonly AssetDbContext _context;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public AssetAssignmentRepository(AssetDbContext context, ISqlConnectionFactory sqlConnectionFactory)
    {
        _context = context;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<IEnumerable<AssetAssignment>> GetAllAsync()
    {
        return await _context.AssetAssignments.AsNoTracking().ToListAsync();
    }

    public async Task<AssetAssignment?> GetByIdAsync(int id)
    {
        return await _context.AssetAssignments.FindAsync(id);
    }

    public async Task AddAsync(AssetAssignment assetAssignment)
    {
        await _context.AssetAssignments.AddAsync(assetAssignment);
    }

    public async Task UpdateAsync(AssetAssignment assignment)
    {
        var existingAssignment = await _context.AssetAssignments.FindAsync(assignment.Id);

        if (existingAssignment is null)
            return;

        existingAssignment.AssetId = assignment.AssetId;
        existingAssignment.EmployeeId = assignment.EmployeeId;
        existingAssignment.AssignmentDate = assignment.AssignmentDate;
        existingAssignment.ReturnDate = assignment.ReturnDate;

    }

    public async Task<IEnumerable<AssetAssignment>> GetByAssetIdAsync(int assetId)
    {
        return await _context.AssetAssignments.AsNoTracking().Where(a => a.AssetId == assetId).ToListAsync();
    }

    public async Task<IEnumerable<AssetAssignment>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _context.AssetAssignments.AsNoTracking().Where(a => a.EmployeeId == employeeId).ToListAsync();
    }

    public async Task<PagedResultDto<AssetAssignmentHistoryDto>> GetAssignmentHistoryAsync(AssignmentQueryDto queryDto)
    {
        // var query = _context.AssetAssignments
        //     .AsNoTracking()
        //     .Include(a => a.Asset)
        //     .Include(a => a.Employee)
        //     .AsQueryable();

        // // Searching
        // if (!string.IsNullOrEmpty(queryDto.SearchTerm))
        // {
        //     switch (queryDto.SearchField)
        //     {
        //         case AssignmentSearchField.AssetName:
        //             query = query.Where(a => a.Asset.AssetName.Contains(queryDto.SearchTerm));
        //             break;
        //         case AssignmentSearchField.AssignedEmployeeName:
        //             query = query.Where(a => a.Employee.FullName.Contains(queryDto.SearchTerm));
        //             break;
        //     }
        // }

        // if (queryDto.ReturnStatus.HasValue)
        // {
        //     if (queryDto.ReturnStatus.Value)
        //         query = query.Where(a => a.ReturnDate != null);
        //     else
        //         query = query.Where(a => a.ReturnDate == null);
        // }

        // if (queryDto.AssignmentDateFrom.HasValue)
        //     query = query.Where(a => a.AssignmentDate >= queryDto.AssignmentDateFrom.Value);

        // if (queryDto.AssignmentDateTo.HasValue)
        //     query = query.Where(a => a.AssignmentDate <= queryDto.AssignmentDateTo.Value);

        // if (queryDto.ReturnDateFrom.HasValue)
        //     query = query.Where(a => a.ReturnDate >= queryDto.ReturnDateFrom.Value);

        // if (queryDto.ReturnDateTo.HasValue)
        //     query = query.Where(a => a.ReturnDate <= queryDto.ReturnDateTo.Value);

        // // Sorting
        // if (queryDto.SortField.HasValue)
        // {
        //     switch (queryDto.SortField.Value)
        //     {
        //         case AssignmentSortField.AssignmentDate:
        //             query = queryDto.SortDirection == SortDirection.Ascending
        //                 ? query.OrderBy(a => a.AssignmentDate)
        //                 : query.OrderByDescending(a => a.AssignmentDate);
        //             break;
        //         case AssignmentSortField.ReturnDate:
        //             query = queryDto.SortDirection == SortDirection.Ascending
        //                 ? query.OrderBy(a => a.ReturnDate)
        //                 : query.OrderByDescending(a => a.ReturnDate);
        //             break;
        //         case AssignmentSortField.AssetName:
        //             query = queryDto.SortDirection == SortDirection.Ascending
        //                 ? query.OrderBy(a => a.Asset.AssetName)
        //                 : query.OrderByDescending(a => a.Asset.AssetName);
        //             break;
        //     }
        // }
        // else
        // {
        //     query = query.OrderByDescending(a => a.AssignmentDate);
        // }

        // var totalCount = await query.CountAsync();

        // var pagedAssignments = await query
        //     .Skip((queryDto.PageNumber - 1) * queryDto.PageSize)
        //     .Take(queryDto.PageSize)
        //     .Select(a => new AssetAssignmentHistoryDto
        //     {
        //         AssignmentId = a.Id,
        //         EmployeeName = a.Employee.FullName,
        //         AssetName = a.Asset.AssetName,
        //         SerialNumber = a.Asset.SerialNumber,
        //         AssignedDate = a.AssignmentDate,
        //         ReturnedDate = a.ReturnDate,
        //         IsReturned = a.ReturnDate != null
        //     })
        //     .ToListAsync();

        // return new PagedResultDto<AssetAssignmentHistoryDto>
        // {
        //     Items = pagedAssignments,
        //     TotalCount = totalCount,
        //     PageNumber = queryDto.PageNumber,
        //     PageSize = queryDto.PageSize
        // };
        var builder = new SqlBuilder();
        var sql = @"
        SELECT 
            aa.Id AS AssignmentId,
            e.FullName AS EmployeeName,
            CAST(e.Id AS VARCHAR) AS EmployeeNumber,
            e.Email AS EmployeeEmail,
            e.Department AS EmployeeDepartment,
            e.Designation AS EmployeeDesignation,
            a.AssetName AS AssetName,
            a.SerialNumber AS SerialNumber,
            a.Type AS AssetType,
            a.Condition AS AssetCondition,
            aa.AssignmentDate AS AssignedDate,
            aa.ReturnDate AS ReturnedDate,
            CASE WHEN aa.ReturnDate IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsReturned
        FROM AssetAssignments aa
        INNER JOIN Employees e ON aa.EmployeeId = e.Id
        INNER JOIN Assets a ON aa.AssetId = a.Id
        /**where**/ 
        /**orderby**/
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
        ";

        // create the template and pass the pagination parameters right away
        var template = builder.AddTemplate(sql, new
        {
            Offset = (queryDto.PageNumber - 1) * queryDto.PageSize,
            queryDto.PageSize
        });

        if (!string.IsNullOrEmpty(queryDto.SearchTerm))
        {
            // call .Where on the BUILDER, not the template
            switch (queryDto.SearchField)
            {
                case AssignmentSearchField.AssetName:
                    builder.Where("a.AssetName LIKE @SearchTerm", new { SearchTerm = $"%{queryDto.SearchTerm}%" });
                    break;
                case AssignmentSearchField.AssignedEmployeeName:
                    builder.Where("e.FullName LIKE @SearchTerm", new { SearchTerm = $"%{queryDto.SearchTerm}%" });
                    break;
            }
        }

        if (queryDto.ReturnStatus.HasValue)
        {
            if (queryDto.ReturnStatus.Value)
                builder.Where("aa.ReturnDate IS NOT NULL");
            else
                builder.Where("aa.ReturnDate IS NULL");
        }

        if (queryDto.AssignmentDateFrom.HasValue)
            builder.Where("aa.AssignmentDate >= @AssignmentDateFrom", new { queryDto.AssignmentDateFrom });

        if (queryDto.AssignmentDateTo.HasValue)
            builder.Where("aa.AssignmentDate <= @AssignmentDateTo", new { queryDto.AssignmentDateTo });

        if (queryDto.ReturnDateFrom.HasValue)
            builder.Where("aa.ReturnDate >= @ReturnDateFrom", new { queryDto.ReturnDateFrom });

        if (queryDto.ReturnDateTo.HasValue)
            builder.Where("aa.ReturnDate <= @ReturnDateTo", new { queryDto.ReturnDateTo });

        // Sorting  
        if (queryDto.SortField.HasValue)
        {
            switch (queryDto.SortField.Value)
            {
                case AssignmentSortField.AssignmentDate:
                    builder.OrderBy($"aa.AssignmentDate {(queryDto.SortDirection == SortDirection.Ascending ? "ASC" : "DESC")}");
                    break;
                case AssignmentSortField.ReturnDate:
                    builder.OrderBy($"aa.ReturnDate {(queryDto.SortDirection == SortDirection.Ascending ? "ASC" : "DESC")}");
                    break;
                case AssignmentSortField.AssetName:
                    builder.OrderBy($"a.AssetName {(queryDto.SortDirection == SortDirection.Ascending ? "ASC" : "DESC")}");
                    break;
            }
        }
        else
        {
            builder.OrderBy("aa.AssignmentDate DESC");
        }

        using var connection = _sqlConnectionFactory.CreateConnection();
        // Run the main query to get the page of data
        var pagedAssignments = await connection.QueryAsync<AssetAssignmentHistoryDto>(
            template.RawSql,
            template.Parameters
        );

        // A second template using the SAME builder just to get the total count
        var countTemplate = builder.AddTemplate(@"
        SELECT COUNT(*) 
        FROM AssetAssignments aa
        INNER JOIN Employees e ON aa.EmployeeId = e.Id
        INNER JOIN Assets a ON aa.AssetId = a.Id
        /**where**/
        ");

        // ExecuteScalarAsync returns a single value (the count) instead of a list
        var totalCount = await connection.ExecuteScalarAsync<int>(
            countTemplate.RawSql,
            countTemplate.Parameters
        );

        // Return the exact same object as returned in EF Core!
        return new PagedResultDto<AssetAssignmentHistoryDto>
        {
            Items = pagedAssignments,
            TotalCount = totalCount,
            PageNumber = queryDto.PageNumber,
            PageSize = queryDto.PageSize
        };
    }
}

