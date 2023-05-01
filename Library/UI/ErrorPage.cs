using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lodeon.Terminal.UI
{
    internal class ErrorPage : Page
    {
        private IReadOnlyCollection<Exception> _exceptions;

        public ErrorPage(IReadOnlyCollection<Exception> exceptions)
        {
            _exceptions = exceptions;
        }

        protected override void Load()
        {
            throw new Exception();
        }

        protected override void OnDeselect() {}

        protected override void OnSelect() { }
    }
}
