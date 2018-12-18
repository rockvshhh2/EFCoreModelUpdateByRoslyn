using System;
using System.Collections.Generic;
using System.Text;

namespace CodeGenerates.Core.Dto
{
    /// <summary>
    /// 資料庫 Dto
    /// </summary>
    public class DbDto
    {
        public DbDto()
        {
            Tables = new List<TableDto>();
        }

        /// <summary>
        /// 資料庫連線字串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 資料庫名稱
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 資料表
        /// </summary>
        public List<TableDto> Tables { get; set; }
    }
}
