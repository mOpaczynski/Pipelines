namespace CakeExtensions.Models
{
    public class OctoCreateReleaseSettings : OctoPushSettings
    {
        public string OctopusProjectName { get; set; }

        public string ReleaseNumber { get; set; }

        public string DefaultPackageVersion { get; set; }

        public string TargetEnvironment { get; set; }
    }
}
