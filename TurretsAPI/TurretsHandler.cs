// -----------------------------------------------------------------------
// <copyright file="TurretsHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Interfaces;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Pickups;
using Mistaken.API.Diagnostics;
using UnityEngine;

namespace Mistaken.TurretsAPI
{
    internal class TurretsHandler : Module
    {
        public TurretsHandler(IPlugin<IConfig> plugin)
            : base(plugin)
        {
            this.LoadTurrets();
        }

        public override string Name => nameof(TurretsHandler);

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Shooting += this.Player_Shooting;
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Shooting -= this.Player_Shooting;
        }

        internal void LoadTurrets()
        {
            var file = Directory.GetFiles(this.assetsPath).FirstOrDefault(x => x.Contains("turrets"));
            if (string.IsNullOrWhiteSpace(file))
            {
                this.Log.Error("Couldn't find any turret in AssetBundle folder");
                return;
            }

            var name = Path.GetFileName(file);
            var bundle = UnityEngine.AssetBundle.LoadFromFile(file);
            var prefab = bundle.LoadAsset<GameObject>("Enginner_Turret_E11");

            if (prefab is null)
            {
                this.Log.Error("Prefab in bundle is null");
                return;
            }

            TurretsPrefabs.Add(ItemType.GunE11SR, prefab);
            bundle.Unload(false);
        }

        internal void SpawnTurret(ItemType type, Vector3 position)
        {
            if (!TurretsPrefabs.ContainsKey(type))
                return;

            var obj = new GameObject("Turret");
            obj.transform.position = position;
            this.SpawnPrefab(TurretsPrefabs[type], obj.transform);
        }

        private static readonly Dictionary<ItemType, GameObject> TurretsPrefabs = new Dictionary<ItemType, GameObject>();

        private string assetsPath = Path.Combine(Paths.Plugins, "AssetBoundle");

        private void Player_Shooting(Exiled.Events.EventArgs.ShootingEventArgs ev)
        {
            var firearm = ev.Shooter.CurrentItem as Firearm;
            this.Log.Debug("Attachments code: " + firearm.Base.GetCurrentAttachmentsCode(), true);
            this.SpawnTurret(ItemType.GunE11SR, ev.Shooter.Position);
        }

        private GameObject SpawnPrefab(GameObject prefab, Transform parent)
        {
            var prefabObject = UnityEngine.Object.Instantiate(prefab, parent);
            prefabObject.hideFlags = HideFlags.HideAndDontSave;

            foreach (var transform in prefabObject.GetComponentsInChildren<Transform>())
            {
                if (!transform.gameObject.activeSelf)
                    continue;

                if (!transform.TryGetComponent<MeshFilter>(out MeshFilter filter))
                {
                    this.Log.Debug("object name: " + transform.name, true);

                    var itemType = ItemType.None;
                    var name = transform.name.ToLower().Split('_');
                    if (name.Length > 1)
                    {
                        if (name[1].Contains("e11"))
                            itemType = ItemType.GunE11SR;
                        else if (name[1].Contains("fsp9"))
                            itemType = ItemType.GunFSP9;
                        else if (name[1].Contains("revolver"))
                            itemType = ItemType.GunRevolver;
                        else if (name[1].Contains("shotgun"))
                            itemType = ItemType.GunShotgun;
                        else if (name[1].Contains("logicer"))
                            itemType = ItemType.GunLogicer;
                        else if (name[1].Contains("crossvec"))
                            itemType = ItemType.GunCrossvec;
                        else if (name[1].Contains("com15"))
                            itemType = ItemType.GunCOM15;
                        else if (name[1].Contains("com18"))
                            itemType = ItemType.GunCOM18;
                        else if (name[1].Contains("ak"))
                            itemType = ItemType.GunAK;
                    }

                    if (itemType != ItemType.None)
                    {
                        var tor = this.CreateEmpty(transform);
                        transform.gameObject.SetActive(false);
                        ItemPickupBase spawned;
                        var item = new Firearm(itemType);
                        item.Scale = tor.transform.lossyScale;
                        spawned = item.SpawnLocked(tor.transform.position, prefabObject.transform, tor.transform.rotation).Base;
                        ((InventorySystem.Items.Firearms.FirearmPickup)spawned).Status = new InventorySystem.Items.Firearms.FirearmStatus(66, InventorySystem.Items.Firearms.FirearmStatusFlags.MagazineInserted | InventorySystem.Items.Firearms.FirearmStatusFlags.Chambered, 6362177);
                        spawned.Rb.constraints = RigidbodyConstraints.FreezeAll;
                        this.Log.Debug("Spawned Firearm: " + itemType.ToString(), true);
                    }

                    continue;
                }

                if (!transform.TryGetComponent<MeshRenderer>(out MeshRenderer renderer))
                    continue;

                PrimitiveType type;

                switch (filter.mesh.name)
                {
                    case "Plane Instance":
                        type = PrimitiveType.Plane;
                        break;
                    case "Cylinder Instance":
                        type = PrimitiveType.Cylinder;
                        break;
                    case "Cube Instance":
                        type = PrimitiveType.Cube;
                        break;
                    case "Capsule Instance":
                        type = PrimitiveType.Capsule;
                        break;
                    case "Quad Instance":
                        type = PrimitiveType.Quad;
                        break;
                    case "Sphere Instance":
                        type = PrimitiveType.Sphere;
                        break;
                    default:
                        continue;
                }

                this.Log.Debug("Spawned primitive object at position: " + transform.position, true);
                API.MapPlus.SpawnPrimitive(type, transform, Color.gray, true, null);
            }

            return prefabObject;
        }

        private GameObject CreateEmpty(Transform parent)
        {
            GameObject tor = new GameObject();
            tor.transform.parent = parent;
            tor.transform.localPosition = Vector3.zero;
            tor.transform.localRotation = Quaternion.identity;
            tor.transform.localScale = Vector3.one;
            return tor;
        }
    }
}
