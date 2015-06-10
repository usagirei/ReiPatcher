// --------------------------------------------------
// ReiPatcher - IniComment.cs
// --------------------------------------------------

#region Usings
using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace ReiPatcher.INI
{

    public class IniComment
    {
        #region Fields
        private List<string> _comments;
        #endregion

        #region Properties
        public List<string> Comments
        {
            get { return _comments; }
            set { _comments = value; }
        }
        #endregion

        #region (De)Constructors
        public IniComment()
        {
            Comments = new List<string>();
        }
        #endregion

        #region Public Methods
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _comments.Count; i++)
            {
                string comment = _comments[i];

                string value = i < _comments.Count - 1
                    ? ";" + comment + Environment.NewLine
                    : ";" + comment;

                sb.Append(value);
            }
            return sb.ToString();
        }

        public void Append(params string[] comments)
        {
            Comments.AddRange(comments);
        }
        #endregion
    }

}