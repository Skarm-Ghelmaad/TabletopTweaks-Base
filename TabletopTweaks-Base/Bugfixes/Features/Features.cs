﻿using HarmonyLib;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using System.Linq;
using TabletopTweaks.Core.Utilities;
using static TabletopTweaks.Base.Main;

namespace TabletopTweaks.Base.Bugfixes.Features {
    internal class Features {
        [HarmonyPatch(typeof(BlueprintsCache), "Init")]
        static class BlueprintsCache_Init_Patch {
            static bool Initialized;

            static void Postfix() {
                if (Initialized) return;
                Initialized = true;
                TTTContext.Logger.LogHeader("Patching Features");
                PatchMongrolsBlessing();
                PatchIncorporealCharm();

                void PatchMongrolsBlessing() {
                    if (Main.TTTContext.Fixes.Features.IsDisabled("MongrolsBlessing")) { return; }

                    var MongrelsBlessingFeature = BlueprintTools.GetBlueprint<BlueprintFeature>("d6821b4401584f469cae3492aeba9808");

                    MongrelsBlessingFeature.FlattenAllActions()
                        .OfType<ContextActionConditionalSaved>()
                        .ForEach(condition => {
                            condition.Failed = Helpers.CreateActionList(
                                new ContextActionDealDamage() {
                                    m_Type = ContextActionDealDamage.Type.EnergyDrain,
                                    EnergyDrainType = EnergyDrainType.Permanent,
                                    DamageType = new DamageTypeDescription(),
                                    Duration = new ContextDurationValue() {
                                        Rate = DurationRate.Days,
                                        DiceCountValue = 0,
                                        BonusValue = 1,
                                    },
                                    Value = new ContextDiceValue() {
                                        DiceCountValue = 0,
                                        BonusValue = 1
                                    }
                                }
                            );
                        });

                    TTTContext.Logger.LogPatch(MongrelsBlessingFeature);
                }
                void PatchIncorporealCharm() {
                    if (Main.TTTContext.Fixes.Features.IsDisabled("IncorporealCharm")) { return; }

                    var IncorporealCharmFeature = BlueprintTools.GetBlueprint<BlueprintFeature>("8ee86ca474114d8d8eb0946a2ff43eb8");
                    IncorporealCharmFeature.AddComponent<RecalculateOnStatChange>(c => {
                        c.Stat = StatType.Charisma;
                    });
                    TTTContext.Logger.LogPatch(IncorporealCharmFeature);
                }
            }
        }
    }
}
