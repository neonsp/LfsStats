using System;
using NUnit.Framework;
using LFSStatistics;

namespace LFSStats.Tests
{
    [TestFixture]
    public class LFSWorldTests
    {
        // Set your pubStatIDkey here or via environment variable LFSW_KEY
        private static string GetKey()
        {
            string key = Environment.GetEnvironmentVariable("LFSW_KEY");
            if (string.IsNullOrEmpty(key))
                Assert.Ignore("LFSW_KEY environment variable not set — skipping integration test");
            return key;
        }

        [Test]
        public void GetWR_WithoutInitialization_ReturnsNull()
        {
            var result = LFSWorld.GetWR("BL2", "FBM");
            Assert.IsNull(result);
        }

        [Test]
        public void Initialize_WithValidKey_ReturnsTrue()
        {
            string key = GetKey();
            bool result = LFSWorld.Initialize(key);
            Assert.IsTrue(result, "LFSWorld should initialize successfully");
        }

        [Test]
        public void GetWR_BL2_FBM_ReturnsValidData()
        {
            string key = GetKey();
            LFSWorld.Initialize(key);

            var wr = LFSWorld.GetWR("BL2", "FBM");

            Assert.IsNotNull(wr, "WR for BL2/FBM should exist");
            Assert.AreEqual("BL2", wr.Track, "Track should be BL2");
            Assert.AreEqual("FBM", wr.CarName, "Car should be FBM");
            Assert.Greater(wr.WrTime, 0, "WR time should be positive");
            Assert.IsNotEmpty(wr.RacerName, "Racer name should not be empty");
            Assert.Greater(wr.IdWr, 0, "idWr should be positive");

            // Print results for inspection
            Console.WriteLine($"Track:  {wr.Track}");
            Console.WriteLine($"Car:    {wr.CarName}");
            Console.WriteLine($"Time:   {wr.WrTime} ms");
            Console.WriteLine($"Racer:  {wr.RacerName}");
            Console.WriteLine($"id_wr:  {wr.IdWr}");
            Console.WriteLine($"Splits: {string.Join(", ", wr.Split)}");
            Console.WriteLine($"Sectors:{string.Join(", ", wr.Sector)}");
            Console.WriteLine($"Download: https://www.lfsworld.net/get_spr.php?file={wr.IdWr}");
        }
    }
}
