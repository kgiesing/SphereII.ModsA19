using Audio;
using HarmonyLib;
using System;
using UnityEngine;

//OnBlockActivated(int _indexInBlockActivationCommands, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)

public class SphereII_Blocks_BlockSecureLoot
{

    private static readonly string AdvFeatureClass = "AdvancedLockpicking";
    private static readonly string Feature = "AdvancedLocks";

    [HarmonyPatch(typeof(BlockSecureLoot))]
    [HarmonyPatch("OnBlockActivated")]
    [HarmonyPatch(new Type[] { typeof(int), typeof(WorldBase), typeof(int), typeof(Vector3i), typeof(BlockValue), typeof(EntityPlayer) })]

    public class SphereII_Block_Init
    {
        public static bool Prefix(ref Block __instance, int _indexInBlockActivationCommands, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player
            , string ___lockPickItem)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            if (_blockValue.ischild)
            {
                return true;
            }
            TileEntitySecureLootContainer tileEntitySecureLootContainer = _world.GetTileEntity(_cIdx, _blockPos) as TileEntitySecureLootContainer;
            if (tileEntitySecureLootContainer == null)
            {
                return false;
            }

            if (tileEntitySecureLootContainer.IsLocked() && tileEntitySecureLootContainer.IsUserAllowed(GamePrefs.GetString(EnumGamePrefs.PlayerId)))
            {
                __instance.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
                return false;
            }
            switch (_indexInBlockActivationCommands)
            {
                case 0:
                    if (!tileEntitySecureLootContainer.IsLocked() || tileEntitySecureLootContainer.IsUserAllowed(GamePrefs.GetString(EnumGamePrefs.PlayerId)))
                    {
                        __instance.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
                        return false;
                    }
                    Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
                    return false;
                case 1:
                    tileEntitySecureLootContainer.SetLocked(true);
                    Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locking");
                    GameManager.ShowTooltip(_player as EntityPlayerLocal, "containerLocked");
                    return false;
                case 2:
                    tileEntitySecureLootContainer.SetLocked(false);
                    Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
                    GameManager.ShowTooltip(_player as EntityPlayerLocal, "containerUnlocked");
                    return false;
                case 3:
                    {
                        LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_player as EntityPlayerLocal);
                        if (uiforPlayer != null)
                        {
                            XUiC_KeypadWindow.Open(uiforPlayer, tileEntitySecureLootContainer);
                        }
                        return false;
                    }
                case 4:
                    {
                        // Check if the player has lock picks.
                        LocalPlayerUI playerUI = (_player as EntityPlayerLocal).PlayerUI;
                        XUiM_PlayerInventory playerInventory = playerUI.xui.PlayerInventory;
                        ItemValue item = ItemClass.GetItem("resourceLockPick", false);
                        if (playerInventory.GetItemCount(item) == 0)
                        {
                            playerUI.xui.CollectedItemList.AddItemStack(new ItemStack(item, 0), true);
                            GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("ttLockpickMissing"));
                            return false;
                        }

                        tileEntitySecureLootContainer.SetLocked(true);
                        XUiC_PickLocking.Open(playerUI, tileEntitySecureLootContainer, _blockValue, _blockPos);
                        return false;

                    }
            }

            return true;

        }

    }
    [HarmonyPatch(typeof(BlockDoorSecure))]
    [HarmonyPatch("OnBlockActivated")]
    [HarmonyPatch(new Type[] { typeof(int), typeof(WorldBase), typeof(int), typeof(Vector3i), typeof(BlockValue), typeof(EntityPlayer) })]

    public class SphereII_Block_Init_Door
    {
        public static bool Prefix(ref Block __instance, int _indexInBlockActivationCommands, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            if (_blockValue.ischild)
            {
                return true;
            }
            TileEntitySecureDoor tileEntitySecureDoor = (TileEntitySecureDoor)_world.GetTileEntity(_cIdx, _blockPos);
            if (tileEntitySecureDoor == null)
                return true;


            if (!tileEntitySecureDoor.IsLocked() || tileEntitySecureDoor.IsUserAllowed(GamePrefs.GetString(EnumGamePrefs.PlayerId)))
            {
                    __instance.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
                    return false;
            }


            if (tileEntitySecureDoor.IsLocked() && tileEntitySecureDoor.IsUserAllowed(GamePrefs.GetString(EnumGamePrefs.PlayerId)))
            {
                __instance.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
                return false;
            }
            if (tileEntitySecureDoor.IsLocked())
            {
                // 1 == try to open locked door.
                if (_indexInBlockActivationCommands == 1)
                {
                    // Check if the player has lock picks.
                    LocalPlayerUI playerUI = (_player as EntityPlayerLocal).PlayerUI;
                    XUiM_PlayerInventory playerInventory = playerUI.xui.PlayerInventory;
                    ItemValue item = ItemClass.GetItem("resourceLockPick", false);
                    if (playerInventory.GetItemCount(item) == 0)
                    {
                        playerUI.xui.CollectedItemList.AddItemStack(new ItemStack(item, 0), true);
                        GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("ttLockpickMissing"));
                        return false;
                    }

                    tileEntitySecureDoor.SetLocked(true);
                    XUiC_PickLocking.Open(playerUI, tileEntitySecureDoor, _blockValue, _blockPos);
                    return false;
                }
            }
            return true;
        }
    }
}