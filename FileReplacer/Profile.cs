using System.Collections.Generic;

namespace FileReplacer
{
    public class Profile
    {
        public bool IsBackupEnabled { get; set; }

        public string BackupLocation{ get; set; }

        public List<string> SourceLocations { get; set; }

        public List<string> DestinationLocations{ get; set; }

        public Profile(bool isBackupEnabled, string backupLocation, List<string> sourceLocations, List<string> destinationLocations)
        {
            IsBackupEnabled = isBackupEnabled;
            BackupLocation = backupLocation;
            SourceLocations = sourceLocations;
            DestinationLocations = destinationLocations;
        }

        public Profile()
        {
            DestinationLocations = new List<string>();
            SourceLocations = new List<string>();
        }
    }
}
