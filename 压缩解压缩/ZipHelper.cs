using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;

namespace 压缩解压缩 {
    class ZipHelper {
        private const String password = "liuyan";
        /// <summary>
        /// 压缩单个文件
        /// </summary>
        /// <param name="SourceFileAbsolutePath">被压缩文件的绝对路径，含文件名</param>
        /// <param name="ZipAbsolutePath">压缩文件的绝对路径，不含文件名，默认与原路径相同</param>
        /// <param name="ZipName">压缩文件的名称，默认与源文件同名,不含后缀</param>
        /// <param name="Level">压缩等级（0-9），默认为5</param>
        /// <param name="BlockSize">每次写入文件的缓存大小，默认2048</param>
        /// <param name="IsEncrypt">是否加密，默认不加密false</param>
        public static void ZipFile(String SourceFileAbsolutePath, String ZipAbsolutePathNoFileName = "", String ZipName = "", int Level = 5, int BlockSize = 2048, bool IsEncrypt = false) {
            if (!File.Exists(SourceFileAbsolutePath)) {
                if (Directory.Exists(SourceFileAbsolutePath)) {
                    throw new Exception(SourceFileAbsolutePath + "\n是一个文件夹而不是一个文件");
                }
                throw new FileNotFoundException("被压缩文件:\n" + SourceFileAbsolutePath + "\n不存在");
            }
            ZipAbsolutePathNoFileName = String.IsNullOrEmpty(ZipAbsolutePathNoFileName) ?
                SourceFileAbsolutePath.Substring(0, SourceFileAbsolutePath.LastIndexOf('\\')) :
                ZipAbsolutePathNoFileName;
            String SourceFileAllName = new FileInfo(SourceFileAbsolutePath).Name;
            String ZipFileAllName = SourceFileAllName.Substring(0, SourceFileAllName.LastIndexOf('.')) + @".zip";
            String ZipFileAbsolutePath = String.IsNullOrEmpty(ZipName) ?
                ZipAbsolutePathNoFileName + @"\" + ZipFileAllName :
                ZipAbsolutePathNoFileName + @"\" + ZipName + @".zip";
            using (FileStream ZipFileStream = File.Create(ZipFileAbsolutePath)) {
                using (ZipOutputStream ZipOutStream = new ZipOutputStream(ZipFileStream)) {
                    using (FileStream SourceFileInputStream = new FileStream(SourceFileAbsolutePath, FileMode.Open, FileAccess.Read)) {
                        ZipEntry zipEntry = new ZipEntry(SourceFileAllName);
                        if (IsEncrypt) {
                            ZipOutStream.Password = password;
                        }
                        ZipOutStream.PutNextEntry(zipEntry);
                        ZipOutStream.SetLevel(Level);
                        byte[] buffer = new byte[BlockSize];
                        int ReadSize = 0;
                        try {
                            while ((ReadSize = SourceFileInputStream.Read(buffer, 0, buffer.Length)) > 0) {
                                ZipOutStream.Write(buffer, 0, ReadSize);
                            }
                        }
                        catch (Exception e) {
                            throw e;
                        }
                        SourceFileInputStream.Close();
                    }
                    ZipOutStream.Finish();
                    ZipOutStream.Close();
                }
                ZipFileStream.Close();
            }

        }

        /// <summary>
        /// 压缩单个文件夹
        /// </summary>
        /// <param name="DirectoryAbsolutePath">被压缩文件夹的绝对路径,含文件夹名</param>
        /// <param name="ZipAbsolutePathNoName">压缩文件的绝对路径，不含文件夹名称，默认同原文件夹</param>
        /// <param name="ZipName">压缩文件的名称，默认同源文件夹名称</param>
        /// <param name="IsEncrypt">是否加密，默认不加密false</param>
        public static void ZipDirectory(String DirectoryAbsolutePath, String ZipAbsolutePathNoName = "", String ZipName = "", bool IsEncrypt = false) {
            if (!Directory.Exists(DirectoryAbsolutePath)) {
                if (File.Exists(DirectoryAbsolutePath)) {
                    throw new Exception(DirectoryAbsolutePath + "\n是一个文件而不是一个文件夹");
                }
                throw new FileNotFoundException("被压缩文件夹:\n" + DirectoryAbsolutePath + "\n不存在");
            }
            ZipAbsolutePathNoName = String.IsNullOrEmpty(ZipAbsolutePathNoName) ?
                DirectoryAbsolutePath.Substring(0, DirectoryAbsolutePath.LastIndexOf('\\')) :
                ZipAbsolutePathNoName;
            String DirectoryName = new DirectoryInfo(DirectoryAbsolutePath).Name;
            String ZipAbsolutePath = String.IsNullOrEmpty(ZipName) ?
                ZipAbsolutePathNoName + @"\" + DirectoryName + @".zip" :
                ZipAbsolutePathNoName + @"\" + ZipName + @".zip";
            using (FileStream ZipStream = File.Create(ZipAbsolutePath)) {
                using (ZipOutputStream ZipOutStream = new ZipOutputStream(ZipStream)) {
                    if (IsEncrypt) {
                        ZipOutStream.Password = password;
                    }
                    ZipSetEntry(DirectoryAbsolutePath, ZipOutStream, "");
                }
            }

        }

        /// <summary>
        /// 遍历文件夹中的目录
        /// </summary>
        /// <param name="directoryAbsolutePath"></param>
        /// <param name="zipOutStream"></param>
        /// <param name="parentPath"></param>
        private static void ZipSetEntry(string directoryAbsolutePath, ZipOutputStream zipOutStream, String parentPath) {
            if (directoryAbsolutePath[directoryAbsolutePath.Length - 1] != Path.DirectorySeparatorChar) {
                directoryAbsolutePath += Path.DirectorySeparatorChar;
            }
            Crc32 crc = new Crc32();
            String[] filenames = Directory.GetFileSystemEntries(directoryAbsolutePath);
            foreach (String file in filenames) {
                if (Directory.Exists(file)) {
                    String path = parentPath;
                    path += file.Substring(file.LastIndexOf("\\") + 1);
                    path += @"\";
                    ZipSetEntry(file, zipOutStream, path);
                }
                else {
                    using (FileStream fs = File.OpenRead(file)) {
                        byte[] buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, buffer.Length);
                        String filename = parentPath + file.Substring(file.LastIndexOf("\\") + 1);
                        ZipEntry zipEntry = new ZipEntry(filename);
                        zipEntry.DateTime = DateTime.Now;
                        zipEntry.Size = fs.Length;
                        fs.Close();
                        crc.Reset();
                        crc.Update(buffer);
                        zipEntry.Crc = crc.Value;
                        zipOutStream.PutNextEntry(zipEntry);
                        zipOutStream.Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }

        /// <summary>
        /// 解压ZIP文件
        /// </summary>
        /// <param name="ZipAbsolutePath">需要解压的ZIP文件的绝对路径，含文件名</param>
        /// <param name="TargetDirectory">解压到的目录,默认与原同目录</param>
        /// <param name="passW">解压密码,默认为空</param>
        /// <param name="OverWrite">是否覆盖已存在的文件</param>
        public static void UnZip(String ZipAbsolutePath, String TargetDirectory = "", String passW = "", bool OverWrite = true) {
            TargetDirectory = String.IsNullOrEmpty(TargetDirectory) ?
                ZipAbsolutePath.Substring(0, ZipAbsolutePath.LastIndexOf('\\')) :
                TargetDirectory;
            if (!Directory.Exists(TargetDirectory)) {
                throw new FileNotFoundException("指定的目录：\n" + TargetDirectory + "\n不存在");
            }
            if (!TargetDirectory.EndsWith("\\")) {
                TargetDirectory += @"\";
            }
            using (ZipInputStream zipIS = new ZipInputStream(File.OpenRead(ZipAbsolutePath))) {
                zipIS.Password = passW;
                ZipEntry zipEntry;
                while ((zipEntry = zipIS.GetNextEntry()) != null) {
                    
                        String directoryName = "";
                        String pathToZip = "";
                        pathToZip = zipEntry.Name;
                        if (!pathToZip.Equals("")) {
                            directoryName = Path.GetDirectoryName(pathToZip) + @"\";
                        }
                        String filename = Path.GetFileName(pathToZip);
                        Directory.CreateDirectory(TargetDirectory + directoryName);
                        if ((filename != "")) {
                            if ((File.Exists(TargetDirectory + directoryName + filename) && OverWrite) || (!File.Exists(TargetDirectory + directoryName + filename))) {
                                using (FileStream streamWriter = File.Create(TargetDirectory + directoryName + filename)) {
                                    int size = 2048;
                                    byte[] data = new byte[2048];
                                    while (true) {
                                        size = zipIS.Read(data, 0, data.Length);
                                        if (size > 0) {
                                            streamWriter.Write(data, 0, size);
                                        }
                                        else {
                                            break;
                                        }
                                    }
                                    streamWriter.Close();
                                }
                            }
                        }
                    
                }
                zipIS.Close();
            }
        }


    }

}
