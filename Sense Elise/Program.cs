using System;
using System.Linq;
using SharpDX;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace SenseElise
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
                case Orbwalking.OrbwalkingMode.LastHit:
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

            if ((Option.Item("HotKey E").GetValue<bool>() && _E.IsReady() && Eprediction.Hitchance > HitChance.High && target.Distance(target) <= _W.Range))
                _E.Cast();
                

            //if (_E.IsReady() && Option.Item("HotKey E").GetValue<Bool>())
        }
        private static void Harass()
        {
            if (Human() && Player.ManaPercent <= Option.Item("Harass Mana").GetValue<Slider>().Value) return;
            var target = TargetSelector.GetTarget(_W.Range, TargetSelector.DamageType.Magical);

            if (target != null)
            {
                var Wprediction = _W.GetPrediction(target);
                if (Option.Item("Harass Human W").GetValue<bool>() && _W.IsReady() && target.Distance(target) <= _W.Range && Wprediction.Hitchance >= HitChance.Medium)
                    _W.Cast(target);
                if (Option.Item("Harass Human Q").GetValue<bool>() && _Q.IsReady() && target.Distance(target) <= _Q.Range)
                    _Q.CastOnUnit(target);
            }
        }

        private static void LaneClear()
        {
            var Minions = MinionManager.GetMinions(Player.ServerPosition, _Q.Range).FirstOrDefault();

            if (Human())
            {
                if (Player.ManaPercent <= Option.Item("LaneClear Mana").GetValue<Slider>().Value) return;
                else
                {
                    if (Option.Item("Lane Human W").GetValue<bool>() && _W.IsReady())
                        _W.Cast(Minions);
                    if (Option.Item("Lane Human Q").GetValue<bool>() && _Q.IsReady())
                        _Q.Cast(Minions);
                }
            }
            else
            {
                if (Option.Item("Lane Spider Q").GetValue<bool>() && _sQ.IsReady() && Minions.Distance(Minions) <= _sQ.Range)
                    _sQ.Cast(Minions);
                if (Option.Item("Lane Spider W").GetValue<bool>() && _sW.IsReady())
                    _sW.Cast();
            }
        }

        private static void JungleClear()
        {
            var JungleMinions = MinionManager.GetMinions(Player.ServerPosition, _Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

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
                    if (_sQ.IsReady() && Option.Item("JungleClearMenu Spider Q").GetValue<bool>())
                        _sQ.Cast(minion);

                    if (_sW.IsReady() && Option.Item("JungleClearMenu Spider W").GetValue<bool>())
                        _sW.Cast();
                      
                }
            }

        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(_W.Range, TargetSelector.DamageType.Magical);

            if (Human())
            {
                var Eprediction = _E.GetPrediction(target);
                if (Option.Item("Combo Human E").GetValue<bool>() && _E.IsReady() && target.Distance(target) <= _E.Range && Eprediction.Hitchance >= HitChance.High)
                    _E.Cast(target);

                if (Option.Item("Combo Human W").GetValue<bool>() && _W.IsReady() && target.Distance(target) <= _W.Range)
                    _W.Cast(target);

                if (Option.Item("Combo Human Q").GetValue<bool>() && _Q.IsReady() && target.Distance(target) <= _Q.Range)
                    _Q.Cast(target);

                if (Option.Item("Auto Smite Enemy").GetValue<bool>())

                if (!_W.IsReady() && !_Q.IsReady() && Option.Item("JungleClearMenu R").GetValue<bool>() && Player.Distance(target) <= _sE.Range)
                    _R.Cast();
            }
            else
            {
                if (Option.Item("Combo Spider Q").GetValue<bool>() && _sQ.IsReady() && target.Distance(target) <= _sQ.Range)
                    _sQ.Cast(target);
                if (Option.Item("Combo Spider W").GetValue<bool>() && _sW.IsReady() && target.Distance(target) <= _sQ.Range)
                    _sW.Cast();
                if (Option.Item("Combo Spider E").GetValue<bool>() && _sE.IsReady() && Player.Distance(target) <= _sE.Range && Player.Distance(target) > _sQ.Range)
                    _sE.Cast(target);
            }

        }

        private static void GankingCombo()
        {
            var Minions = MinionManager.GetMinions(_E.Range).FirstOrDefault();
            var target = TargetSelector.GetTarget(_W.Range, TargetSelector.DamageType.Magical);

            if (Human())
            {
                var Eprediction = _E.GetPrediction(target);
                if (_R.IsReady() && Option.Item("R").GetValue<bool>())
                {
                    _R.Cast();
                }
                if (Option.Item("Combo Human E").GetValue<bool>() && _E.IsReady() && target.Distance(target) <= _E.Range && Eprediction.Hitchance >= HitChance.High)
                    _E.Cast(target);

                if (Option.Item("Combo Human W").GetValue<bool>() && _W.IsReady() && target.Distance(target) <= _W.Range)
                    _W.Cast(target);

                if (Option.Item("Combo Human Q").GetValue<bool>() && _Q.IsReady() && target.Distance(target) <= _Q.Range)
                    _Q.Cast(target);
            }
            else
            {
                if (_sE.IsReady() && Option.Item("GankingCombo Spider E").GetValue<bool>() && _sE.IsReady() && Player.Distance(Minions) <= _sE.Range && Player.Distance(target) > _sQ.Range)
                    _sE.Cast(Minions);
                else if (_sE.IsReady() && Player.Distance(target) <= _sE.Range && Player.Distance(target) > _sQ.Range && Option.Item("GankingCombo Spider E").GetValue<bool>())
                     _sE.Cast(target);
                if (_sQ.IsReady() && Option.Item("GankingCombo Spider Q").GetValue<bool>())
                    _sQ.Cast(target);
                if (_sW.IsReady() && Option.Item("GankingCombo Spider W").GetValue<bool>())
                    _sW.Cast();
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            var elise = Drawing.WorldToScreen(Player.Position);

            if (Option.Item("Drawing Q Spider").GetValue<bool>())
                Render.Circle.DrawCircle(Player.Position, _sQ.Range, Color.White, 1);
            if (Option.Item("Drawing E Spider").GetValue<bool>())
                Render.Circle.DrawCircle(Player.Position, _sE.Range, Color.Red, 1);
            if (Option.Item("Drawing E Spider").GetValue<bool>())
                Render.Circle.DrawCircle(Player.Position, _Q.Range, Color.Blue, 1);
            if (Option.Item("Drawing Q Human").GetValue<bool>())
                Render.Circle.DrawCircle(Player.Position, _Q.Range, Color.Green, 1);
            if (Option.Item("Drawing W Human").GetValue<bool>())
                Render.Circle.DrawCircle(Player.Position, _W.Range, Color.Yellow, 1);
            if (Option.Item("Drawing E Human").GetValue<bool>())
                Render.Circle.DrawCircle(Player.Position, _E.Range, Color.YellowGreen, 1);

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

            var MiscMenu = new Menu("Misc", "Misc");
            {
                MiscMenu.AddItem((new MenuItem("Auto Smite Red", "Auto Smite Red").SetValue(true)));
                MiscMenu.AddItem((new MenuItem("Auto Smite Blue", "Auto Smite Blue").SetValue(true)));
                MiscMenu.AddItem((new MenuItem("Auto Smite Baron", "Auto Smite Baron").SetValue(true)));
                MiscMenu.AddItem((new MenuItem("Auto Smite Dragon", "Auto Smite Dragon").SetValue(true)));
                MiscMenu.AddItem((new MenuItem("Auto Smite Enemy", "Auto Smite Enemy").SetValue(true)));
                MiscMenu.AddItem((new MenuItem("Anti Gapcloser", "Anti Gapcloser").SetValue(true)));
            }

            var DrawingMenu = new Menu("Drawing", "Drawing");
            {
                DrawingMenu.AddItem((new MenuItem("Drawing Q Human", "Draw Q [ Human ]").SetValue(true)));
                DrawingMenu.AddItem((new MenuItem("Drawing W Human", "Draw W [ Human ]").SetValue(true)));
                DrawingMenu.AddItem((new MenuItem("Drawing E Human", "Draw E [ Human ]").SetValue(true)));
                DrawingMenu.AddItem((new MenuItem("Drawing Q Spider", "Draw Q [ Spider ]").SetValue(true)));
                DrawingMenu.AddItem((new MenuItem("Drawing E Spider", "Draw E [ Spider ]").SetValue(true)));
                DrawingMenu.AddItem((new MenuItem("ComboDamage", "Combo Damage").SetValue(true)));
            }

            Option.AddSubMenu(HarassMenu);
            Option.AddSubMenu(LaneClearMenu);
            Option.AddSubMenu(JungleClearMenu);
            Option.AddSubMenu(ComboMenu);
            Option.AddSubMenu(GankingComboMenu);
            Option.AddSubMenu(HotKeyMenu);
            Option.AddSubMenu(MiscMenu);
            Option.AddSubMenu(DrawingMenu);

            Option.AddToMainMenu();
        }

        private static bool Human()
        {
            return Player.Spellbook.GetSpell(SpellSlot.Q).Name == "EliseHumanQ";
        }
    }
}
