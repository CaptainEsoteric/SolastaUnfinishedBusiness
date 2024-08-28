﻿using System.Collections;
using JetBrains.Annotations;

namespace SolastaUnfinishedBusiness.Interfaces;

// On spell being cast
internal interface IOnSpellCasted
{
    int Priority { get; }

    IEnumerator OnSpellCasted(
        RulesetCharacter featureOwner,
        GameLocationCharacter caster,
        CharacterActionCastSpell castAction,
        [UsedImplicitly] RulesetEffectSpell selectEffectSpell,
        [UsedImplicitly] RulesetSpellRepertoire selectedRepertoire,
        SpellDefinition selectedSpellDefinition);
}
