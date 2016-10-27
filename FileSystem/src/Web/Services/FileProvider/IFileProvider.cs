using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Web.Services.FileProvider
{
    public interface IFileProvider
    {
        /// <summary>
        /// 判断一个虚拟路径的文件是否存在。
        /// </summary>
        /// <param name="virtualPath">虚拟路径。</param>
        /// <returns>虚拟路径。</returns>
        bool FileExists(string virtualPath);

        /// <summary>
        /// 打开一个读取流。
        /// </summary>
        /// <param name="virtualPath">虚拟路径。</param>
        /// <returns>读取流。</returns>
        Stream OpenReadStream(string virtualPath);

        /// <summary>
        /// 删除文件。
        /// </summary>
        /// <param name="virtualPath">虚拟路径。</param>
        void DeleteFile(string virtualPath);

        /// <summary>
        /// 删除目录。
        /// </summary>
        /// <param name="virtualPath">虚拟路径。</param>
        void DeleteDirectory(string virtualPath);

        /// <summary>
        /// 判断目录是否存在。
        /// </summary>
        /// <param name="virtualPath">虚拟路径。</param>
        /// <returns>如果存在则返回true，否则返回false。</returns>
        bool DirectoryExists(string virtualPath);

        /// <summary>
        /// 创建一个目录。
        /// </summary>
        /// <param name="virtualPath">虚拟路径。</param>
        void CreateDirectory(string virtualPath);

        /// <summary>
        /// 获取指定路径下的所有文件。
        /// </summary>
        /// <param name="virtualPath">虚拟路径。</param>
        /// <param name="fileExtensions">文件扩展名。</param>
        /// <param name="skip">跳过数量。</param>
        /// <param name="take">取出数量。</param>
        /// <param name="includeChildren">是否包含子目录文件。</param>
        /// <returns>文件路径集合。</returns>
        IEnumerable<string> ListFiles(string virtualPath, string[] fileExtensions = null, int skip = 0, int take = 1000, bool includeChildren = false);

        /// <summary>
        /// 创建文件。
        /// </summary>
        /// <param name="path">文件路径。</param>
        /// <param name="stream">文件流。</param>
        /// <returns>一个任务。</returns>
        Task CreateFile(string path, Stream stream);
    }
}