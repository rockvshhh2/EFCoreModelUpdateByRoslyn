namespace CodeGenerates.Core.Dto
{
    /// <summary>
    /// 欄位 Dto
    /// </summary>
    public class ColumnDto
    {
        /// <summary>
        /// 欄位名稱
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 欄位描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 定序
        /// </summary>
        public string Collation { get; set; }

        /// <summary>
        /// 欄位資料型態
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// 欄位長度
        /// </summary>
        public int? Length { get; set; }

        /// <summary>
        /// 欄位編碼
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 是否允許NULL
        /// </summary>
        public bool IsNull { get; set; }
    }
}
