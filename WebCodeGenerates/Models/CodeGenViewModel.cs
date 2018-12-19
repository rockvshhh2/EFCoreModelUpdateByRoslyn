using CodeGenerates.Core.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebCodeGenerates.Models
{
    public class CodeGenViewModel
    {
        public List<DbDto> Dbs { get; set; }

        public bool IsCreate { get; set; }

        public bool IsNeedAttributes { get; set; }
    }
}
