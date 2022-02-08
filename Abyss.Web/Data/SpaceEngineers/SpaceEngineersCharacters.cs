namespace Abyss.Web.Data.SpaceEngineers;

public class SpaceEngineersCharacters
{
    public List<Character> Characters { get; set; }

    public class Character
    {
        public string DisplayName { get; set; }
        public long EntityId { get; set; }
        public decimal Mass { get; set; }
        public PositionData Position { get; set; }
        public decimal LinearSpeed { get; set; }

        public class PositionData
        {
            public decimal X { get; set; }
            public decimal Y { get; set; }
            public decimal Z { get; set; }
        }
    }
}
