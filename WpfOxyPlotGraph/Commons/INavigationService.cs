using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WpfOxyPlotGraph.Commons
{
    public interface INavigationService
    {
        void NavigateTo<T>() where T : Page;
        void NavigateTo(string pageName);
        void GoBack();
        void GoForward();
        bool CanGoBack { get; }
    }
}
