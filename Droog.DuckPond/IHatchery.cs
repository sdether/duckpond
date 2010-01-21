using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Droog.DuckPond {
    public interface IHatchery {
        object Create(object instance, Type type);
    }
}
