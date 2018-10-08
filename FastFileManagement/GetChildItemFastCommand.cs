using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace FastFileManagement
{
    [Cmdlet(VerbsCommon.Get, "ChildItemFast")]
    [OutputType(typeof(FileSystemInfo))]

    public sealed class GetChildItemFastCommand : Cmdlet
    {
        private readonly char pathSeparator = System.IO.Path.DirectorySeparatorChar;
        private ScanType scanType = ScanType.FilesAndFolders;

        #region parameters
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty()]
        public string[] Path { get; set; } = new string[] {".\\"};

        [Parameter(Mandatory = false, Position = 1)]
        public string Filter { get; set; } = "*";

        [Parameter()]
        public SwitchParameter ExcludeFiles { get; set; }

        [Parameter()]
        public SwitchParameter ExcludeDirectories { get; set; }

        [Parameter()]
        public SwitchParameter Recurse { get; set; }

        [Parameter(Mandatory = false)]
        [ValidateRange(1, int.MaxValue)]
        public int Depth { get; set; }

        [Alias("NamesOnly")]
        [Parameter()]
        public SwitchParameter PathsOnly { get; set; }
        #endregion

        protected override void BeginProcessing()
        {
            if (ExcludeFiles == false && ExcludeDirectories == false)
            {
                scanType = ScanType.FilesAndFolders;
            }
            else if (!ExcludeFiles)
            {
                scanType = ScanType.FilesOnly;
            }
            else if (!ExcludeDirectories)
            {
                scanType = ScanType.FoldersOnly;
            }
        }
        protected override void ProcessRecord()
        {
            List<string> foldersToScan = Path.ToList();
            if (Recurse == true)
            {
                int newDepth = 0;
                while (foldersToScan.Count != 0)
                {
                    string folder = foldersToScan.First();

                    if (Depth > 0 && Path.Contains(folder))
                    {
                        newDepth = folder.Trim(pathSeparator).Split(pathSeparator).Count() + Depth;
                    }
                    try
                    {
                        var subFolders = Directory.EnumerateDirectories(folder, Filter, SearchOption.TopDirectoryOnly);
                        if (newDepth > 0)
                        {
                            subFolders = subFolders.Where(sf => sf.Trim(pathSeparator).Split(pathSeparator).Count() <= newDepth);
                        }
                        foldersToScan.AddRange(subFolders);
                        foreach (var item in ScanDirectory(folder, scanType, PathsOnly, Filter))
                        {
                            WriteObject(item);
                        }
                    }
                    catch
                    {
                        WriteWarning("Could not scan folder contents of: " + folder);
                    }
                    foldersToScan.Remove(folder);
                }
            }
            else
            {
                foreach (String directory in foldersToScan)
                {
                    foreach (var item in ScanDirectory(directory, scanType, PathsOnly, Filter))
                    {
                        WriteObject(item);
                    }
                }
            }
        }

        private static IEnumerable<object> ScanDirectory(string path, ScanType scanType, bool pathsOnly, string filter)
        {
            IEnumerable<object> resultdata = null;
            switch (scanType)
            {
                case ScanType.FilesAndFolders:
                    if (pathsOnly)
                    {
                        resultdata = Directory.EnumerateFileSystemEntries(path, filter);
                    }
                    else
                    {
                        resultdata = new DirectoryInfo(path).EnumerateFileSystemInfos(filter);
                    }
                    break;
                case ScanType.FilesOnly:
                    if (pathsOnly)
                    {
                        resultdata = Directory.EnumerateFiles(path, filter);
                    }
                    else
                    {
                        resultdata = new DirectoryInfo(path).EnumerateFiles(filter);
                    }
                    break;
                case ScanType.FoldersOnly:
                    if (pathsOnly)
                    {
                        resultdata = Directory.EnumerateDirectories(path, filter);
                    }
                    else
                    {
                        resultdata = new DirectoryInfo(path).EnumerateDirectories(filter);
                    }
                    break;
            }
            return resultdata;
        }
        private enum ScanType
        {
            FilesAndFolders = 0,
            FilesOnly = 1,
            FoldersOnly = 2
        }
    }
}
