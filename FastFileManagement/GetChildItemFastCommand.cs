using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace FastFileManagement
{
    [Cmdlet(VerbsCommon.Get, "ChildItemFast",DefaultParameterSetName = PathParameterSet)]
    [OutputType(typeof(FileSystemInfo))]

    public sealed class GetChildItemFastCommand : PSCmdlet
    {
        private readonly char pathSeparator = System.IO.Path.DirectorySeparatorChar;
        private ScanType scanType = ScanType.FilesAndFolders;
        private const string LiteralParamSet = "Literal";
        private const string PathParameterSet = "Path";
        private bool _shouldExpandWildcards;
        private string[] _paths = new string[] {@".\"};

        #region parameters
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = PathParameterSet)]
        [ValidateNotNullOrEmpty]
        public string[] Path
        {
            get { return _paths; }
            set
            {
                _shouldExpandWildcards = true;
                _paths = value;
            }
        }
        [Parameter(Position = 0, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true, ParameterSetName = LiteralParamSet)]
        [ValidateNotNullOrEmpty]
        public string[] LiteralPath
        {
            get { return _paths; }
            set { _paths = value; }
        }

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
            if (ExcludeDirectories==true && ExcludeFiles==true)
            {
                ThrowTerminatingError(new ErrorRecord(new PSArgumentException("You cannot exclude both files and directories"), "ExcludeEveryResultType", ErrorCategory.InvalidArgument, this));
            }
            else if (ExcludeFiles==true)
            {
                scanType = ScanType.FoldersOnly;
            }
            else if (ExcludeDirectories==true)
            {
                scanType = ScanType.FilesOnly;
            }
        }
        protected override void ProcessRecord()
        {
            #region resolve paths from input and write out file objects for any input paths that points to a file.
            List<string> resolvedPaths = new List<string>();
            foreach (string rawPath in _paths)
            {
                if (_shouldExpandWildcards==true)
                {
                    resolvedPaths.AddRange(GetResolvedProviderPathFromPSPath(rawPath, out ProviderInfo provider));
                }
                else
                {
                    resolvedPaths.Add(GetUnresolvedProviderPathFromPSPath(rawPath));
                }
            }
            List<string> foldersToScan = new List<string>();
            foreach (string resolvedPath in resolvedPaths)
            {
                if (File.Exists(resolvedPath))
                {
                    WriteObject(new FileInfo(resolvedPath));
                }
                else
                {
                    foldersToScan.Add(resolvedPath);
                }
            }
            #endregion

            if (Recurse == true)
            {
                int newDepth = 0;
                while (foldersToScan.Count != 0)
                {
                    string folder = foldersToScan.First();

                    if (Depth > 0 && resolvedPaths.Contains(folder))
                    {
                        newDepth = folder.Trim(pathSeparator).Split(pathSeparator).Count() + Depth;
                    }
                    try
                    {
                        var subFolders = Directory.EnumerateDirectories(folder,"*", SearchOption.TopDirectoryOnly);
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
                    //Makes the cmdlet stop properly when the pipeline is stopped by Select-Object -first X
                    catch (PipelineStoppedException e)
                    {
                        throw e;
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
                foreach (string directory in foldersToScan)
                {
                    try
                    {
                        foreach (var item in ScanDirectory(directory, scanType, PathsOnly, Filter))
                        {
                            WriteObject(item);
                        }
                    }
                    //Makes the cmdlet stop properly when the pipeline is stopped by Select-Object -first X
                    catch (PipelineStoppedException e)
                    {
                        throw e;
                    }
                    catch 
                    {
                        WriteWarning("Could not scan folder contents of: " + directory);
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
