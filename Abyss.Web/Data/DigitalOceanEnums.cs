namespace Abyss.Web.Data
{
    public class DigitalOceanEnums
    {
        public class ActionStatus
        {
            public const string InProgress = "in-progress";
            public const string Completed = "completed";
            public const string Errored = "errored";
        }

        public class DropletStatus
        {
            public const string New = "new";
            public const string Active = "active";
            public const string Off = "off";
            public const string Archive = "archive";
        }

        public class ImageType
        {
            public const string Snapshot = "snapshot";
            public const string Backup = "backup";
        }
    }
}
