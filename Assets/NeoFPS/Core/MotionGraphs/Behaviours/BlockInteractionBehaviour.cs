using UnityEngine;

namespace NeoFPS.CharacterMotion.Behaviours
{
    [MotionGraphElement("Character/BlockInteraction", "BlockInteractionBehaviour")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgb-blockinteractionbehaviour.html")]
    public class BlockInteractionBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("Whether to block or unblock the parameter on entering the state.")]
        private BlockValue m_OnEnter = BlockValue.Block;

        [SerializeField, Tooltip("Whether to block or unblock the parameter on exiting the state.")]
        private BlockValue m_OnExit = BlockValue.Unblock;

        private CharacterInteractionHandler m_Handler = null;

        public enum BlockValue
        {
            Block,
            Unblock,
            Nothing
        }

        public override void Initialise(MotionGraphConnectable o)
        {
            base.Initialise(o);

            m_Handler = controller.GetComponent<CharacterInteractionHandler>();
        }

        public override void OnEnter()
        {
            if (m_Handler != null)
            {
                // Change value
                switch (m_OnEnter)
                {
                    case BlockValue.Block:
                        m_Handler.AddBlocker(this);
                        return;
                    case BlockValue.Unblock:
                        m_Handler.RemoveBlocker(this);
                        return;
                }
            }
        }

        public override void OnExit()
        {
            if (m_Handler != null)
            {
                switch (m_OnExit)
                {
                    case BlockValue.Block:
                        m_Handler.AddBlocker(this);
                        return;
                    case BlockValue.Unblock:
                        m_Handler.RemoveBlocker(this);
                        return;
                }
            }
        }
    }
}