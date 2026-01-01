

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Studio.Models.Legacy;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Studio.Models
{

    #region Profile

    public class ProfileV2
    {
        public BattleTagV2 Battletag { get; set; }
        public string CustomName { get; set; }
        public string Email { get; set; }
        public string AvatarURL { get; set; }
        [JsonIgnore]
        public ProfileSnapshotV2 LatestSnapshot => Snapshots.OrderByDescending(s => s.Timestamp).FirstOrDefault(defaultValue: null);
        public List<ProfileSnapshotV2> Snapshots { get; set; } = [];

        // allows casting from old profile type
        public static implicit operator ProfileV2(ProfileV1 v)
        {
            var profile =  new ProfileV2()
            {
                AvatarURL = v.Avatar,
                Battletag = new BattleTagV2(v.Battletag.ToString()),
                Email = v.Email,
                CustomName = v.CustomId,
                Snapshots = []
            };
            if (v.RankedCareer != null)
            {
                profile.Snapshots.Add(new ProfileSnapshotV2()
                {
                    Timestamp = v.LastUpdate,
                    Tank = new Tank() { Rank = v.RankedCareer.Tank == null ? null : new RankV2(v.RankedCareer.Tank.CurrentRank.SkillRating) },
                    Damage = new Damage() { Rank = v.RankedCareer.Damage == null ? null : new RankV2(v.RankedCareer.Damage.CurrentRank.SkillRating) },
                    Support = new Support() { Rank = v.RankedCareer.Support == null ? null : new RankV2(v.RankedCareer.Support.CurrentRank.SkillRating) },
                });
            }
            return profile;
        }
    }

    public class ProfileSnapshotV2
    {
        public int Timestamp { get; set; }
        public Tank Tank { get; set; } = new Tank();
        public Support Support { get; set; } = new Support();
        public Damage Damage { get; set; } = new Damage();

        // allows indexing for role type
        public RoleV2 this[Roles role]
        {
            get
            {
                switch (role)
                {
                    case Roles.Tank:
                        return Tank;
                    case Roles.Damage:
                        return Damage;
                    case Roles.Support:
                        return Support;
                    default: return null;
                }
            }
        }
    }

    public class BattleTagV2
    {
        [JsonIgnore]
        public string FullTag => ToString();

        public string Username { get; set; }
        public string Tag { get; set; }

        [JsonConstructor]
        public BattleTagV2(string username, string tag)
        {
            Username = username;
            Tag = tag;
        }
        public BattleTagV2(string battletag)
        {
            string[] parts = battletag.Split('#');
            Username = parts[0];
            Tag = parts[1];
        }

        public override string ToString()
        {
            return $"{Username}#{Tag}";
        }
    }

    #endregion


    #region Roles

    // TODO: move selection logic out of model
    public abstract class RoleV2 : INotifyPropertyChanged
    {
        private bool _isSelectedForComparison;
        [JsonIgnore]
        public bool IsSelectedForComparison
        {
            get => _isSelectedForComparison;
            set
            {
                _isSelectedForComparison = value;
                OnPropertyChanged();
            }

        }
        [JsonIgnore]
        public abstract Roles Type { get; }


        public RankV2 Rank { get; set; }
        public Dictionary<StatisticType, float> Stats { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }

    public static class StatisticScaler
    {
        public static Dictionary<StatisticType, MappingRange> StatRanges = new()
        {
            {StatisticType.Damage, new MappingRange(3000, 12000) },
            {StatisticType.Healing, new MappingRange(3000, 12000) },
            {StatisticType.Elims, new MappingRange(5, 40) },
            {StatisticType.Deaths, new MappingRange(12, 2) },
        };

        // scales stat to 0-1 range
        public static Dictionary<StatisticType, float> ScaleStatistics(Dictionary<StatisticType, float> originalStats)
        {
            Dictionary<StatisticType, float> statCollection = new();
            foreach (var stat in originalStats.Keys)
            {
                if (StatRanges.ContainsKey(stat))
                    statCollection[stat] = StatRanges[stat].MapFloatToScalar(originalStats[stat]);
                else
                    statCollection[stat] = originalStats[stat];
            }
            return statCollection;
        }
    }

    public class MappingRange(double min, double max)
    {
        public double Min { get; set; } = min;
        public double Max { get; set; } = max;

        public float MapFloatToScalar(float value)
        {
            if (Max == Min)
                return 0f; // avoid div by 0
            float low = (float)Math.Min(Min, Max);
            float high = (float)Math.Max(Min, Max);

            var clamped = Math.Clamp(value, low, high);
            float scalar = (clamped - low) / (high - low);

            // flip if inverted
            if (Min > Max)
                scalar = 1f - scalar;

            return scalar;
        }
    }

    public class Tank : RoleV2
    {
        public override Roles Type => Roles.Tank;
    }

    public class Support : RoleV2
    {
        public override Roles Type => Roles.Support;
    }

    public class Damage : RoleV2
    {
        public override Roles Type => Roles.Damage;
    }

    #endregion

    #region Ranks

    public class RankV2
    {
        public int Tier { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Division Division { get; set; }

        [JsonIgnore]
        public int SkillRating { get; set; }

        [JsonConstructor]
        public RankV2(int tier, Division division)
        {
            Division = division;
            Tier = tier;
            SkillRating = CalculateSR(tier, division);
        }

        public RankV2(int sr)
        {
            SkillRating = sr;

            var rank = CalculateDiv(sr);

            Division = rank.division;
            Tier = rank.tier;
        }

        public static int CalculateSR(int tier, Division division)
        {
            int baseSr = (int)division;
            int remainder = (5 - tier) * 100;

            return baseSr + remainder;
        }

        public static (int tier, Division division) CalculateDiv(int sr)
        {
            if (sr < 0) sr = 0;
            if (sr >= 5000) sr = 4999;

            Division division = Division.Bronze;
            int remainder = 0;

            if (sr < 400)
                remainder = 1;
            foreach (Division div in Enum.GetValues(typeof(Division)))
            {
                if (sr < (int)div)
                    break;
                division = div;
            }

            int baseDivRank = (int)division;
            remainder = Math.Min(sr - (division == Division.Bronze ? 1000 : baseDivRank), 499);

            int tier = Math.Min(5, 5 - remainder / 100);

            return (tier, division);
        }

    }
    #endregion

    public enum Roles
    {
        Tank,
        Support,
        Damage
    }

    public enum Division
    {
        Bronze = 0,
        Silver = 1500,
        Gold = 2000,
        Platinum = 2500,
        Diamond = 3000,
        Master = 3500,
        Grandmaster = 4000,
        Champion = 4500
    }

    public enum StatisticType
    {
        Damage,
        Deaths,
        Elims,
        Healing,
        TimePlayed,
        Winrate,
    }

}
