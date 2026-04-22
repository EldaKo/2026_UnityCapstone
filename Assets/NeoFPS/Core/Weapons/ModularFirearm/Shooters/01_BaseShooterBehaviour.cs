using UnityEngine;
using UnityEngine.Events;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.ModularFirearms
{
	public abstract class BaseShooterBehaviour : BaseFirearmModuleBehaviour, IShooter, IFirearmModuleValidity, INeoSerializableComponent
    {
		public event UnityAction<IModularFirearm> onShoot;

        private float m_SpreadMultiplier = 1f;

        public float spreadMultiplier
        {
            get { return m_SpreadMultiplier; }
            set { m_SpreadMultiplier = Mathf.Clamp(value, 0f, 10f); }
        }

        protected virtual void OnEnable()
        {
            if (firearm != null)
                firearm.SetShooter(this);
        }

        public override void Enable()
        {
            base.Enable();

            if (activationMode == FirearmModuleActivationMode.Ignore && firearm != null)
                firearm.SetShooter(this);
        }

        public virtual void Shoot (float accuracy, IAmmoEffect effect)
		{
			SendOnShootEvent ();
		}

		protected void SendOnShootEvent ()
		{
			if (onShoot != null)
				onShoot (firearm);
        }

        public virtual bool isModuleValid
        {
            get { return true; }
        }

        private static readonly NeoSerializationKey k_SpreadMultiplierKey = new NeoSerializationKey("spreadMultiplier");

        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_SpreadMultiplierKey, m_SpreadMultiplier);
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            reader.TryReadValue(k_SpreadMultiplierKey, out m_SpreadMultiplier, m_SpreadMultiplier);
        }
    }
}