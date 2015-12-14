using System;
using System.Linq;
using SharpDX;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace Sense_Elise
{
    class Program
    {
        private static Menu Option;
        private static Obj_AI_Hero Player;
        private static string championName = "Elise";
        private static Orbwalking.Orbwalker _Orbwalker;
        private static Spell _Q, _W, _E, _R, _sQ, _sW, _sE;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.ChampionName != championName) return;

            Game.PrintChat("<font color='#5CD1E5'>[ Sense Elise ] </font><font color='#FF0000'> Thank you for use this assembly \n<font color='#1DDB16'>if you have any feedback, let me know that.</font>");

            _Q = new Spell(SpellSlot.Q, 625f);
            _W = new Spell(SpellSlot.W, 950f);
            _E = new Spell(SpellSlot.E, 1075f);
            _R = new Spell(SpellSlot.R);
            _sQ = new Spell(SpellSlot.Q, 475f);
            _sW = new Spell(SpellSlot.W);
            _sE = new Spell(SpellSlot.E, 750f);

            SetMenu();

            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }
        private static void OnUpdate(EventArgs args)
        {
            var JungleMinions = MinionManager.GetMinions(Player.ServerPosition, _Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            switch (_Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                    if (JungleMinions.Count > 0)
                        JungleClear();
                    else
                        LaneClear();
                    break;

                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;

                case Orbwalking.OrbwalkingMode.GankingComboMode:
                    GankingCombo();
                    break;

                case Orbwalking.OrbwalkingMode.HotKey:
                    HotKey();
                    break;
            }
        }

        private static void HotKey()
        {
            if (!Human()) return;

            var target = TargetSelector.GetTarget(_E.Range, TargetSelector.DamageType.Magical);
            var Eprediction = _E.GetPrediction(target);

            if (_E.IsReady() && Option.Item("HotKey E").GetValue<bool>() && target.Distance(target) <= _E.Range)
            {
                switch (Eprediction.Hitchance)
                {
                    case HitChance.Medium:
                    case HitChance.High:
                    case HitChance.VeryHigh:
                        _E.Cast(Eprediction.CastPosition);
                        break;
                }
            }
        }
        private static void Harass()
        {
            if (Human() && Player.ManaPercent <= Option.Item("Harass Mana").GetValue<Slider>().Value) return;
            var target = TargetSelector.GetTarget(_W.Range, TargetSelector.DamageType.Magical);
            var Qtarget = TargetSelector.GetTarget(_Q.Range, TargetSelector.DamageType.Magical);

            if (target != null)
            {
                if (Option.Item("Harass Human W").GetValue<bool>() && _W.IsReady() && target.Distance(target) <= _W.Range)
                    _W.Cast(target);

                if (Option.Item("Harass Human Q").GetValue<bool>() && _Q.IsReady() && target.Distance(Qtarget) <= _Q.Range)
                    _Q.Cast(Qtarget);
            }
        }

        private static void LaneClear()
        {
            var WMinions = MinionManager.GetMinions(Player.ServerPosition, _W.Range).FirstOrDefault();
            var QMinions = MinionManager.GetMinions(Player.ServerPosition, _Q.Range).FirstOrDefault();
            var sQMinions = MinionManager.GetMinions(Player.ServerPosition, _sQ.Range).FirstOrDefault();

            if (Human())
            {
                if (Player.ManaPercent <= Option.Item("LaneClear Mana").GetValue<Slider>().Value) return;
                else
                {
                    if (Option.Item("Lane Human W").GetValue<bool>() && _W.IsReady() && WMinions.Distance(WMinions) <= _W.Range)
                        _W.Cast(WMinions);

                    if (Option.Item("Lane Human Q").GetValue<bool>() && _Q.IsReady() && QMinions.Distance(QMinions) <= _Q.Range)
                        _Q.Cast(QMinions);
                }
            }
            else
            {
                if (Option.Item("Lane Spider Q").GetValue<bool>() && _sQ.IsReady() && sQMinions.Distance(sQMinions) <= _sQ.Range)
                    _sQ.Cast(sQMinions);
                if (Option.Item("Lane Spider W").GetValue<bool>() && _sW.IsReady() && sQMinions.Distance(sQMinions) <= 125)
                    _sW.Cast();
            }
        }

        private static void JungleClear()
        {
            var JungleMinions = MinionManager.GetMinions(Player.ServerPosition, _W.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            foreach (var minion in JungleMinions)
            {
                if (Human())
                {
                    if (Player.ManaPercent <= Option.Item("LaneClear Mana").GetValue<Slider>().Value) return;
                    else
                    {
                        if (_Q.IsReady() && minion.IsValidTarget() && Option.Item("JungleClearMenu Human Q").GetValue<bool>() && Player.Distance(minion) <= _Q.Range)
                            _Q.Cast(minion);

                        if (_W.IsReady() && minion.IsValidTarget() && Option.Item("JungleClearMenu Human W").GetValue<bool>() && Player.Distance(minion) <= _W.Range)
                            _W.Cast(minion);

                        if (!_W.IsReady() && !_Q.IsReady() && Option.Item("JungleClearMenu R").GetValue<bool>())
                            _R.Cast();
                    }
                }
                else
                {
                    if (_sQ.IsReady() && Option.Item("JungleClearMenu Spider Q").GetValue<bool>() && Player.Distance(minion) <= _sQ.Range)
                        _sQ.Cast(minion);

                    if (_sW.IsReady() && Option.Item("JungleClearMenu Spider W").GetValue<bool>() && Player.Distance(minion) <= 125)
                        _sW.Cast();

                }
            }

        }

        private static void Combo()
        {
            var Qtarget = TargetSelector.GetTarget(_Q.Range, TargetSelector.DamageType.Magical);
            var Wtarget = TargetSelector.GetTarget(_W.Range, TargetSelector.DamageType.Magical);
            var Etarget = TargetSelector.GetTarget(_E.Range, TargetSelector.DamageType.Magical);
            var sQtarget = TargetSelector.GetTarget(_sQ.Range, TargetSelector.DamageType.Magical);
            var sWtarget = TargetSelector.GetTarget(_sW.Range, TargetSelector.DamageType.Magical);
            var sEtarget = TargetSelector.GetTarget(_sW.Range, TargetSelector.DamageType.Magical);
            if (Etarget != null) return;

            if (Human())
            {
                var Eprediction = _E.GetPrediction(Etarget);
                var Wprediction = _W.GetPrediction(Wtarget);

                if (Option.Item("Combo Human E").GetValue<bool>() && _E.IsReady() && Etarget.Distance(Wtarget) <= _E.Range)
                {
                    switch (Eprediction.Hitchance)
                    {
                        case HitChance.Medium:
                        case HitChance.High:
                        case HitChance.VeryHigh:
                        case HitChance.Immobile:
                            _E.Cast(Etarget);
                            break;
                    }
                }

                if (Option.Item("Combo Human W").GetValue<bool>() && _W.IsReady() && Wtarget.Distance(Wtarget) <= _W.Range)
                {
                    switch (Wprediction.Hitchance)
                    {
                        case HitChance.Low:
                        case HitChance.Medium:
                        case HitChance.High:
                        case HitChance.VeryHigh:
                        case HitChance.Immobile:
                            _W.Cast(Wtarget);
                            break;
                    }
                }

                if (Option.Item("Combo Human Q").GetValue<bool>() && _Q.IsReady() && Qtarget.Distance(Qtarget) <= _Q.Range)
                    _Q.Cast(Qtarget);

                if (!_Q.IsReady() && !_W.IsReady() && !_E.IsReady() && Option.Item("Combo R").GetValue<bool>())
                    _R.Cast();
            }
            else
            {
                if (Option.Item("Combo Spider Q").GetValue<bool>() && _sQ.IsReady() && sQtarget.Distance(sQtarget) <= _sQ.Range)
                    _sQ.Cast(sQtarget);
                if (Option.Item("Combo Spider W").GetValue<bool>() && _sW.IsReady() && sQtarget.Distance(sQtarget) <= 125)
                    _sW.Cast();
                if (Option.Item("Combo Spider E").GetValue<bool>() && _sE.IsReady() && Player.Distance(sEtarget) <= _sE.Range && Player.Distance(sEtarget) > _sQ.Range)
                    _sE.Cast(sEtarget);
            }

        }

        private static void GankingCombo()
        {
            var Qtarget = TargetSelector.GetTarget(_Q.Range, TargetSelector.DamageType.Magical);
            var Wtarget = TargetSelector.GetTarget(_W.Range, TargetSelector.DamageType.Magical);
            var Etarget = TargetSelector.GetTarget(_E.Range, TargetSelector.DamageType.Magical);
            var sQtarget = TargetSelector.GetTarget(_sQ.Range, TargetSelector.DamageType.Magical);
            var sWtarget = TargetSelector.GetTarget(_sW.Range, TargetSelector.DamageType.Magical);
            var sEtarget = TargetSelector.GetTarget(_sW.Range, TargetSelector.DamageType.Magical);


            if (Etarget != null) return;

            if (Human())
            {
                var Eprediction = _E.GetPrediction(Etarget);
                var Wprediction = _W.GetPrediction(Wtarget);
                if (_R.IsReady() && Option.Item("R").GetValue<bool>())
                    _R.Cast();

                if (!Human())
                {
                    if (_sE.IsReady() && Player.Distance(sEtarget) <= _sE.Range && Player.Distance(sEtarget) > _sQ.Range && Option.Item("GankingCombo Spider E").GetValue<bool>())
                        _sE.Cast(sEtarget);
                    if (_sQ.IsReady() && Option.Item("GankingCombo Spider Q").GetValue<bool>())
                        _sQ.Cast(sQtarget);
                    if (_sW.IsReady() && Option.Item("GankingCombo Spider W").GetValue<bool>())
                        _sW.Cast();
                    if (!_sW.IsReady() && _sQ.IsReady() && _R.IsReady())
                        _R.Cast();
                }

                if (Option.Item("GankingCombo Human E").GetValue<bool>() && _E.IsReady() && Etarget.Distance(Etarget) <= _E.Range)
                {
                    switch (Eprediction.Hitchance)
                    {
                        case HitChance.Low:
                        case HitChance.Medium:
                        case HitChance.High:
                        case HitChance.VeryHigh:
                        case HitChance.Immobile:
                            _E.Cast(Etarget);
                            break;
                    }

                }

                if (Option.Item("GankingCombo Human W").GetValue<bool>() && _W.IsReady() && Wtarget.Distance(Wtarget) <= _W.Range)
                    switch (Eprediction.Hitchance)
                    {
                        case HitChance.Low:
                        case HitChance.Medium:
                        case HitChance.High:
                        case HitChance.VeryHigh:
                        case HitChance.Immobile:
                            _E.Cast(Wtarget);
                            break;
                    }

                if (Option.Item("GankingCombo Human Q").GetValue<bool>() && _Q.IsReady() && Qtarget.Distance(Qtarget) <= _Q.Range)
                    _Q.Cast(Qtarget);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            var elise = Drawing.WorldToScreen(Player.Position);

            if (Human())
            {
                if (Option.Item("Drawing Q Human").GetValue<bool>())
                    Render.Circle.DrawCircle(Player.Position, _Q.Range, Color.Green, 1);

                if (Option.Item("Drawing W Human").GetValue<bool>())
                    Render.Circle.DrawCircle(Player.Position, _W.Range, Color.Yellow, 1);

                if (Option.Item("Drawing E Human").GetValue<bool>())
                    Render.Circle.DrawCircle(Player.Position, _E.Range, Color.YellowGreen, 1);

            }

            else
            {
                if (Option.Item("Drawing Q Spider").GetValue<bool>())
                    Render.Circle.DrawCircle(Player.Position, _sQ.Range, Color.White, 1);

                if (Option.Item("Drawing E Spider").GetValue<bool>())

                    Render.Circle.DrawCircle(Player.Position, _sE.Range, Color.Red, 1);

                if (Option.Item("Drawing E Spider").GetValue<bool>())
                    Render.Circle.DrawCircle(Player.Position, _Q.Range, Color.Blue, 1);
            }


            //  if (Option.Item("ComboDamage").GetValue<bool>())

        }

        private static void SetMenu()
        {
            Option = new Menu("Sense Elise", "Sense_Elise", true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Option.AddSubMenu(targetSelectorMenu);

            Option.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            _Orbwalker = new Orbwalking.Orbwalker(Option.SubMenu("Orbwalker"));

            var HarassMenu = new Menu("Harass", "Harass");
            {
                HarassMenu.AddItem((new MenuItem("Harass Human Q", "Use Q [ Human ]").SetValue(true)));
                HarassMenu.AddItem((new MenuItem("Harass Human W", "Use W [ Human ]").SetValue(true)));
                HarassMenu.AddItem((new MenuItem("Harass Mana", "Harass Limit Mana (%)").SetValue(new Slider(60))));
            }

            var LaneClearMenu = new Menu("LaneClear", "LaneClear");
            {
                LaneClearMenu.AddItem((new MenuItem("Lane Human Q", "Use Q [ Human ]").SetValue(true)));
                LaneClearMenu.AddItem((new MenuItem("Lane Human W", "Use W [ Human ]").SetValue(true)));
                LaneClearMenu.AddItem((new MenuItem("Lane Spider Q", "Use Q [ Spider ]").SetValue(true)));
                LaneClearMenu.AddItem((new MenuItem("Lane Spider W", "Use W [ Spider ]").SetValue(true)));
                LaneClearMenu.AddItem((new MenuItem("LaneClear Mana", "Lane Clear Limit Mana (%)").SetValue(new Slider(60))));
            }

            var JungleClearMenu = new Menu("JungleClear", "JungleClear");
            {
                JungleClearMenu.AddItem((new MenuItem("JungleClearMenu Human Q", "Use Q [ Human ]").SetValue(true)));
                JungleClearMenu.AddItem((new MenuItem("JungleClearMenu Human W", "Use W [ Human ]").SetValue(true)));
                JungleClearMenu.AddItem((new MenuItem("JungleClearMenu Spider Q", "Use Q [ Spider ]").SetValue(true)));
                JungleClearMenu.AddItem((new MenuItem("JungleClearMenu Spider W", "Use W [ Spider ]").SetValue(true)));
                JungleClearMenu.AddItem((new MenuItem("JungleClearMenu R", "Auto Switch Form").SetValue(true)));
                JungleClearMenu.AddItem((new MenuItem("JungleClear Mana", " Jungle Clear Limit Mana (%)").SetValue(new Slider(0))));
            }

            var ComboMenu = new Menu("Combo", "Combo");
            {
                ComboMenu.AddItem((new MenuItem("Combo Human Q", "Use Q [ Human ]").SetValue(true)));
                ComboMenu.AddItem((new MenuItem("Combo Human W", "Use W [ Human ]").SetValue(true)));
                ComboMenu.AddItem((new MenuItem("Combo Human E", "Use E [ Human ]").SetValue(true)));
                ComboMenu.AddItem((new MenuItem("Combo Spider Q", "Use Q [ Spider ]").SetValue(true)));
                ComboMenu.AddItem((new MenuItem("Combo Spider W", "Use W [ Spider ]").SetValue(true)));
                ComboMenu.AddItem((new MenuItem("Combo Spider E", "Use E [ Spider ]").SetValue(true)));
                ComboMenu.AddItem((new MenuItem("Combo R", "Auto Switch Form").SetValue(true)));
            }

            var GankingComboMenu = new Menu("GankingCombo", "GankingCombo");
            {
                GankingComboMenu.AddItem((new MenuItem("GankingCombo Human Q", " Use Q [ Human ]").SetValue(true)));
                GankingComboMenu.AddItem((new MenuItem("GankingCombo Human W", " Use W [ Human ]").SetValue(true)));
                GankingComboMenu.AddItem((new MenuItem("GankingCombo Human E", " Use E [ Human ]").SetValue(true)));
                GankingComboMenu.AddItem((new MenuItem("GankingCombo Spider Q", " Use Q [ Spider ]").SetValue(true)));
                GankingComboMenu.AddItem((new MenuItem("GankingCombo Spider W", " Use W [ Spider ]").SetValue(true)));
                GankingComboMenu.AddItem((new MenuItem("GankingCombo Spider E", " Use E [ Spider ]").SetValue(true)));
                GankingComboMenu.AddItem((new MenuItem("R", "Auto Switch Form").SetValue(true)));
                GankingComboMenu.AddItem((new MenuItem("GankingComboMode", "GankingCombo Active").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press))));
            }

            var HotKeyMenu = new Menu("E Hotkey", "E Hotkey");
            {
                HotKeyMenu.AddItem((new MenuItem("HotKey E", "HotKey E").SetValue(true)));
                //    HotKeyMenu.AddItem((new MenuItem("Flash Use E", "If enemy HP (%) Flash + Use E").SetValue(new Slider(20))));
                HotKeyMenu.AddItem((new MenuItem("HotKey", " HotKey Active").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press))));
            }
            /*
            var MiscMenu = new Menu("Misc", "Misc");
            {
                MiscMenu.AddItem((new MenuItem("Auto Smite Red", "Auto Smite Red").SetValue(true)));
                MiscMenu.AddItem((new MenuItem("Auto Smite Blue", "Auto Smite Blue").SetValue(true)));
                MiscMenu.AddItem((new MenuItem("Auto Smite Baron", "Auto Smite Baron").SetValue(true)));
                MiscMenu.AddItem((new MenuItem("Auto Smite Dragon", "Auto Smite Dragon").SetValue(true)));
                MiscMenu.AddItem((new MenuItem("Auto Smite Enemy", "Auto Smite Enemy").SetValue(true)));
            }
            */
            var DrawingMenu = new Menu("Drawing", "Drawing");
            {
                DrawingMenu.AddItem((new MenuItem("Drawing Q Human", "Draw Q [ Human ]").SetValue(true)));
                DrawingMenu.AddItem((new MenuItem("Drawing W Human", "Draw W [ Human ]").SetValue(true)));
                DrawingMenu.AddItem((new MenuItem("Drawing E Human", "Draw E [ Human ]").SetValue(true)));
                DrawingMenu.AddItem((new MenuItem("Drawing Q Spider", "Draw Q [ Spider ]").SetValue(true)));
                DrawingMenu.AddItem((new MenuItem("Drawing E Spider", "Draw E [ Spider ]").SetValue(true)));
                // DrawingMenu.AddItem((new MenuItem("ComboDamage", "Combo Damage").SetValue(true)));
            }

            Option.AddSubMenu(HarassMenu);
            Option.AddSubMenu(LaneClearMenu);
            Option.AddSubMenu(JungleClearMenu);
            Option.AddSubMenu(ComboMenu);
            Option.AddSubMenu(GankingComboMenu);
            Option.AddSubMenu(HotKeyMenu);
            //     Option.AddSubMenu(MiscMenu);
            Option.AddSubMenu(DrawingMenu);

            Option.AddToMainMenu();
        }

        private static bool Human()
        {
            return Player.Spellbook.GetSpell(SpellSlot.Q).Name == "EliseHumanQ";
        }

    }
}
