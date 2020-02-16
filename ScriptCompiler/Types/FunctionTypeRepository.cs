using System.Collections.Generic;

namespace ScriptCompiler.Types {
    public class FunctionTypeRepository {
        private readonly Dictionary<string, (SType returnType, List<SType> @params)> repository
            = new Dictionary<string, (SType returnType, List<SType> @params)>();
        
        public void Register(string name, SType returnType, List<SType> parameters) {
            repository.Add(name, (returnType, parameters));
        }

        public SType ReturnType(string name) {
            return repository[name].returnType;
        }

        public List<SType> Parameters(string name) {
            return repository[name].@params;
        }
    }
}