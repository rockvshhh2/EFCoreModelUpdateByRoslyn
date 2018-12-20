using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using CodeGenerates.Core.Dto;
using DbService.Interface;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebCodeGenerates.Models;
using System.IO.Compression;
using CodeGenerates.Service.Service;

namespace WebCodeGenerates.Controllers
{
    public static class DbTmpData
    {
        static DbTmpData()
        {
            Dbs = new List<DbDto>();
        }

        public static List<DbDto> Dbs { get; set; }

        public static bool IsCreate { get; set; }

        public static bool IsNeedAttributes { get; set; }

        public static List<string> Mdeols { get; set; }

    }

    public class CodeGenController : Controller
    {
        private readonly IDbInfoService _dbInfoService;

        private readonly string _uploadFolder;
        private readonly string _unZipFolder;

        public CodeGenController(IDbInfoService dbInfoService, IHostingEnvironment env)
        {
            _dbInfoService = dbInfoService;
            _uploadFolder = $@"{env.WebRootPath}\uploadFile";
            _unZipFolder = $@"{env.WebRootPath}\unZip";
        }

        
        public IActionResult Index()
        {

            return View(new CodeGenViewModel
            {
                Dbs = DbTmpData.Dbs,
                IsCreate = DbTmpData.IsCreate,
                IsNeedAttributes = DbTmpData.IsNeedAttributes,
                Models = DbTmpData.Mdeols
            });
        }

        [HttpPost]
        public IActionResult AddDb(string connectionString)
        {
            DbTmpData.Dbs.Add(_dbInfoService.GetDbInfo(connectionString));

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult CreateCode(List<DbDto> dbDtos, bool IsCreate, bool IsNeedAttributes,bool IsPlural, IFormFile zipfile)
        {
            for (int i = 0; i < dbDtos.Count; i++)
            {
                var soruceDb = dbDtos[i];
                var targetDb = DbTmpData.Dbs[i];

                for (int j = 0; j < soruceDb.Tables.Count; j++)
                {
                    var sourceTable = soruceDb.Tables[j];
                    var targetTable = targetDb.Tables[j];

                    targetTable.IsNeed = sourceTable.IsNeed;
                }
            }

            DbTmpData.IsCreate = IsCreate;
            DbTmpData.IsNeedAttributes = IsNeedAttributes;

            string extractPath = $"{_unZipFolder}/{Guid.NewGuid().ToString()}";

            if (!IsCreate)
            {
                if (zipfile != null)
                {
                    string path = $"{_uploadFolder}/{zipfile.FileName}";
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        zipfile.CopyTo(stream);
                    }

                    if (!Directory.Exists(extractPath))
                    {
                        Directory.CreateDirectory(extractPath);
                    }

                    ZipFile.ExtractToDirectory(path, extractPath);
                }
            }

            var ss = new AnalysisRewriteService(new CodeGenerates.Service.SyntaxCommand());

            DbTmpData.Mdeols = ss.UpdateModels(extractPath, DbTmpData.Dbs, IsPlural, IsNeedAttributes);

            return RedirectToAction(nameof(Index));
        }
    }
}