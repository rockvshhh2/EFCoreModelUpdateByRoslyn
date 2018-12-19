using CodeGenerates.Core.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbService.Interface
{
    public interface IDbInfoService
    {
        /// <summary>
        /// 取得資料庫資訊
        /// </summary>
        /// <param name="connectionString">連線字串</param>
        /// <returns></returns>
        DbDto GetDbInfo(string connectionString);

        /// <summary>
        /// 取得資料表資訊
        /// </summary>
        /// <param name="connectionString">連線字串s</param>
        /// <returns></returns>
        List<TableDto> GetTables(string connectionString);
    }
}
