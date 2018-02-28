using System;
using Cake.Core.IO;

namespace CakeExtensions.Models
{
    public class PackageSettings
    {
        public string Id { get; set; }

        public string Version { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string Description { get; set; }

        public string Summary { get; set; }

        public Uri ProjectUrl { get; set; }

        public string FilesSource { get; set; }

        public string FilesTarget { get; set; }

        public string BasePath { get; set; }

        public DirectoryPath OutputDirectory { get; set; }

        public bool Overwrite { get; set; }
    }
}
