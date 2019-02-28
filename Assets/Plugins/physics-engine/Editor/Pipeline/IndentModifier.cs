using System.Text;

namespace Pipeline
{
    public class IndentModifer
    {
        private int m_CurrentIndent;

        public IndentModifer()
        {
            m_CurrentIndent = 0;
        }

        public void Reset()
        {
            m_CurrentIndent = 0;
        }

        public void Indent()
        {
            m_CurrentIndent++;
        }

        public void Raise()
        {
            m_CurrentIndent--;
            if (m_CurrentIndent < 0)
                m_CurrentIndent = 0;
        }

        public string GetIndent()
        {
            StringBuilder indentStr = new StringBuilder();
            int numOfIndentLevel = m_CurrentIndent;
            while (numOfIndentLevel > 0)
            {
                numOfIndentLevel--;
                indentStr.Append("\t");
            }

            return indentStr.ToString();
        }
    }
}