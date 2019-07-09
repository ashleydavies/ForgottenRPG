using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;

namespace ScriptCompiler.Types {
    public class UserTypeRepository {
        private readonly Dictionary<string, UserType> _userTypes = new Dictionary<string, UserType>();

        public bool ExistsType(string name) {
            return _userTypes.ContainsKey(name);
        }

        public UserType this[string name] {
            get => _userTypes[name];
            set {
                if (_userTypes.ContainsKey(name))
                    throw new CompileException($"Attempt to redefine type {name}", 0, 0);

                _userTypes[name] = value;
            }
        }
    }
}