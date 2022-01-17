// -----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.API.Features;
using Exiled.API.Features.Items;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Pickups;
using Mirror;
using UnityEngine;

namespace Mistaken.TurretsAPI
{
    internal static class Extensions
    {
        public static Pickup SpawnLocked(this Item instance, Vector3 position, Transform parent, Quaternion rotation = default(Quaternion))
        {
            instance.Base.PickupDropModel.Info.ItemId = instance.Type;
            instance.Base.PickupDropModel.Info.Position = position;
            instance.Base.PickupDropModel.Info.Weight = instance.Weight;
            instance.Base.PickupDropModel.Info.Rotation = new LowPrecisionQuaternion(rotation);
            instance.Base.PickupDropModel.NetworkInfo = instance.Base.PickupDropModel.Info;
            ItemPickupBase itemPickupBase = UnityEngine.Object.Instantiate(instance.Base.PickupDropModel, position, rotation, parent);
            FirearmPickup firearmPickup = itemPickupBase as FirearmPickup;
            if ((object)firearmPickup != null)
            {
                Exiled.API.Features.Items.Firearm firearm = instance as Exiled.API.Features.Items.Firearm;
                if (firearm != null)
                {
                    firearmPickup.Status = new FirearmStatus(firearm.Ammo, FirearmStatusFlags.MagazineInserted, firearmPickup.Status.Attachments);
                }
                else
                {
                    ItemBase @base = instance.Base;
                    byte ammo = (byte)(((int?)(@base as AutomaticFirearm)?._baseMaxAmmo) ?? ((int?)(@base as Shotgun)?._ammoCapacity) ?? ((@base is Revolver) ? 6 : 0));
                    firearmPickup.Status = new FirearmStatus(ammo, FirearmStatusFlags.MagazineInserted, firearmPickup.Status.Attachments);
                }

                firearmPickup.NetworkStatus = firearmPickup.Status;
            }

            NetworkServer.Spawn(itemPickupBase.gameObject);
            itemPickupBase.Info = new PickupSyncInfo
            {
                Locked = true,
                InUse = false,
                ItemId = itemPickupBase.Info.ItemId,
                Position = itemPickupBase.Info.Position,
                Rotation = itemPickupBase.Info.Rotation,
                Serial = itemPickupBase.Info.Serial,
                Weight = itemPickupBase.Info.Weight,
                _flags = PickupSyncInfo.PickupFlags.Locked,
            };
            itemPickupBase.InfoReceived(default(PickupSyncInfo), itemPickupBase.Info);
            Pickup pickup = Pickup.Get(itemPickupBase);
            pickup.Scale = instance.Scale;
            return pickup;
        }
    }
}
