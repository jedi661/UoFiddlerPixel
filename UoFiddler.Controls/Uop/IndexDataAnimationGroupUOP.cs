namespace UoFiddler.Controls.Uop
{
    public class IndexDataAnimationGroupUOP
    {
        public IndexDataFileInfo[] m_Direction { get; private set; }

        public IndexDataAnimationGroupUOP()
        {
            m_Direction = new IndexDataFileInfo[5];
        }

        public void SetFrameCount(int frameCount)
        {
            for (int i = 0; i < m_Direction.Length; i++)
            {
                if (m_Direction[i] != null)
                {
                    m_Direction[i].SetFrameCount(frameCount);
                }
            }
        }
    }
}
