using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Web.Utility.Extensions;

namespace Web.Services.FileProvider
{
    public class LocalFileProvider : IFileProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        protected virtual string RootPath { get; } = "";
        private readonly string _basePath;

        public LocalFileProvider(IHostingEnvironment hostingEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _basePath = Path.Combine(hostingEnvironment.WebRootPath, "uploads");
        }

        #region Implementation of IFolder

        private string Map(string virtualPath)
        {
            virtualPath = virtualPath.TrimStart(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            return Path.Combine(_basePath, virtualPath);
        }

        /// <summary>
        /// 判断一个虚拟路径的文件是否存在。
        /// </summary>
        /// <param name="virtualPath">虚拟路径。</param>
        /// <returns>虚拟路径。</returns>
        public bool FileExists(string virtualPath)
        {
            return File.Exists(Map(virtualPath));
        }

        /// <summary>
        /// 打开一个读取流。
        /// </summary>
        /// <param name="virtualPath">虚拟路径。</param>
        /// <returns>读取流。</returns>
        public Stream OpenReadStream(string virtualPath)
        {
            var fileName = Map(virtualPath);
            return !File.Exists(fileName) ? null : File.OpenRead(fileName);
        }

        /// <summary>
        /// 删除文件。
        /// </summary>
        /// <param name="virtualPath">虚拟路径。</param>
        public void DeleteFile(string virtualPath)
        {
            var path = Map(virtualPath);
            if (File.Exists(path))
                File.Delete(path);
        }

        /// <summary>
        /// 删除目录。
        /// </summary>
        /// <param name="virtualPath">虚拟路径。</param>
        public void DeleteDirectory(string virtualPath)
        {
            var path = Map(virtualPath);
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        /// <summary>
        /// 判断目录是否存在。
        /// </summary>
        /// <param name="virtualPath">虚拟路径。</param>
        /// <returns>如果存在则返回true，否则返回false。</returns>
        public bool DirectoryExists(string virtualPath)
        {
            var path = Map(virtualPath);
            return Directory.Exists(path);
        }

        /// <summary>
        /// 创建一个目录。
        /// </summary>
        /// <param name="virtualPath">虚拟路径。</param>
        public void CreateDirectory(string virtualPath)
        {
            var path = Map(virtualPath);
            Directory.CreateDirectory(path);
        }

        /// <summary>
        /// 获取指定路径下的所有文件。
        /// </summary>
        /// <param name="virtualPath">虚拟路径。</param>
        /// <param name="fileExtensions">文件扩展名。</param>
        /// <param name="skip">跳过数量。</param>
        /// <param name="take">取出数量。</param>
        /// <param name="includeChildren">是否包含子目录文件。</param>
        /// <returns>文件路径集合。</returns>
        public IEnumerable<string> ListFiles(string virtualPath, string[] fileExtensions = null, int skip = 0, int take = 1000,
            bool includeChildren = false)
        {
            var path = Map(virtualPath);
            return !Directory.Exists(path) ? Enumerable.Empty<string>() : Directory.GetFiles(path, "*", includeChildren ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).Where(
                p =>
                {
                    if (fileExtensions == null)
                        return true;
                    var ex = Path.GetExtension(p);
                    return fileExtensions.Any(z => string.Equals(z, ex, StringComparison.OrdinalIgnoreCase));
                }).Select(GetVirtualPath).Skip(skip).Take(take);
        }

        /// <summary>
        /// 创建文件。
        /// </summary>
        /// <param name="path">文件路径。</param>
        /// <param name="stream">文件流。</param>
        /// <returns>一个任务。</returns>
        public async Task CreateFile(string path, Stream stream)
        {
            path = path.TrimStart(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            var fileName = Path.Combine(_basePath, path);
            var directoryName = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);

            using (var fileStream = File.Create(fileName))
            {
                await fileStream.WriteAsync(buffer, 0, buffer.Length);
            }
        }

        /// <summary>
        /// 根据虚拟路径获取文件的Url地址。
        /// </summary>
        /// <param name="virtualPath">虚拟路径。</param>
        /// <returns>Uri地址。</returns>
        public string GetUrl(string virtualPath)
        {
            var context = _httpContextAccessor.HttpContext;
            var path = Path.Combine(_basePath, FixPath(virtualPath))
                .Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (context == null)
            {
                return path;
            }

            return context.Request.GetUrlPrefix() + path;
        }

        private static string FixPath(string path)
        {
            return path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).TrimStart(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// 获取虚拟路径。
        /// </summary>
        /// <param name="path">路径。</param>
        /// <returns>虚拟路径。</returns>
        public string GetVirtualPath(string path)
        {
            if (Path.IsPathRooted(path))
                path = path.Remove(0, _basePath.Length).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            //  ~/{RootPath}/xxx to ~/{RootPath}
            if (path.StartsWith("~/" + RootPath, StringComparison.OrdinalIgnoreCase))
                return path;
            //  /{RootPath}/xxx to ~/{RootPath}
            if (path.StartsWith("/" + RootPath, StringComparison.OrdinalIgnoreCase))
                return path.Insert(0, "~");
            //  {RootPath}/xxx to ~/{RootPath}
            if (path.StartsWith(RootPath, StringComparison.OrdinalIgnoreCase))
                return path.Insert(0, "~/");

            // xxx、~/xxx、/xxx to ~/{RootPath}/xxx
            if (path.StartsWith("~/"))
                path = path.Remove(0, 2);
            else if (path.StartsWith("/"))
                path = path.Remove(0, 1);
            return path.Insert(0, "~/" + RootPath + "/");
        }

        #endregion Implementation of IFolder
    }
}