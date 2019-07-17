using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using ScriptCompiler.AST;

namespace ScriptCompiler.Types {
    public class UserType : SType {
        private readonly List<(string name, SType type)> _fields;
        private readonly Dictionary<string, int> _fieldOffsets = new Dictionary<string, int>();
        

        public UserType(List<(string, SType)> fields) : base(fields.Sum(tuple => tuple.Item2.Length)) {
            _fields = fields;

            int currOffset = 0;
            foreach ((string name, SType type) in fields) {
                _fieldOffsets[name] = currOffset;
                currOffset += type.Length;
            }
        }

        public SType TypeOfField(string field) => _fields.First(x => x.name == field).type;
        public int OffsetOfField(string field) => _fieldOffsets[field];

        public static UserType FromStruct(StructNode structNode, UserTypeRepository utr) {
            return new UserType(structNode.DeclarationNodes.Select(d => (d.Identifier, d.TypeNode.GetSType(utr))).ToList());
        }
    }
}