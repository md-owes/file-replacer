using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace FileReplacer
{
    public class ViewModel
    {
        public ViewModel()
        {
            DestinationValues = new ObservableCollection<Destination>();
            SourceValues = new ObservableCollection<Source>();
        }

        public ObservableCollection<Destination> DestinationValues
        {
            get;
            set;
        }
        public ObservableCollection<Source> SourceValues
        {
            get;
            set;
        }

        public Visibility IsPopupVisible
        {
            get;
            set;
        }
    }
    public class Destination
    {
        public string Path
        {
            get;
            set;
        }
    }
    public class Source
    {
        public string Path
        {
            get;
            set;
        }
    }

    public class SourceComparer : IEqualityComparer<Source>
    {
        public bool Equals(Source x, Source y)
        {
            if (Object.ReferenceEquals(x, y))
                return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Path == y.Path;
        }
        public int GetHashCode(Source srcObject)
        {
            if (Object.ReferenceEquals(srcObject, null))
                return 0;
            return srcObject.Path.GetHashCode();
        }
    }

    public class DestinationComparer : IEqualityComparer<Destination>
    {
        public bool Equals(Destination x, Destination y)
        {
            if (Object.ReferenceEquals(x, y))
                return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Path == y.Path;
        }
        public int GetHashCode(Destination destObject)
        {
            if (Object.ReferenceEquals(destObject, null))
                return 0;
            return destObject.Path.GetHashCode();
        }
    }
}
