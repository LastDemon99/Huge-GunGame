using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfinityScript;

namespace Huge_Gun_Game
{
    public class Huge_Gun_Game : BaseScript
    {
        private List<string> GunGameWepList = new List<string>();

        public Huge_Gun_Game()
        {
            GSCFunctions.MakeDvarServerInfo("didyouknow", "^2Huge GunGame script by LastDemon99");
            GSCFunctions.MakeDvarServerInfo("g_motd", "^2Huge GunGame script by LastDemon99");
            GSCFunctions.MakeDvarServerInfo("motd", "^2Huge GunGame script by LastDemon99");
            GSCFunctions.PreCacheItem("at4_mp");
            GSCFunctions.PreCacheItem("iw5_mk12spr_mp");

            LoadWepList();
            InfiniteStock();

            OnNotify("game_over", () => { AfterDelay(7500, () => { Utilities.ExecuteCommand("map_rotate"); }); });

            PlayerConnected += new Action<Entity>((player) =>
            {
                ServerWelcomeTittle(player, "Huge GunGame", new float[] { 0, 0, 1 });
                player.SetClientDvar("ui_mapname", "Huge GunGame");
                player.SetClientDvar("ui_gametype", "Huge GunGame");

                player.SetField("gunnum", 0);

                SpecialWepsInit(player);
                EquipmenStock(player);

                Init_Hud(player);
                OnSpawn(player);
            });
        }

        private void LoadWepList()
        {
            int[] wepClass = RandomNum(10, 0, 10);
            string[] weplist;

            for (int i = 0; i < wepClass.Length; i++)
            {
                switch (wepClass[i])
                {
                    case 0:
                        weplist = ListRandomizer(Pistols);
                        foreach (string wep in weplist)
                            GunGameWepList.Add(wep);
                        break;
                    case 1:
                        weplist = ListRandomizer(MachinePistols);
                        foreach (string wep in weplist)
                            GunGameWepList.Add(wep);
                        break;
                    case 2:
                        weplist = ListRandomizer(Shotguns);
                        foreach (string wep in weplist)
                            GunGameWepList.Add(wep);
                        break;
                    case 3:
                        weplist = ListRandomizer(SniperRiflesList);
                        foreach (string wep in weplist)
                            GunGameWepList.Add(wep);
                        break;
                    case 4:
                        weplist = ListRandomizer(AssaultRifles);
                        foreach (string wep in weplist)
                            GunGameWepList.Add(wep);
                        break;
                    case 5:
                        weplist = ListRandomizer(SubmachineGuns);
                        foreach (string wep in weplist)
                            GunGameWepList.Add(wep);
                        break;
                    case 6:
                        weplist = ListRandomizer(LightMachineGuns);
                        foreach (string wep in weplist)
                            GunGameWepList.Add(wep);
                        break;
                    case 7:
                        weplist = ListRandomizer(Launchers);
                        foreach (string wep in weplist)
                            GunGameWepList.Add(wep);
                        break;
                }
            }
            GunGameWepList.Add("javelin_mp");
        }
        private void Update_Hud(Entity player)
        {
            player.GetField<HudElem>("gunnumhud").SetText((player.GetField<int>("gunnum") + 1).ToString() + "/" + GunGameWepList.Count);
        }
        private void SetWinning(Entity player)
        {
            Notify("game_win", "winner");
            Notify("game_over");
            Notify("block_notifies");

            HudElem firstTitle = HudElem.CreateServerFontString(HudElem.Fonts.HudBig, 2f);
            firstTitle.SetPoint("TOP", "BOTTOM", 0, 2);
            firstTitle.SetText("^7" + player.Name + " has won the game");
            firstTitle.GlowColor = (new Vector3(0, 0, 1));
            firstTitle.GlowAlpha = 1f;
            firstTitle.SetPulseFX(150, 6000, 1000);

            foreach (Entity ent in Players)
            {
                ent.ClosePopUpMenu("");
                ent.CloseInGameMenu();
                ent.FreezeControls(true);

                if (ent == player)
                    ent.PlayLocalSound("winning_music");
                else
                    ent.PlayLocalSound("losing_music");
            }
        }
        private void InfiniteStock()
        {
            OnInterval(500, () =>
            {
                foreach (Entity player in BaseScript.Players)
                    if (!LetalEquipment.Contains(player.CurrentWeapon))
                        GSCFunctions.SetWeaponAmmoStock(player, player.CurrentWeapon, 45);
                return true;
            });
        }

        private void Init_Hud(Entity player)
        {
            HudElem gungamehud = HudElem.CreateFontString(player, HudElem.Fonts.Small, 1.6f);
            gungamehud.SetPoint("TOP LEFT", "TOP LEFT", 115, 5);
            gungamehud.SetText("Weapon:");
            gungamehud.Alpha = 0f;
            gungamehud.HideWhenInMenu = true;
            gungamehud.Foreground = true;
            gungamehud.HideWhenDead = true;

            HudElem wepnum = HudElem.CreateFontString(player, HudElem.Fonts.Small, 1.6f);
            wepnum.SetPoint("TOP LEFT", "TOP LEFT", 115, 22);
            wepnum.SetText((player.GetField<int>("gunnum") + 1).ToString() + "/" + GunGameWepList.Count);
            wepnum.Alpha = 0f;
            wepnum.HideWhenInMenu = true;
            wepnum.Foreground = true;
            wepnum.HideWhenDead = true;

            gungamehud.Alpha = 1f;
            wepnum.Alpha = 1f;

            player.SetField("gunnumhud", new Parameter(wepnum));
            player.SetField("gungamehud", new Parameter(gungamehud));
        }
        private void OnSpawn(Entity player)
        {
            DisableSelectClass(player);

            player.SpawnedPlayer += new Action(() =>
            {
                player.TakeAllWeapons();
                player.GiveWeapon(GunGameWepList[player.GetField<int>("gunnum")]);
                AfterDelay(350, () => { player.SwitchToWeaponImmediate(GunGameWepList[player.GetField<int>("gunnum")]); });
                GSCFunctions.DisableWeaponPickup(player);
                GSCFunctions.ClearPerks(player);
                GSCFunctions.SetPerk(player, "specialty_scavenger");
            });
        }

        private void EquipmenStock(Entity player)
        {
            player.OnNotify("weapon_fired", (self, weapon) =>
            {
                if (LetalEquipment.Contains(weapon.As<string>()))
                {
                    player.SetField("gg_savewep", weapon.As<string>());
                    player.TakeAllWeapons();
                    AfterDelay(1500, () =>
                    {
                        if (LetalEquipment.Contains(GunGameWepList[player.GetField<int>("gunnum")]))
                        {
                            player.GiveWeapon(player.GetField<string>("gg_savewep"));
                            player.SwitchToWeaponImmediate(player.GetField<string>("gg_savewep"));
                        }
                    });
                }
            });
        }
        private void DisableSelectClass(Entity player)
        {
            GSCFunctions.ClosePopUpMenu(player, "");
            GSCFunctions.CloseInGameMenu(player);
            player.Notify("menuresponse", "team_marinesopfor", "allies");
            player.OnNotify("joined_team", ent =>
            {
                AfterDelay(500, () => { ent.Notify("menuresponse", "changeclass", "class1"); });
            });
            player.OnNotify("menuresponse", (player2, menu, response) =>
            {
                if (menu.ToString().Equals("class") && response.ToString().Equals("changeclass_marines"))
                {
                    AfterDelay(100, () => { player.Notify("menuresponse", "changeclass", "back"); });
                }
            });
        }

        private void SpecialWepsInit(Entity player)
        {
            player.NotifyOnPlayerCommand("attack", "+attack");
            player.OnNotify("attack", self =>
            {
                if (player.CurrentWeapon == "stinger_mp")
                {
                    if (GSCFunctions.PlayerAds(player) >= 1f)
                    {
                        if (GSCFunctions.GetWeaponAmmoClip(player, player.CurrentWeapon) != 0)
                        {
                            Vector3 vector = GSCFunctions.AnglesToForward(GSCFunctions.GetPlayerAngles(player));
                            Vector3 dsa = new Vector3(vector.X * 1000000f, vector.Y * 1000000f, vector.Z * 1000000f);
                            GSCFunctions.MagicBullet("stinger_mp", GSCFunctions.GetTagOrigin(player, "tag_weapon_left"), dsa, self);
                            GSCFunctions.SetWeaponAmmoClip(player, player.CurrentWeapon, 0);
                        }
                    }
                }
            });
            player.OnNotify("weapon_fired", (self, weapon) =>
            {
                if (weapon.As<string>() == "uav_strike_marker_mp")
                {
                    Vector3 asd = GSCFunctions.AnglesToForward(GSCFunctions.GetPlayerAngles(player));
                    Vector3 dsa = new Vector3(asd.X * 1000000, asd.Y * 1000000, asd.Z * 1000000);
                    GSCFunctions.MagicBullet("ac130_40mm_mp", GSCFunctions.GetTagOrigin(player, "tag_weapon_left"), dsa, self);
                }

            });
        }

        private string[] ListRandomizer(string[] list)
        {
            int[] newAR = RandomNum(list.Length, 0, list.Length);
            List<string> newlist = new List<string>();
            for (int i = 0; i < newAR.Length; i++)
                newlist.Add(list[newAR[i]]);

            string[] _list = new string[newlist.Count];
            for (int i = 0; i < newlist.Count; i++)
                _list[i] = newlist[i];

            return _list;
        }
        private int[] RandomNum(int size, int Min, int Max)
        {
            int[] UniqueArray = new int[size];
            Random rnd = new Random();
            int Random;

            for (int i = 0; i < size; i++)
            {
                Random = rnd.Next(Min, Max);

                for (int j = i; j >= 0; j--)
                {

                    if (UniqueArray[j] == Random)
                    { Random = rnd.Next(Min, Max); j = i; }

                }
                UniqueArray[i] = Random;
            }
            return UniqueArray;
        }

        public static void ServerWelcomeTittle(Entity player, string tittle, float[] rgb)
        {
            player.SetField("welcome", 0);
            player.SpawnedPlayer += new Action(() =>
            {
                if (player.GetField<int>("welcome") == 0)
                {
                    HudElem serverWelcome = HudElem.CreateFontString(player, HudElem.Fonts.HudBig, 1f);
                    serverWelcome.SetPoint("TOPCENTER", "TOPCENTER", 0, 165);
                    serverWelcome.SetText(tittle);
                    serverWelcome.GlowColor = (new Vector3(rgb[0], rgb[1], rgb[2]));
                    serverWelcome.GlowAlpha = 1f;
                    serverWelcome.SetPulseFX(150, 4700, 700);
                    player.SetField("welcome", 1);

                    AfterDelay(5000, () => { serverWelcome.Destroy(); });
                }
            });
        }

        public override void OnPlayerKilled(Entity player, Entity inflictor, Entity attacker, int damage, string mod, string weapon, Vector3 dir, string hitLoc)
        {
            if (player != attacker && mod == "MOD_FALLING" || (mod == "MOD_MELEE" && weapon != "riotshield_mp"))
            {
                player.PlayLocalSound("mp_war_objective_lost");
                if (player.GetField<int>("gunnum") > 0)
                    player.SetField("gunnum", player.GetField<int>("gunnum") - 1);

                Update_Hud(player);
            }

            if ((mod == "MOD_PISTOL_BULLET") || (mod == "MOD_RIFLE_BULLET") || (mod == "MOD_HEAD_SHOT") || (mod == "MOD_PROJECTILE") || (mod == "MOD_PROJECTILE_SPLASH") ||
            (mod == "MOD_IMPACT") || (mod == "MOD_GRENADE") || (mod == "MOD_GRENADE_SPLASH") || (mod == "MOD_MELEE" && (weapon == "riotshield_mp" || weapon == "iw5_riotshieldjugg_mp")))
            {
                if (damage >= player.Health && weapon != attacker.CurrentWeapon)
                    return;

                attacker.PlayLocalSound("mp_enemy_obj_captured");
                attacker.SetField("gunnum", attacker.GetField<int>("gunnum") + 1);
                Update_Hud(attacker);

                if (attacker.GetField<int>("gunnum") < GunGameWepList.Count)
                {
                    attacker.TakeAllWeapons();
                    attacker.GiveWeapon(GunGameWepList[attacker.GetField<int>("gunnum")]);
                    attacker.SwitchToWeaponImmediate(GunGameWepList[attacker.GetField<int>("gunnum")]);
                }
                else if (attacker.GetField<int>("gunnum") >= GunGameWepList.Count)
                    SetWinning(attacker);
            }
        }

        #region Data
        public static string[] AssaultRifles = {  "iw5_m4_mp",
                                           "iw5_ak47_mp",
                                           "iw5_m16_mp",
                                           "iw5_fad_mp",
                                           "iw5_acr_mp",
                                           "iw5_type95_mp",
                                           "iw5_mk14_mp",
                                           "iw5_scar_mp",
                                           "iw5_g36c_mp",
                                           "iw5_cm901_mp",
                                           "iw5_mk12spr_mp" };

        public static string[] SubmachineGuns = {  "iw5_mp7_mp",
                                           "iw5_m9_mp",
                                           "iw5_p90_mp",
                                           "iw5_pp90m1_mp",
                                           "iw5_ump45_mp"};

        public static string[] SniperRiflesList = {  "iw5_barrett_mp_barrettscopevz",
                                           "iw5_rsass_mp_rsassscopevz",
                                           "iw5_dragunov_mp_dragunovscopevz",
                                           "iw5_msr_mp_msrscopevz",
                                           "iw5_l96a1_mp_l96a1scopevz",
                                           "iw5_as50_mp_as50scopevz"};

        public static string[] Shotguns = {  "iw5_ksg_mp",
                                           "iw5_1887_mp",
                                           "iw5_striker_mp",
                                           "iw5_aa12_mp",
                                           "iw5_usas12_mp",
                                           "iw5_spas12_mp"};

        public static string[] LightMachineGuns = {  "iw5_m60_mp",
                                           "iw5_mk46_mp",
                                           "iw5_pecheneg_mp",
                                           "iw5_sa80_mp",
                                           "iw5_mg36_mp"};

        public static string[] Pistols = {  "iw5_44magnum_mp",
                                           "iw5_usp45_mp",
                                           "iw5_deserteagle_mp",
                                           "iw5_mp412_mp",
                                           "iw5_p99_mp",
                                           "iw5_fnfiveseven_mp"};

        public static string[] MachinePistols = {  "iw5_g18_mp",
                                           "iw5_fmg9_mp",
                                           "iw5_mp9_mp",
                                           "iw5_skorpion_mp"};

        public static string[] Launchers = {  "m320_mp",
                                           "rpg_mp",
                                           "iw5_smaw_mp",
                                           "stinger_mp",
                                           "xm25_mp",
                                           "at4_mp" };

        public static string[] LetalEquipment = { "semtex_mp",
                                           "throwingknife_mp",
                                           "claymore_mp",
                                           "c4_mp",
                                           "bouncingbetty_mp"};

        public static string[] SpecialWeps = { "iw5_riotshieldjugg_mp",
                                           "uav_strike_marker_mp",
                                           "defaultweapon_mp",
                                           "at4_mp",
                                           "iw5_mk12spr_mp_acog",
                                           "iw5_m60jugg_mp_acog_xmags_camo03",
                                           "gl_mp"};

        #endregion
    }
}
