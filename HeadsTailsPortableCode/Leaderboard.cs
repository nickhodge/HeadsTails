using Newtonsoft.Json;

namespace HeadsTailsPortableCode
{
    public class Leaderboard
    {
        public int Id;

        [JsonProperty(PropertyName = "username")]
        public string userName { get; set; }
        [JsonProperty(PropertyName = "groupname")]
        public string groupName { get; set; }
        [JsonProperty(PropertyName = "channelw8notifications")]
        public string channelW8notifications { get; set; }
        [JsonProperty(PropertyName = "channelwp8notifications")]
        public string channelWP8notifications { get; set; }
        [JsonProperty(PropertyName = "highscore")]
        public int highScore { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1}", userName, highScore);
        }

    }
}
