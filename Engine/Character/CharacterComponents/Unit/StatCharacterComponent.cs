using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace TestShader
{



    public enum StatUpdateOrder
    {
        Default
    }



    public class StatCharacterComponent : CharacterComponent
    {
        public Dictionary<string, EventListener> _onChange = new Dictionary<string, EventListener>();
        //public StatCharacterComponentEvents Events { get; set; } = new StatCharacterComponentEvents();

        public Dictionary<string, GenStat> stats = new Dictionary<string, GenStat>();

        public StatCharacterComponent(ICharacter character) : base(character)
        {

        }

        public override void LoadContent()
        {
            foreach (FieldInfo field in GetType().GetFields())
            {
                if (field.FieldType.IsSubclassOf(typeof(GenStat)))
                {
                    stats[field.Name] = (GenStat)field.GetValue(this);
                    _onChange[field.Name] = (EventListener)field.FieldType.GetField("_onChange").GetValue(field.GetValue(this));
                }
                else continue;
            }
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {

            foreach (GenStat stat in stats.Values)
            {
                stat.Update(gameTime);
            }

            base.Update(gameTime);
        }

        public override void UnsafeDispose()
        {
            foreach (GenStat stat in stats.Values)
            {
                stat.UnsafeDispose();
            }
            base.UnsafeDispose();
        }


    }
}
