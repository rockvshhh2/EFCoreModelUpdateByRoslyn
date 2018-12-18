using CodeGenerates.Core.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbService.Interface
{
    public interface IDbInfoService
    {
        DbDto GetDbInfo(string connectionString);

        List<TableDto> GetTables(string connectionString);
    }
}
