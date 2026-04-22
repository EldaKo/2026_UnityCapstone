using UnityEngine;

namespace NeoFPS.WieldableTools
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-blockinteractiontoolaction.html")]
    public class BlockInteractionToolAction : BaseWieldableToolModule
    {
        private CharacterInteractionHandler m_Handler = null;

        public override bool isValid
        {
            get { return true; }
        }

        public override WieldableToolActionTiming timing
        {
            get { return WieldableToolActionTiming.Start | WieldableToolActionTiming.End; }
        }

        public override void Initialise(IWieldableTool t)
        {
            base.Initialise(t);

            if (t.wielder != null)
                m_Handler = t.wielder.GetComponent<CharacterInteractionHandler>();
            else
                m_Handler = null;
        }

        public override void FireStart()
        {
            if (m_Handler != null)
                m_Handler.AddBlocker(this);
        }

        public override void FireEnd(bool success)
        {
            if (m_Handler != null)
                m_Handler.RemoveBlocker(this);
        }

        public override bool TickContinuous()
        {
            return true;
        }
    }
}