using System.Collections.ObjectModel;

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
}
