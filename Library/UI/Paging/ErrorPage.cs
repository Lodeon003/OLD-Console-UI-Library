//using Lodeon.Terminal.UI.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lodeon.Terminal.UI.Paging
{
    internal class ErrorPage : Page
    {
        private IReadOnlyCollection<Exception> _exceptions;

        public ErrorPage(IReadOnlyCollection<Exception> exceptions)
        {
            _exceptions = exceptions;
        }

        public override void Popup(string title, string text)
        {
            throw new NotImplementedException();
        }

        protected override void OnDeselect() { }

        protected override void OnSelect() { }
    }
}
