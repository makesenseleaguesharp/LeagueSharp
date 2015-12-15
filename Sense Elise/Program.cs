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
            _sW = new Spell(SpellSlot.W, 125f);
            _sE = new Spell(SpellSlot.E, 750f);

            SetMenu();

            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }
        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead) return;
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
            var target = TargetSelector.GetTarget(_E.Range, TargetSelector.DamageType.Magical);

            if (Human())
            {
                if (_E.IsReady() && Option_item("HotKey E") && target != null)
                {
                    var HC = HitChance.VeryHigh;
                    switch (Option.Item("Combo E HitChance").GetValue<Slider>().Value)
                    {
                        case 1:
                            HC = HitChance.Impossible;
                            break;
                        case 2:
                            HC = HitChance.Low;
                            break;
                        case 3:
                            HC = HitChance.Medium;
                            break;
                        case 4:
                            HC = HitChance.High;
                            break;
                        case 5:
                            HC = HitChance.VeryHigh;
                            break;
                    }
                    _E.CastIfHitchanceEquals(target, HC, true);
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
                if (Option_item("Harass Human W") && _W.IsReady() && target.Distance(Player.Position) < _W.Range)
                    _W.Cast(target);

                if (Option_item("Harass Human Q") && _Q.IsReady() && target.Distance(Player.Position) < _Q.Range)
                    _Q.CastOnUnit(Qtarget);
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
                    if (Option_item("Lane Human W") && _W.IsReady() && WMinions.Distance(Player.Position) < _W.Range)
                        _W.Cast(WMinions);

                    if (Option_item("Lane Human Q") && _Q.IsReady() && QMinions.Distance(Player.Position) < _Q.Range)
                        _Q.CastOnUnit(QMinions);
                }
            }
            else
            {
                if (Option_item("Lane Spider Q") && _sQ.IsReady() && sQMinions.Distance(sQMinions) < _sQ.Range)
                    _sQ.CastOnUnit(sQMinions);
                if (Option_item("Lane Spider Q") && _sW.IsReady() && sQMinions.Distance(sQMinions) < 125)
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
                        if (_Q.IsReady() && minion.IsValidTarget() && Option_item("JungleClearMenu Human Q") && Player.Distance(minion) <= _Q.Range)
                            _Q.CastOnUnit(minion);

                        if (_W.IsReady() && minion.IsValidTarget() && Option_item("JungleClearMenu Human W") && Player.Distance(minion) <= _W.Range)
                            _W.Cast(minion);

                        if (!_W.IsReady() && !_Q.IsReady() && Option_item("JungleClearMenu R"))
                            _R.Cast();
                    }
                }
                else
                {
                    if (_sQ.IsReady() && Option_item("JungleClearMenu Spider Q") && Player.Distance(minion) <= _sQ.Range)
                        _sQ.CastOnUnit(minion);

                    if (_sW.IsReady() && Option_item("JungleClearMenu Spider W") && Player.Distance(minion) <= 125)
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
            var sEtarget = TargetSelector.GetTarget(_sE.Range, TargetSelector.DamageType.Magical);

            var Wprediction = _W.GetPrediction(Wtarget);

            if (Human())
            {
                if (_E.IsReady() && Etarget != null && Option_item("Combo Human E"))
                {
                    var HC = HitChance.VeryHigh;
                    switch (Option.Item("Combo E HitChance").GetValue<Slider>().Value)
                    {
                        case 1:
                            HC = HitChance.Impossible;
                            break;
                        case 2: 
                            HC = HitChance.Low;
                            break;
                        case 3: 
                            HC = HitChance.Medium;
                            break;
                        case 4: 
                            HC = HitChance.High;
                            break;
                        case 5: 
                            HC = HitChance.VeryHigh;
                            break;
                    }
                    _E.CastIfHitchanceEquals(Etarget, HC, true);
                }
                   

                if (Option_item("Combo Human W") && _W.IsReady() && Wtarget != null)
                {
                    switch (Wprediction.Hitchance)
                    {
                        case HitChance.Medium:
                        case HitChance.High:
                        case HitChance.VeryHigh:
                        case HitChance.Immobile:
                            _W.Cast(Wprediction.CastPosition);
                            break;
                    }
                }

                if (Option_item("Combo Human Q") && _Q.IsReady() && Qtarget != null)
                    _Q.Cast(Qtarget, true);

                if (!_Q.IsReady() && !_W.IsReady() && !_E.IsReady() && Option_item("Combo R") && sQtarget != null)
                    _R.Cast();
            }
            else
            {
                if (Option_item("Combo Spider Q") && _sQ.IsReady() && sQtarget != null)
                    _sQ.Cast(sQtarget);

                if (Option_item("Combo Spider W") && _sW.IsReady() &&  sWtarget != null)
                    _sW.Cast();

                if (Option_item("Combo Spider E") && _sE.IsReady() && Player.Distance(sEtarget) <= _sE.Range && Player.Distance(sEtarget) > _sQ.Range)
                    _sE.Cast(sEtarget);

                if (Option_item("Combo R") && _E.IsReady() && !_sQ.IsReady() && !_sW.IsReady() && Etarget != null)
                    _R.Cast();
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

            var Wprediction = _W.GetPrediction(Wtarget);

            if (Human())
            {
                if (_R.IsReady() && Option.Item("R").GetValue<bool>())
                    _R.Cast();
            }

            if (!Human())
            {
                if (_sE.IsReady() && sEtarget != null && Player.Distance(sEtarget) <= _sE.Range && Player.Distance(sEtarget) > _sQ.Range && Option_item("GankingCombo Spider E"))
                    _sE.Cast(sEtarget);
                if (_sQ.IsReady() && Option_item("GankingCombo Spider Q") && sQtarget != null)
                    _sQ.Cast(sQtarget);
                if (_sW.IsReady() && Option_item("GankingCombo Spider W") && sWtarget != null)
                    _sW.Cast();
                if (!_sW.IsReady() && _sQ.IsReady() && _R.IsReady() && _E.IsReady())
                    _R.Cast();
            }

            if (Option_item("GankingCombo Human E") && _E.IsReady() && Etarget != null)
            {
                var HC = HitChance.VeryHigh;
                switch (Option.Item("Combo E HitChance").GetValue<Slider>().Value)
                {
                    case 1:
                        HC = HitChance.Impossible;
                        break;
                    case 2:
                        HC = HitChance.Low;
                        break;
                    case 3:
                        HC = HitChance.Medium;
                        break;
                    case 4:
                        HC = HitChance.High;
                        break;
                    case 5:
                        HC = HitChance.VeryHigh;
                        break;
                }
                _E.CastIfHitchanceEquals(Etarget, HC, true);
            }


            if (Option_item("GankingCombo Human W") && _W.IsReady() && Wtarget.Distance(Player.Position) <= _W.Range)
                switch (Wprediction.Hitchance)
                {
                    case HitChance.Medium:
                    case HitChance.High:
                    case HitChance.VeryHigh:
                    case HitChance.Immobile:
                        _W.Cast(Wtarget);
                        break;
                }

            if (Option_item("GankingCombo Human Q") && _Q.IsReady() && Qtarget != null)
                _Q.Cast(Qtarget, true);

        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            var elise = Drawing.WorldToScreen(Player.Position);

            if (Human())
            {
                if (Option.Item("Drawing Q Human").GetValue<bool>())
                    Render.Circle.DrawCircle(Player.Position, _Q.Range, Color.Yellow, 1);

                if (Option.Item("Drawing W Human").GetValue<bool>())
                    Render.Circle.DrawCircle(Player.Position, _W.Range, Color.Red, 1);

                if (Option.Item("Drawing E Human").GetValue<bool>())
                    Render.Circle.DrawCircle(Player.Position, _E.Range, Color.White, 1);

            }

            else
            {
                if (Option.Item("Drawing Q Spider").GetValue<bool>())
                    Render.Circle.DrawCircle(Player.Position, _sQ.Range, Color.White, 1);

                if (Option.Item("Drawing E Spider").GetValue<bool>())
                    Render.Circle.DrawCircle(Player.Position, _sE.Range, Color.Red, 1);;
            }
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
                ComboMenu.AddItem((new MenuItem("Combo E HitChance", "Human E HitChance").SetValue(new Slider(3, 1, 5))));
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
                GankingComboMenu.AddItem((new MenuItem("GankingCombo E HitChance", "Human E HitChance").SetValue(new Slider(3, 1, 5))));
                GankingComboMenu.AddItem((new MenuItem("GankingCombo Spider Q", " Use Q [ Spider ]").SetValue(true)));
                GankingComboMenu.AddItem((new MenuItem("GankingCombo Spider W", " Use W [ Spider ]").SetValue(true)));
                GankingComboMenu.AddItem((new MenuItem("GankingCombo Spider E", " Use E [ Spider ]").SetValue(true)));
                GankingComboMenu.AddItem((new MenuItem("R", "Auto Switch Form").SetValue(true)));
                GankingComboMenu.AddItem((new MenuItem("GankingComboMode", "GankingCombo Active").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press))));
            }

            var HotKeyMenu = new Menu("E Hotkey", "E Hotkey");
            {
                HotKeyMenu.AddItem((new MenuItem("HotKey E", "HotKey Human E").SetValue(true)));
                HotKeyMenu.AddItem((new MenuItem("HotKeyMenu E HitChance", "Human E HitChance").SetValue(new Slider(3, 1, 5))));
                //    HotKeyMenu.AddItem((new MenuItem("Flash Use E", "If enemy HP (%) Flash + Use E").SetValue(new Slider(20))));
                HotKeyMenu.AddItem((new MenuItem("HotKey", "HotKey Active").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press))));
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
                //DrawingMenu.AddItem((new MenuItem("ComboDamage", "Combo Damage").SetValue(true)));
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

        private static bool Option_item(string itemname)
        {
            return Option.Item(itemname).GetValue<bool>();
        }

    }
}
