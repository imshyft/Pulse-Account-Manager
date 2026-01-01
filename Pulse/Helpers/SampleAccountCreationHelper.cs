using Studio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Studio.Helpers
{
    public static class SampleAccountCreationHelper
    {
        public static List<ProfileSnapshotV2> GenerateRandomSnapshots(Random rnd, int count)
        {
            List<ProfileSnapshotV2> snapshots = [];
            int tankRank = rnd.Next(500, 4500);
            int dmgRank = rnd.Next(500, 4500);
            int suppRank = rnd.Next(500, 4500);
            //_randomDates = new int[count];
            int date = 1726641630;
            for (int j = 0; j < count; j++)
            {
                //_randomDates[j] = date;
                bool noTank = rnd.NextDouble() < 0.3;
                bool noDmg = rnd.NextDouble() < 0.3;
                bool noSupp = rnd.NextDouble() < 0.3;

                Tank tank = new()
                {
                    Rank = noTank ? null : new RankV2(RandomWalkRank(rnd, ref tankRank)),
                    Stats = noTank ? null : GenerateRandomStats(rnd)
                };

                Support supp = new()
                {
                    Rank = noSupp ? null : new RankV2(RandomWalkRank(rnd, ref suppRank)),
                    Stats = noSupp ? null : GenerateRandomStats(rnd)
                };

                Damage dmg = new()
                {
                    Rank = noDmg ? null : new RankV2(RandomWalkRank(rnd, ref dmgRank)),
                    Stats = noDmg ? null : GenerateRandomStats(rnd)
                };

                snapshots.Add(new ProfileSnapshotV2()
                {
                    Timestamp = date,
                    Tank = tank,
                    Damage = dmg,
                    Support = supp
                });

                date += rnd.Next(80000, 160000);
            }

            return snapshots;
        }

        public static Dictionary<StatisticType, float> GenerateRandomStats(Random rnd)
        {
            var dict = new Dictionary<StatisticType, float>();
            dict[StatisticType.Damage] = rnd.Next(3000, 12000);
            dict[StatisticType.Healing] = rnd.Next(3000, 12000);
            dict[StatisticType.TimePlayed] = rnd.Next(300, 900);
            dict[StatisticType.Elims] = rnd.Next(5, 40);
            dict[StatisticType.Deaths] = rnd.Next(2, 12);
            dict[StatisticType.Winrate] = (float)rnd.NextDouble();

            return dict;

        }

        public static int RandomWalkRank(Random rnd, ref int rank)
        {
            rank = Math.Max(1, rank + rnd.Next(-2, 2) * 100);
            return rank;
        }
    }
}
