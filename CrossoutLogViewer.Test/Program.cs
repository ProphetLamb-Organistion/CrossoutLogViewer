using CrossoutLogView.Database;
using CrossoutLogView.Database.Collection;
using CrossoutLogView.Database.Connection;
using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.Log;

using System;
using System.Linq;

namespace CrossoutLogView.Test
{
    class Program
    {
        static void Main()
        {
            //statCon.Dispose();
            //Test_Request();
            //Test_Connections();
            //Test_Upload();
            Test_Service();
            //Test_Dataprovider();
            //Test_DisplayStringFactory();
            Console.ReadLine();
        }

        static void Test_Upload()
        {
            var uploader = new LogUploader(@"C:\Users\Normiuser\Documents\My Games\Crossout\logs\2020.05.15 14.38.54\combat.log");
            uploader.Parse();
            uploader.Upload();
        }

        static void Test_Request()
        {
            using var logCon = new LogConnection();
            logCon.Open();
            var logs = logCon.RequestAll(DateTime.Now.AddDays(-3).Ticks);
            logCon.Dispose();
            using var statCon = new StatisticsConnection();
            statCon.Open();
            var user = statCon.RequestUser("InkyBusiness");
            var game = statCon.RequestGame(new DateTime(637239526409140000));
        }

        static void Test_Connections()
        {
            using var logCon = new LogConnection();
            using var statCon = new StatisticsConnection();
            var dmg = new Damage[]
            {
                new Damage(1214, "vic1", "atk1", "weap1", 666.66666666, DamageFlag.None),
                new Damage(1234, "vic1", "atk1", "weap1", 420.42, DamageFlag.HUD_IMPORTANT),
            };
            var kills = new Killing[]
            {
                new Killing(1235, "vic1", "atk1")
            };
            var assists = new KillAssist[]
            {
                new KillAssist(1235, "atk1", "weap1", 0.0, 420.42 + 666.66666666, DamageFlag.HUD_IMPORTANT),
                new KillAssist(1235, "atk2", "weap1", 0.0, 2.6,  DamageFlag.None)
            };
            logCon.Open();
            logCon.Insert(dmg);
            logCon.Insert(kills);
            logCon.Insert(assists);
            logCon.Close();
        }

        static void Test_Service()
        {
            var flFTest = new ControlService();
            flFTest.Start();
        }
        static void Test_Dataprovider()
        {
            var me = DataProvider.GetUser(Settings.Current.MyUserID);
            DataProvider.CompleteUser(Settings.Current.MyUserID);
            var typhoon = DataProvider.GetWeapon("CarPart_Gun_BigCannon_EX_Relic");
        }

        static void Test_DisplayStringFactory()
        {
            var typh = DisplayStringFactory.AssetName("CarPart_Gun_Flamethrower_light");
            var map = DisplayStringFactory.MapName("sand_valley");
        }
    }
}
