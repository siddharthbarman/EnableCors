using System;
using System.Collections.Generic;

namespace Sid.Utils
{
    public class CmdLine
	{
		public CmdLine(string[] args, char flagCharacter = '-')
		{
			if (args == null)
				m_args = new string[] { };
			else
				m_args = args;
			
			m_flagCharacter = flagCharacter;
			Parse();
		}

		private void Parse()
		{
			string lastFlag = null;

			foreach(string s in m_args)
			{
				string flag;
				if (IsStringFlag(s, out flag))
				{
					lastFlag = flag;
					m_flagValueDict.Add(flag, null);
				}
				else
				{
					if (lastFlag != null)
					{
						m_flagValueDict[lastFlag] = s;
						lastFlag = null;
					}
					else
					{
						m_positionalArgs.Add(s);
					}
				}
			}
		}

		public string GetFlagValue(string flag)
		{
			return m_flagValueDict[flag];
		}

		public T GetFlagValue<T>(string flag)
		{
			return (T)Convert.ChangeType(m_flagValueDict[flag], typeof(T));
		}

		public bool IsFlagPresent(string flag)
		{
			return m_flagValueDict.ContainsKey(flag);
		}

		public string GetPositionalArgument(int index)
		{
			return m_positionalArgs[index];
		}

		public int FlagCount
		{
			get
			{
				return m_flagValueDict.Count;
			}
		}

		public int RawArgCount
		{
			get { return m_args.Length; }
		}

		public int PositionalArgumentCount
		{
			get { return m_positionalArgs.Count; }
		}

		public string this[string flag]
		{
			get {
                if (!m_flagValueDict.ContainsKey(flag))
                    return null;
                else
                    return m_flagValueDict[flag];
            }
		}

		public string this[int positionalIndex]
		{
			get { return m_positionalArgs[positionalIndex]; }
		}

		private bool IsStringFlag(string s, out string flag)
		{
			if (string.IsNullOrEmpty(s))
			{
				flag = null;
				return false;
			}
			else
			{
				flag = s.Substring(1);
				return s[0] == m_flagCharacter;
			}
		}

		private char m_flagCharacter;
		private string[] m_args;
		private List<string> m_positionalArgs = new List<string>();
		private Dictionary<string, string> m_flagValueDict = new Dictionary<string, string>();
	}
}
