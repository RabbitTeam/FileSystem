using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using Web.Utility;
using IFileProvider = Web.Services.FileProvider.IFileProvider;

namespace Web.Controllers
{
    [Route("")]
    public class FileController : Controller
    {
        #region Field

        private readonly IFileProvider _fileProvider;

        #endregion Field

        #region Constructor

        public FileController(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        #endregion Constructor

        #region Action

        [HttpPut("{bucket}/{*fileName}")]
        public ActionResult Upload(string bucket, string fileName)
        {
            var files = Request.Form.Files;
            if (files == null || !files.Any())
                return StatusCode(500);

            var file = files[0];
            using (var stream = file.OpenReadStream())
                _fileProvider.CreateFile(Combine(bucket, fileName), stream);

            return Ok();
        }

        public class ListFileModel
        {
            public string FileExtensions { get; set; }
            public int Skip { get; set; }
            public int Take { get; set; }
            public bool IncludeChildren { get; set; }
        }

        [HttpGet("{bucket}/{*path}")]
        public ActionResult Get(string bucket, string path, [FromQuery]ListFileModel model)
        {
            path = Combine(bucket, path);
            if (_fileProvider.FileExists(path))
            {
                var stream = _fileProvider.OpenReadStream(path);
                return File(stream, FileUtilitys.GetContentType(path));
            }
            if (model == null)
                model = new ListFileModel();

            if (model.Take <= 0)
                model.Take = 1000;

            return Json(_fileProvider.ListFiles(path, model.FileExtensions?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries), model.Skip, model.Take, model.IncludeChildren));
        }

        public class HeadResultModel
        {
            public string Type { get; set; }
        }

        [HttpHead("{bucket}/{*path}")]
        public ActionResult Head(string bucket, string path)
        {
            path = Combine(bucket, path);
            if (_fileProvider.FileExists(path))
                return Json(new HeadResultModel
                {
                    Type = "file"
                });
            if (_fileProvider.DirectoryExists(path))
                return Json(new HeadResultModel
                {
                    Type = "folder"
                });
            return StatusCode(404);
        }

        [HttpDelete("{bucket}/{*path}")]
        public ActionResult Delete(string bucket, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return StatusCode(500);

            path = Combine(bucket, path);

            if (_fileProvider.FileExists(path))
                _fileProvider.DeleteFile(path);
            else
                _fileProvider.DeleteDirectory(path);

            return Ok();
        }

        [HttpPost("{bucket}/{*directoryName}")]
        public ActionResult CreateDirectory(string bucket, string directoryName)
        {
            directoryName = Combine(bucket, directoryName);
            _fileProvider.CreateDirectory(directoryName);

            return Ok();
        }

        #endregion Action

        #region Private Method

        private static string Combine(string bucket, string path)
        {
            if (string.IsNullOrWhiteSpace(bucket))
                throw new ArgumentNullException();

            if (path == null)
                path = string.Empty;
            path = path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (!path.StartsWith("/"))
                path = path.Insert(0, "/");
            return bucket + path;
        }

        #endregion Private Method
    }
}