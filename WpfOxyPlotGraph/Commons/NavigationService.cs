using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WpfOxyPlotGraph.Commons
{
    public class NavigationService : INavigationService
    {
        private Frame _mainFrame;

        public Frame MainFrame
        {
            set => _mainFrame = value;
        }

        public void NavigateTo<T>() where T : Page
        {
            var page = Activator.CreateInstance<T>();
            _mainFrame.Navigate(page);
        }

        public void NavigateTo(string pageName)
        {
            // 페이지 이름으로 네비게이션 (리플렉션 사용)
            var pageType = Type.GetType($"YourApp.Views.{pageName}");
            if (pageType != null)
            {
                var page = Activator.CreateInstance(pageType) as Page;
                _mainFrame.Navigate(page);
            }
        }

        public void GoBack()
        {
            if (_mainFrame.CanGoBack)
                _mainFrame.GoBack();
        }

        public void GoForward()
        {
            if (_mainFrame.CanGoForward)
                _mainFrame.GoForward();
        }

        public bool CanGoBack => _mainFrame?.CanGoBack ?? false;
        public bool CanGoForward => _mainFrame?.CanGoForward ?? false;
    }
}
