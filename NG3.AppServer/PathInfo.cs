using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NG3.AppServer
{
    [Serializable]
    public class PathInfo
    {
        private string _visualPath = string.Empty;
        public string VisualPath
        {
            get { return _visualPath; }
            set { _visualPath = value; }
        }

        private string _physicalPath = string.Empty;
        public string PhysicalPath
        {
            get { return _physicalPath; }
            set { _physicalPath = value; }
        }

        private string _lowerCasedVirtualPath;
        public string LowerCasedVirtualPath
        {
            get { return _lowerCasedVirtualPath; }
            set { _lowerCasedVirtualPath = value; }
        }

        private string _lowerCasedVirtualPathWithTrailingSlash;
        public string LowerCasedVirtualPathWithTrailingSlash
        {
            get { return _lowerCasedVirtualPathWithTrailingSlash; }
            set { _lowerCasedVirtualPathWithTrailingSlash = value; }
        }

        private string _physicalClientScriptPath;
        public string PhysicalClientScriptPath
        {
            get { return _physicalClientScriptPath; }
            set { _physicalClientScriptPath = value; }
        }

        private string _lowerCasedClientScriptPathWithTrailingSlash;
        public string LowerCasedClientScriptPathWithTrailingSlash
        {
            get { return _lowerCasedClientScriptPathWithTrailingSlash; }
            set { _lowerCasedClientScriptPathWithTrailingSlash = value; }
        }


    }
}
