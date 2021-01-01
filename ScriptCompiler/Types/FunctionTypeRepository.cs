using System.Collections.Generic;

namespace ScriptCompiler.Types {
    public class FunctionTypeRepository {
        private readonly Dictionary<FunctionReference, (SType returnType, List<SType> @params)> _repository
            = new();
        
        public void Register(FunctionReference reference, SType returnType, List<SType> parameters) {
            _repository.Add(reference, (returnType, parameters));
        }

        public SType ReturnType(FunctionReference reference) {
            return _repository[reference].returnType;
        }

        public List<SType> Parameters(FunctionReference reference) {
            return _repository[reference].@params;
        }
    }
}