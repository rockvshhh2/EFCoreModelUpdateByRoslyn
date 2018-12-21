using CodeGenerates.Core.Dto;
using DbService.Interface;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DbService.Service
{
    public class MSSQLDbService : IDbInfoService
    {
        public DbDto GetDbInfo(string connectionString)
        {
            DbDto db = new DbDto {
                ConnectionString = connectionString
            };

            List<TmpTableInfo> tmpTableInfos;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                GetTables(conn, db);
                tmpTableInfos = GetTableInfos(conn);
            }

            foreach (var table in db.Tables)
            {
                var infos = tmpTableInfos.Where(x => x.TableName == table.Name);

                foreach (var info in infos)
                {
                    table.Columns.Add(new ColumnDto
                    {
                        Name = info.ColumnName,
                        Collation = info.CollationName,
                        DataType = info.DataType,
                        Description = info.Description,
                        IsNull = info.isNull,
                        Length = info.CharMaxLength,
                        Sort = info.OrdinalPostion
                    });
                }
            }

            return db;
        }

        public List<TableDto> GetTables(string connectionString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 取得Table資訊
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="db"></param>
        private void GetTables(SqlConnection conn, DbDto db)
        {
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM INFORMATION_SCHEMA.TABLES", conn))
            {
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    bool isFirst = true;

                    while (dr.Read())
                    {
                        if (dr["TABLE_NAME"] != null)
                        {
                            if (isFirst)
                            {
                                if (dr["TABLE_CATALOG"] != null)
                                {
                                    db.Name = dr["TABLE_CATALOG"].ToString();
                                }
                            }

                            db.Tables.Add(new TableDto
                            {
                                Name = dr["TABLE_NAME"].ToString(),
                                IsView = dr["TABLE_TYPE"].ToString() == "VIEW"
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 取得Table欄位資訊
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="db"></param>
        private List<TmpTableInfo> GetTableInfos(SqlConnection conn)
        {
            List<TmpTableInfo> datas = new List<TmpTableInfo>();

            using (SqlCommand cmd = new SqlCommand(@"SELECT *,(SELECT [value] FROM ::fn_listextendedproperty(NULL, 'schema', 'dbo', 'table', isc.TABLE_NAME, 'column', NULL) WHERE [name] = 'MS_Description' AND [objname] = isc.COLUMN_NAME COLLATE Latin1_General_CI_AI) AS DESCRIPTION  FROM INFORMATION_SCHEMA.COLUMNS as isc", conn))
            {
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var t = new TmpTableInfo();
                        t.TableName = dr["TABLE_NAME"].ToString();
                        t.ColumnName = dr["COLUMN_NAME"].ToString();
                        t.OrdinalPostion = Convert.ToInt32(dr["ORDINAL_POSITION"]);
                        t.isNull = dr["IS_NULLABLE"].ToString() == "YES";
                        t.DataType = dr["DATA_TYPE"].ToString();
                        t.CharMaxLength = dr["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value ? Convert.ToInt32(dr["CHARACTER_MAXIMUM_LENGTH"]) : 0;
                        t.CollationName = dr["COLLATION_NAME"].ToString();
                        t.Description = dr["DESCRIPTION"].ToString();

                        datas.Add(t);
                    }
                }
            }

            return datas;
        }

        class TmpTableInfo
        {
            public string TableName { get; set; }

            public string ColumnName { get; set; }

            public int OrdinalPostion { get; set; }

            public bool isNull { get; set; }

            public string DataType { get; set; }

            public int? CharMaxLength { get; set; }

            public string CollationName { get; set; }

            public string Description { get; set; }
        }
    }
}
