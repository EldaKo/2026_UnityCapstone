using UnityEngine;
using UnityEngine.Events;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.ModularFirearms
{
	public abstract class BaseReloaderBehaviour : BaseFirearmModuleBehaviour, IReloader, IFirearmModuleValidity, INeoSerializableComponent
    {
        [Header ("Reloader Settings")]

        [SerializeField, Delayed, Tooltip("The number of rounds that can be fit in the magazine at once.")]
		private int m_MagazineSize = 1;

        [SerializeField, MaxValue("m_MagazineSize"), Tooltip("The number of rounds in the magazine on initialisation.")]
		private int m_StartingMagazine = 1;

        public event UnityAction<IModularFirearm, int> onCurrentMagazineChange;
		public event UnityAction<IModularFirearm> onReloadStart;
		public event UnityAction<IModularFirearm> onReloadComplete;

        private int m_CurrentMagazine = -1;
        private int m_MagazineExtension = 0;
        private float m_ReloadSpeedMultiplier = 1f;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (m_MagazineSize < 1)
                m_MagazineSize = 1;
            m_StartingMagazine = Mathf.Clamp(m_StartingMagazine, 0, m_MagazineSize);
        }
#endif
        
		protected virtual void Start ()
		{
            // Set starting size (check if changed by serialization first)
            if (currentMagazine == -1)
                currentMagazine = startingMagazine;
		}

		protected virtual void Update ()
		{
		}

        protected virtual void OnEnable()
        {
            firearm.SetReloader(this);
            OnReloadSpeedMultiplierChanged();
        }
        protected virtual void OnDisable() { }

        public override void Enable()
        {
            base.Enable();

            if (activationMode == FirearmModuleActivationMode.Ignore)
                firearm.SetReloader(this);
        }

        public virtual bool empty { get { return currentMagazine == 0; } }
		public virtual bool full { get { return currentMagazine == magazineSize; } }
		public virtual bool canReload { get { return !full && (firearm.ammo == null || firearm.ammo.available); } }

		public int magazineSize
		{
			get { return m_MagazineSize + m_MagazineExtension; }
			protected set
			{
				m_MagazineSize = value;
				if (m_CurrentMagazine > m_MagazineSize)
					currentMagazine = m_MagazineSize;
			}
		}

		public int startingMagazine
		{
			get { return m_StartingMagazine; }
            set { m_StartingMagazine = Mathf.Clamp(value, 0, m_MagazineSize); }
		}

		public int currentMagazine
		{
			get
            {
                if (m_CurrentMagazine == -1)
                    return m_StartingMagazine;
                else
                    return m_CurrentMagazine;
            }
			set
			{
                int oldValue = m_CurrentMagazine;
				m_CurrentMagazine = Mathf.Clamp (value, 0, m_MagazineSize + m_MagazineExtension);
                OnCurrentMagazineChange(oldValue, m_CurrentMagazine);
            }
		}

        protected float reloadTimeMultiplier
        {
            get;
            private set;
        } = 1f;

        public float reloadSpeedMultiplier
        {
            get { return m_ReloadSpeedMultiplier; }
            set
            {
                m_ReloadSpeedMultiplier = Mathf.Clamp( value, 0.01f, 10f);
                reloadTimeMultiplier = 1f / m_ReloadSpeedMultiplier;
                OnReloadSpeedMultiplierChanged();
            }
        }

        public int magazineExtension
        {
            get { return m_MagazineExtension; }
            set
            {
                m_MagazineExtension = value;
                int oldCount = m_CurrentMagazine;
                m_CurrentMagazine = Mathf.Clamp(m_CurrentMagazine, 0, m_MagazineSize + m_MagazineExtension);
                OnCurrentMagazineChange(oldCount, m_CurrentMagazine);
            }
        }

        public virtual void DecrementMag (int amount)
		{
			currentMagazine -= amount;
		}

		public abstract bool isReloading { get; }
		public abstract Waitable Reload ();

        public abstract FirearmDelayType reloadDelayType { get; }
        public virtual void ManualReloadPartial() { }
		public abstract void ManualReloadComplete ();

        public virtual bool interruptable
        {
            get { return false; }
        }

        public virtual void Interrupt()
        { }

        protected virtual void OnCurrentMagazineChange(int from, int to)
        {
            if (onCurrentMagazineChange != null)
                onCurrentMagazineChange(firearm, m_CurrentMagazine);
        }

        protected void SendReloadStartedEvent ()
		{
			if (onReloadStart != null)
				onReloadStart (firearm);
		}
		protected void SendReloadCompletedEvent ()
		{
			// Fire completed event
			if (onReloadComplete != null)
				onReloadComplete (firearm);
        }

        protected virtual void OnReloadSpeedMultiplierChanged() { }

        public virtual bool isModuleValid
        {
            get { return true; }
        }

        private static readonly NeoSerializationKey k_MagSizeKey = new NeoSerializationKey("magazineSize");
        private static readonly NeoSerializationKey k_StartingMagKey = new NeoSerializationKey("startingMag");
        private static readonly NeoSerializationKey k_CurrentMagKey = new NeoSerializationKey("currentMag");
        private static readonly NeoSerializationKey k_ReloadSpeedMultiplierKey = new NeoSerializationKey("speedMult");
        private static readonly NeoSerializationKey k_MagazineExtensionKey = new NeoSerializationKey("magExt");

        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            // Write magazine info
            writer.WriteValue(k_MagSizeKey, magazineSize);
            writer.WriteValue(k_StartingMagKey, startingMagazine);
            writer.WriteValue(k_CurrentMagKey, currentMagazine);

            writer.WriteValue(k_ReloadSpeedMultiplierKey, m_ReloadSpeedMultiplier);
            writer.WriteValue(k_MagazineExtensionKey, m_MagazineExtension);
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            // Read magazine info
            reader.TryReadValue(k_MagazineExtensionKey, out m_MagazineExtension, m_MagazineExtension);

            int intResult = 0;
            if (reader.TryReadValue(k_MagSizeKey, out intResult, m_MagazineSize))
                magazineSize = intResult;
            if (reader.TryReadValue(k_StartingMagKey, out intResult, m_StartingMagazine))
                startingMagazine = intResult;
            if (reader.TryReadValue(k_CurrentMagKey, out intResult, m_CurrentMagazine))
                currentMagazine = intResult;

            if (reader.TryReadValue(k_ReloadSpeedMultiplierKey, out float speedMultiplier, m_ReloadSpeedMultiplier))
                reloadSpeedMultiplier = speedMultiplier;
        }
    }
}
