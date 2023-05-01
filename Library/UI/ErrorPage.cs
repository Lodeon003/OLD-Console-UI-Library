using Lodeon.Terminal.UI.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lodeon.Terminal.UI
{
    internal class ErrorPage : LayoutPage
    {
        private IReadOnlyCollection<Exception> _exceptions;

        public ErrorPage(IReadOnlyCollection<Exception> exceptions)
        {
            _exceptions = exceptions;
        }

        protected override void OnDeselect() {}

        protected override void OnSelect() { }
    }
}
