using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CbLib
{
    public class SelectionItem : ObservableObject
    {
        private string _Name = "";
        private object _Obj = new();
        private bool _IsSelected = false;

        public string Name
        {
            get { return _Name; }
            set { _Name = value; OnPropertyChanged(); }
        }

        public object Obj
        {
            get { return _Obj; }
            set { _Obj = value; }
        }

        public bool IsSelected
        {
            get { return _IsSelected; }
            set { _IsSelected = value; OnPropertyChanged(); }
        }


        public SelectionItem() { }

    }
}
