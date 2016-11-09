using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Core;
namespace ZipOneCode.ZipProvider
{
    public class ZipHelper
    {
        #region 压缩文件
        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="destinationZipFilePath"></param>
        public static void CreateZip(string sourceFilePath, string destinationZipFilePath)
        {
            if (sourceFilePath[sourceFilePath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                sourceFilePath += System.IO.Path.DirectorySeparatorChar;
            ZipOutputStream zipStream = new ZipOutputStream(File.Create(destinationZipFilePath));
            zipStream.SetLevel(9);  // 压缩级别 0-9
            CreateZipFiles(sourceFilePath, zipStream);
            zipStream.Finish();
            zipStream.Close();
        }
        /// <summary>
        /// 递归压缩文件
        /// </summary>
        /// <param name="sourceFilePath">待压缩的文件或文件夹路径</param>
        /// <param name="zipStream">打包结果的zip文件路径（类似 D:\WorkSpace\a.zip）,全路径包括文件名和.zip扩展名
        /// <param name="staticFile"></param>
        private static void CreateZipFiles(string sourceFilePath, ZipOutputStream zipStream)
        {
            Crc32 crc = new Crc32();
            string[] filesArray = Directory.GetFileSystemEntries(sourceFilePath);
            foreach (string file in filesArray)
            {
                if (Directory.Exists(file))                     //如果当前是文件夹，递归
                {
                    CreateZipFiles(file, zipStream);
                }
                else                                            //如果是文件，开始压缩
                {
                    FileStream fileStream = File.OpenRead(file);
                    byte[] buffer = new byte[fileStream.Length];
                    fileStream.Read(buffer, 0, buffer.Length);
                    string tempFile = file.Substring(sourceFilePath.LastIndexOf("\\") + 1);
                    ZipEntry entry = new ZipEntry(tempFile);
                    entry.DateTime = DateTime.Now;
                    entry.Size = fileStream.Length;
                    fileStream.Close();
                    crc.Reset();
                    crc.Update(buffer);
                    entry.Crc = crc.Value;
                    zipStream.PutNextEntry(entry);
                    zipStream.Write(buffer, 0, buffer.Length);
                }
            }
        }
        #endregion

        #region 解压缩
        /// <summary>
        /// 解压缩
        /// </summary>
        /// <param name="压缩文件地址"></param>
        /// <param name="解压到的目录"></param>
        public static void UnZip(string FileToUpZip, string ZipedFolder)
        {
            if (!File.Exists(FileToUpZip))
            {
                return;
            }

            if (!Directory.Exists(ZipedFolder))
            {
                Directory.CreateDirectory(ZipedFolder);
            }

            ZipInputStream s = null;
            ZipEntry theEntry = null;

            string fileName;
            FileStream streamWriter = null;
            try
            {
                s = new ZipInputStream(File.OpenRead(FileToUpZip));
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    if (theEntry.Name != String.Empty)
                    {
                        fileName = Path.Combine(ZipedFolder, theEntry.Name);
                        ///判断文件路径是否是文件夹

                        if (fileName.EndsWith("/") || fileName.EndsWith("\\"))
                        {
                            Directory.CreateDirectory(fileName);
                            continue;
                        }

                        streamWriter = File.Create(fileName);
                        int size = 2048;
                        byte[] data = new byte[2048];
                        while (true)
                        {
                            size = s.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
            finally
            {
                if (streamWriter != null)
                {
                    streamWriter.Close();
                    streamWriter = null;
                }
                if (theEntry != null)
                {
                    theEntry = null;
                }
                if (s != null)
                {
                    s.Close();
                    s = null;
                }
                GC.Collect();
                GC.Collect(1);
            }
        }

        #endregion
    }
}