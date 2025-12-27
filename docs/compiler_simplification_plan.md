# Compiler Simplification Proposal

## Goal
Eliminate the intermediate “register machine” pseudo‑assembly pass and emit VM‑ready bytecode (or a minimal direct‑assembly IR) to reduce code size, complexity, and time spent on translation steps.

## Current Issues
- Two models are mixed: a register‑style compiler emits pseudo‑assembly, then the assembler converts into stack‑style VM instructions.
- This creates extra MOV/MEMREAD/MEMWRITE/STACK shuffling, inflating code size and making debugging harder.
- The VM itself already implements a stack machine; the pseudo‑assembly acts like a second, redundant machine.

## Proposed Direction (Direct‑to‑VM)
Replace the pseudo‑assembly layer with a direct bytecode emitter that targets the VM’s stack instructions and existing module format (header + relocation/export tables).

### 1) Define the “Minimal IR”
Keep a tiny internal IR only if needed for:
- Labels and jumps
- Relocations
- Exports
- String/static data sections

No register abstraction. Operands become stack values immediately.

### 2) New Pipeline
Current:
```
AST → CodeGenerator (pseudo‑assembly) → Assembler → VM bytecode
```
Proposed:
```
AST → BytecodeEmitter → VM bytecode
```

### 3) BytecodeEmitter Responsibilities
- Maintain a list of bytecode words.
- Track labels and patch sites.
- Build relocation/export/base‑relocation tables (same format as current Assembler).
- Emit static/data sections with correct page alignment semantics (respecting `MemoryPage.PageSize`).
- Provide helpers for:
  - push literal
  - stack arithmetic
  - comparisons + conditional jumps
  - mem read/write + mem read/write stack
  - native call + extern symbol relocation
  - string pool and static storage

### 4) Stack‑First Expression Generation
Refactor expression generation to use stack directly:
- Each expression leaves its result on the VM stack.
- Binary ops: emit left → emit right → emit op.
- Assignments: compute rhs → write into lvalue memory.
- Avoid register temporaries.

This drops the “stack in memory vs stack in VM” duality.

### 5) LValue & Memory Access
Introduce direct helpers:
- `EmitAddressOf(LValue)` produces address on stack.
- `EmitLoad(LValue)` reads memory into stack.
- `EmitStore(LValue)` pops value and writes into address.

This replaces all `MOV`/`MEMCOPY`/register hopping.

### 6) Function Calls
Adopt a consistent calling convention with stack only:
- Push args (left→right or right→left, choose once).
- Push return address label.
- Jump (or extern jump via relocation).
- Callee writes return value into caller stack location or pushes return value onto VM stack.

If functions currently use the memory stack model (r1), pick one:
- Option A: Use VM stack only for args/returns, and keep memory stack for locals.
- Option B: Use memory stack only, and treat VM stack as expression evaluation only (less ideal).

Recommendation: Use VM stack for expression evaluation and argument passing; keep memory stack for locals with a small set of dedicated ops for local accesses.

### 7) Native Calls
Native calls already operate on the VM stack. With direct emission:
- Evaluate args → VM stack.
- Emit `CALL_NATIVE_EXTERN` (relocation with hash).
- Handle return on VM stack directly.
This removes the “VM stack ↔ memory stack” shuffles.

### 8) Data Section & Strings
Keep the same module layout:
- [0] entry pointer
- [1] static size
- [2] relocation table offset
- [3] export table offset
- [4] base relocation table offset
- statics
- data (strings)
- text (instructions)
- relocation/export/base relocation tables

Emitter should generate the final layout in one step.

## Migration Plan (Incremental)
1) **Add BytecodeEmitter** with label/relocation/export support, using existing instruction enum.
2) **Switch simple statements** (print, literals, simple arithmetic) to direct emission.
3) **Switch LValues + assignments** to direct stack‑based memory access.
4) **Switch function calls** to direct VM conventions.
5) **Replace Assembler usage** in compiler entry points.
6) **Remove old assembly instruction classes** once unused.

## Expected Wins
- Much smaller bytecode for scripts (fewer moves/copies).
- Faster compile time (no assembly parsing/translation).
- Simpler debugging (one machine model).
- Reduced maintenance overhead (single pipeline).

## Open Decisions
- Final calling convention (stack layout for args/return).
- Whether to keep a minimal IR or emit bytecode directly while traversing AST.
- How to handle locals with the least overhead (memory stack vs dedicated opcodes).

## Suggested Next Step
Prototype a BytecodeEmitter that can compile:
- A single `print` statement
- A native call with arguments
- An `if` condition with a label jump

Verify bytecode matches the current VM semantics before broader migration.
