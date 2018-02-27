using System.Collections.Generic;
using Cake.Core.IO;

namespace CakeExtensions.Models
{
    public class OctoPushSettings
    {
        public string ApiKey { get; set; }

        public string ServerUrl { get; set; }

        public bool ReplaceExisting { get; set; }

        public List<FilePath> Packages { get; set; }
    }
}
