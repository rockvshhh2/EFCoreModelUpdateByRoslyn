﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CodeGenerates.Core.Dto
{
    /// <summary>
    /// 資料表 Dto
    /// </summary>
    public class TableDto
    {
        public TableDto()
        {
            Columns = new List<ColumnDto>();
        }

        /// <summary>
        /// 資料表名稱
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 是否需要
        /// </summary>
        public bool IsNeed { get; set; }

        /// <summary>
        /// 資料欄位
        /// </summary>
        public List<ColumnDto> Columns { get; set; }
    }
}