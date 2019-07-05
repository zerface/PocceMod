﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PocceMod.Mod
{
    public static class Peds
    {
        public static List<int> Get(bool includeAnimals = true, bool includePlayers = false, bool includeDead = false, float rangeSquared = 900.0f)
        {
            var peds = new List<int>();
            int ped = 0;
            int handle = API.FindFirstPed(ref ped);
            var player = Game.Player.Character.Handle;
            var coords = Game.Player.Character.Position;

            if (handle == -1)
                return peds;

            do
            {
                var pos = API.GetEntityCoords(ped, true);

                if (!includeAnimals && !API.IsPedHuman(ped))
                    continue;

                if (!includePlayers && API.IsPedAPlayer(ped))
                    continue;

                if (!includeDead && API.IsPedDeadOrDying(ped, true))
                    continue;

                if (coords.DistanceToSquared(pos) > rangeSquared || ped == player)
                    continue;

                peds.Add(ped);

            } while (API.FindNextPed(handle, ref ped));

            API.EndFindPed(handle);
            return peds;
        }

        public static async Task<int> Spawn(uint[] modelList, int pedType = 26)
        {
            var model = modelList[API.GetRandomIntInRange(0, modelList.Length)];
            var coords = Game.Player.Character.Position;
            var pos = new Vector3();

            if (!API.GetSafeCoordForPed(coords.X, coords.Y, coords.Z, true, ref pos, 16))
            {
                pos.X = coords.X;
                pos.Y = coords.Y;
                pos.Z = coords.Z + 1.0f;
            }

            await Common.RequestModel(model);
            return API.CreatePed(pedType, model, pos.X, pos.Y, pos.Z, 0.0f, true, false);
        }

        public static async Task Arm(int ped, uint[] weaponList)
        {
            API.TaskSetBlockingOfNonTemporaryEvents(ped, true);
            API.SetPedKeepTask(ped, true);
            await BaseScript.Delay(10);

            API.SetPedCombatAbility(ped, 100);
            API.SetPedCombatMovement(ped, 2);
            API.SetPedCombatRange(ped, 2);
            API.SetPedHearingRange(ped, float.MaxValue);
            API.SetPedCombatAttributes(ped, 5, true);
            API.SetPedCombatAttributes(ped, 17, true);
            API.SetPedCombatAttributes(ped, 46, true);
            API.SetPedCombatAttributes(ped, 1424, true);
            API.SetPedFleeAttributes(ped, 0, false);
            API.SetEntityHealth(ped, 200);

            if (weaponList != null)
            {
                API.SetPedArmour(ped, 200);
                var weapon = weaponList[API.GetRandomIntInRange(0, weaponList.Length)];
                new Ped(ped).Weapons.Give((WeaponHash)weapon, int.MaxValue, true, true);
            }
        }
    }
}
