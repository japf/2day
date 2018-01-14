using Windows.ApplicationModel;

namespace Chartreuse.Today.Core.Universal.Tools
{
    public static class ApplicationVersion
    {
        public static string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        public static int GetAppNumericVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return version.Major*1000 + version.Minor*100 + version.Build*10 + version.Revision;
        }

        public static int GetRevisionVersion()
        {
            return Package.Current.Id.Version.Revision;
        }
    }
}