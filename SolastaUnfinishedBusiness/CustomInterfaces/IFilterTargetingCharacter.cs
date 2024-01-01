﻿namespace SolastaUnfinishedBusiness.CustomInterfaces;

public interface IFilterTargetingCharacter
{
    public bool EnforceFullSelection { get; }

    public bool IsValid(CursorLocationSelectTarget __instance, GameLocationCharacter target);
}

public interface ISelectPositionAfterCharacter;
