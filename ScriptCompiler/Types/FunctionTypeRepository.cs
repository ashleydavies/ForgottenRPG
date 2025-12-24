using System.Collections.Generic;

namespace ScriptCompiler.Types {
    public class FunctionTypeRepository {
        private readonly Dictionary<FunctionReference, (SType returnType, List<SType> @params, bool isExtern)> _repository
            = new();
        
        public void Register(FunctionReference reference, SType returnType, List<SType> parameters, bool isExtern = false) {
            _repository.Add(reference, (returnType, parameters, isExtern));
        }

        public bool IsExtern(FunctionReference reference) {
            return _repository[reference].isExtern;
        }

        public SType ReturnType(FunctionReference reference) {
            return _repository[reference].returnType;
        }

        public List<SType> Parameters(FunctionReference reference) {
            return _repository[reference].@params;
        }
    }
}