using System;
using System.Collections.Generic;
using System.Text;
using Igor.Elixir.AST;

namespace Igor.Elixir.Model
{
    public static class ExModelHelper
    {
        public static ExFile FileOf(this ExModel model, Module astModule)
        {
            return model.File(astModule.exFileName);
        }

        public static ExModule ModuleOf(this ExModel model, Module astModule)
        {
            return model.File(astModule.exFileName).Module(astModule.exName);
        }

        public static ExFile FileOf(this ExModel model, Form astForm)
        {
            return model.FileOf(astForm.Module);
        }
    }
}
