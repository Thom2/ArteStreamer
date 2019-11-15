using System;
using System.Collections;

namespace arte_7
{
    public class VideoDateComparer : IComparer
    {
        #region IComparer Members

        private const string STR_HEUTE = "Heute";
        private const string STR_GESTERN = "Gestern";

        public int Compare(object x, object y)
        {
            Video a = x as Video;
            Video b = y as Video;

            if (null != a && null != b)
            {
                if (a.DisplayedAt.StartsWith(STR_HEUTE))
                {
                    return 1;
                }

                if (b.DisplayedAt.StartsWith(STR_GESTERN))
                    return 1;


            }
            return 0;
        }

        #endregion
    }
}
