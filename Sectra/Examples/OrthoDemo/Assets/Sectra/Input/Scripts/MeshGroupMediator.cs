
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using IMeshGroupBehaviour = Sectra.Interfaces.IMeshGroupBehaviour;

namespace Sectra
{
    public class MeshGroupMediator : MonoBehaviour
    {
        private struct Item
        {
            public readonly GameObject GameObject;
            public readonly MeshCollider MeshCollider;
            public readonly IMeshGroupBehaviour ManipScript;

            public Item(GameObject gameObject, MeshCollider meshCollider, IMeshGroupBehaviour manipScript) : this()
            {
                GameObject = gameObject;
                MeshCollider = meshCollider;
                ManipScript = manipScript;
            }
        }

        private int N = 0;
        private List<Item> items;

        // Use this for initialization
        void Start()
        {
            // Get items (bone fragments)
            N = this.transform.childCount;
            items = new List<Item>(N);
            for (var i = 0; i < N; i++)
            {
                var go = this.transform.GetChild(i).gameObject;
                var mc = go.GetComponent<MeshCollider>();
                var mgb = go.GetComponent<IMeshGroupBehaviour>();
                items.Add(new Item(go, mc, mgb));
                if (mgb == null)
                    continue;
                mgb.EnteringForeground += this.OnItemEnteringForeground;
                mgb.ExitingForeground += this.OnItemExitingForeground;
                mgb.EnteringSoloInput += this.OnItemEnteringSoloInput;
                mgb.ExitingSoloInput += this.OnItemExitingSoloInput;
            }
        }
        
        void OnItemEnteringForeground(object sender, EventArgs args)
        {
            foreach (var item in NonFocused(sender))
            {
                item.ManipScript.EnterBackground();
                item.ManipScript.enabled = false;
            }
        }

        void OnItemExitingForeground(object sender, EventArgs args)
        {
            foreach (var item in NonFocused(sender))
            {
                item.ManipScript.enabled = true;
                item.ManipScript.EnterNeutral();
            }
        }

        void OnItemEnteringSoloInput(object sender, EventArgs args)
        {
            foreach (var item in NonFocused(sender))
            {
                item.MeshCollider.enabled = false;
                item.ManipScript.enabled = false;
            }
        }

        void OnItemExitingSoloInput(object sender, EventArgs args)
        {
            foreach (var item in NonFocused(sender))
            {
                item.MeshCollider.enabled = true;
                item.ManipScript.enabled = true;
            }
        }
        
        private IEnumerable<Item> NonFocused(object sender)
        {
            return items.Where(c => 
                c.GameObject != (GameObject)sender &&
                c.MeshCollider != null && 
                c.ManipScript != null).ToList();
        }
    }

    public static class EventHandlerExtensions
    {
        public static void Fire(this EventHandler handler)
        {
            if (handler != null)
                handler.Invoke(null, EventArgs.Empty);
        }
        public static void Fire(this EventHandler handler, object sender)
        {
            if (handler != null)
                handler.Invoke(sender, EventArgs.Empty);
        }
        public static void Fire(this EventHandler handler, object sender, EventArgs args)
        {
            if (handler != null)
                handler.Invoke(sender, args);
        }
    }
}
