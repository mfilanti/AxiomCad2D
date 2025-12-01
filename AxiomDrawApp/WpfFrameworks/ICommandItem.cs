using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.WpfFrameworks
{
    public interface ICommandItem
    {
        void UpdateCanExecute();
    }
}
